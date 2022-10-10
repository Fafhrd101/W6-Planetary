using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class PrGameInitializer : MonoBehaviour {
    
    public GameObject[] playersPrefabs;
    private PrTopDownCharController[] playersControllers;
    private PrTopDownCharInventory[] playersInventorys;
    public Transform[] playersSpawnPos;
    public bool[] spawnPointFull;
    public PrPlayerSettings playersSettings;
    public PrWeaponList weaponList;
    private string[] weaponNames;

    //pickups
    private GameObject[] pickups;
    private Vector3[] playersDeathPos;
    private bool useLives = false;
    private int[] livesPerPlayer;
    private bool[] playerReachedEndZone;

    //timers
    public bool displayTimer = true;

    [Header("Debug")]
    
    public Mesh areaMesh;
    public Mesh targetArrow;

    // Use this for initialization
    void Start () {
        //Set initialarrays
        playersControllers = new PrTopDownCharController[4];
        playersInventorys = new PrTopDownCharInventory[4];
        spawnPointFull = new bool[playersSpawnPos.Length];

        if (playersSettings)
        {
            if (weaponList)
            {
                weaponNames = new string[weaponList.weapons.Length];
                int index = 0;
                foreach (GameObject w in weaponList.weapons)
                {
                    weaponNames[index] = w.gameObject.GetComponent<PrWeapon>().weaponName;
                    index++;
                }
            }
            pickups = GameObject.FindGameObjectsWithTag("Pickup");
            if (pickups.Length > 0)
            {
                foreach (GameObject pickup in pickups)
                {
                    if (pickup.GetComponent<PrPickupObject>())
                    {
                        pickup.GetComponent<PrPickupObject>().weaponNames = weaponNames;
                        pickup.GetComponent<PrPickupObject>().Initialize();
                    }
                }
            }

            PrItemsSpawner[] itemSpawners = FindObjectsOfType<PrItemsSpawner>();
            if (itemSpawners.Length > 0)
            {
                foreach (PrItemsSpawner itemSpawner in itemSpawners)
                {
                    itemSpawner.weaponNames = weaponNames;
                }
            }
        }
    }
    

    // Update is called once per frame
	void Update () {
        
    }

    void CallEnemiesToStop()
    {
        PrNPCAI[] Enemies = FindObjectsOfType(typeof(PrNPCAI)) as PrNPCAI[];
        if (Enemies.Length != 0)
        {
            foreach (PrNPCAI enemy in Enemies)
            {
                enemy.StopAllActivities();
            }
        }
        // PrNPCSpawner[] spawners = FindObjectsOfType(typeof(PrNPCSpawner)) as PrNPCSpawner[];
        // foreach (var s in spawners)
        // {
        //     s.SpawnerEnabled = false;
        // }

    }
    
    void UpdateLastPlayerPos(int thePlayer)
    {
        playersDeathPos[thePlayer] = playersControllers[thePlayer].transform.position;
            
    }

    void UpdateLastPlayerPosObjective(int thePlayer)
    {
        if (PrPlayerInfo.player1)
        {
            if (playersControllers[thePlayer] != null)
            {
                PrPlayerInfo.player1.lastPlayerPosition = playersControllers[thePlayer].transform.position;
            }
        }
    }

    int RandomNum(int lastRandNum)
    {
        int randNum = Random.Range(0, playersSpawnPos.Length);
        
        return randNum;
    }
    
    void SpawnPlayer(int playerNumber, bool randomPos)
    {
        if (playersSettings.playersInGame[playerNumber])
        {
            int posInt = playerNumber;

            if (randomPos)
            {
                posInt = RandomNum(posInt);
                int tries = 0;
                while (spawnPointFull[posInt] == true && tries < 12)
                {
                    posInt = RandomNum(posInt);
                    tries += 1;
                }

            }
            //set last position if using lives
            Vector3 finalSpawnPos = playersSpawnPos[posInt].position;
            if (useLives && playersDeathPos[playerNumber] != playersSpawnPos[playerNumber].position)
            {
                Debug.Log("aaaa" + playersDeathPos[playerNumber]);
                finalSpawnPos = playersDeathPos[playerNumber];
            }

            Debug.Log(finalSpawnPos);
            //Instantiate player Prefab in Scene
            GameObject tempPlayer = Instantiate(playersPrefabs[playerNumber], finalSpawnPos, playersSpawnPos[posInt].rotation) as GameObject;
            tempPlayer.transform.parent = this.transform;

        }

    }
    
    void OnDrawGizmos()
    {
        if (playersSettings && targetArrow)
        {
            int n = 0;
            foreach (Transform spawnPos in playersSpawnPos)
            {
                Gizmos.color = playersSettings.playerColor[n] * 2;
                Gizmos.DrawMesh(targetArrow, spawnPos.position + Vector3.up, Quaternion.Euler(0, 10, 0), Vector3.one);
                n += 1;

               // Gizmos.color = Color.white;
                Gizmos.DrawMesh(areaMesh, spawnPos.position, Quaternion.Euler(0, 0, 0), Vector3.one);

            }
        }
   
    }

}

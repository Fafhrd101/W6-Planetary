using System.Collections.Generic;
using DunGen.DungeonCrawler;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using Random = UnityEngine.Random;

public class NPC_Spawner : Singleton<NPC_Spawner>
{
    [TextArea] public string note = "We attempt to keep each type within desired amounts," +
                                    "and slow down the loading slightly.";
    
    [Tooltip("Limits the wandering/patrolling guys, but ignores shopkeepers and such.")]
    public int maxNpcCount;
    [Tooltip("Trys to keep this many raiders in the area at once.")]
    public int desiredRaiderCount;
    [Tooltip("Trys to keep this many local police in the area at once.")]
    public int desiredPoliceCount;
    [Tooltip("Trys to keep this many visitors in the area at once.")]
    public int desiredVisitorCount;
    [Tooltip("Trys to keep this many citizens in the area at once.")]
    public int desiredCitizenCount;
    [Space]
    [Header("NPC Settings")]
    public GameObject[] npc;
    private int _npcChosen;

    [Header("Spawner Settings")]
    public int spawnPerCycle = 1;
    public List<Transform> raiderDropZone;
    private float _spawnerRadius = 1.5f;
    private float _spawnRate = 1.0f;
    private float _spawnStartDelay;
    private float _spawnTimer;
	private int _totalSpawned;
    private PlanetaryBaseSetup _setup;
    
    private void Start () 
    {
        _setup = GameObject.FindObjectOfType<PlanetaryBaseSetup>();
    }
    
    public void Update ()
    {
        var currentlySpawned = _setup.totalNpcs.Count;
        if (currentlySpawned >= maxNpcCount) return;
        if (_spawnStartDelay <= 0.0f)
        {
            _spawnTimer += Time.deltaTime;
            if (!(_spawnTimer >= _spawnRate)) return;
            if (_setup.raiders.Count < desiredRaiderCount)
                SpawnRaider();
            if (_setup.visitors.Count < desiredVisitorCount)
                SpawnVisitor();
            if (_setup.citizens.Count < desiredCitizenCount)
                SpawnCitizen();
            if (_setup.police.Count < desiredPoliceCount)
                SpawnPolice();
        }
        else
            _spawnStartDelay -= Time.deltaTime;
    }

    private static Vector3 RandomCircle(Vector3 center, float radius)
    {
        var ang = Random.value * 360;
        Vector3 pos;
        pos.x = center.x + radius * Mathf.Sin(ang * Mathf.Deg2Rad);
        pos.y = center.y ;
        pos.z = center.z + radius * Mathf.Cos(ang * Mathf.Deg2Rad);
        return pos;
    }

    private GameObject SpawnRaider()
	{
        //print("Spawning Raider");
        _npcChosen = 4; // raider model
        var spawnPosition = RandomCircle(_setup.POIs[Random.Range(0, _setup.POIs.Count)].transform.position, _spawnerRadius);
        var rot = Quaternion.FromToRotation(Vector3.forward, transform.position - spawnPosition);
        if (raiderDropZone.Count > 0)
            spawnPosition = raiderDropZone[Random.Range(0, raiderDropZone.Count)].position;
        var npcSpawned = Instantiate(npc[_npcChosen], spawnPosition, rot);
        npcSpawned.GetComponent<PrNPCAI>().faction = ObjectFactory.Instance.Factions[_npcChosen];
        npcSpawned.GetComponent<PrNPCAI>().currentState = PrNPCAI.AIState.Wander;
        npcSpawned.transform.parent = _setup.transform;
        npcSpawned.transform.localScale = Vector3.one;
        npcSpawned.GetComponent<PrNPCAI>().type = PrNPCAI.NpcType.Soldier;
        npcSpawned.name = npcSpawned.GetComponent<PrNPCAI>().type + "_" +npcSpawned.GetComponent<PrNPCAI>().faction ;
        npcSpawned.name = npcSpawned.name.Replace("(Faction)", "");
        npcSpawned.GetComponent<PrNPCAI>().BuildMobLists();
        npcSpawned.GetComponent<PrNPCAI>().lookForPlayer = true;
        npcSpawned.GetComponent<PrNPCAI>().assignedWeapon = npcSpawned.GetComponent<PrNPCAI>().weaponChoices[3]; // rifle
        _setup.raiders.Add(npcSpawned);
        _setup.totalNpcs.Add(npcSpawned);
        return npcSpawned;
    }
    
    public GameObject SpawnMerchant(string storeType, Transform poi, Transform interactPosition)
    {
        //print("Spawning Merchant");
        var npcSpawned = Instantiate(npc[0], poi.position, poi.rotation);
        npcSpawned.GetComponent<PrNPCAI>().faction = ObjectFactory.Instance.Factions[0]; // EA
        npcSpawned.transform.parent = _setup.transform;
        npcSpawned.transform.localScale = Vector3.one;
        npcSpawned.GetComponent<PrNPCAI>().type = PrNPCAI.NpcType.Merchant;
        npcSpawned.GetComponent<PrNPCAI>().currentState = PrNPCAI.AIState.Idle;
        npcSpawned.GetComponent<DialogueActor>().enabled = true;
        switch (storeType)
        {
            case "Bar" or "BlackmarketBar":
                npcSpawned.GetComponent<DialogueActor>().actor = "BarTender";
                npcSpawned.GetComponent<DialogueSystemTrigger>().conversation = "BarTender";
                npcSpawned.GetComponent<PrNPCAI>().work = interactPosition.transform;
                for (var i = 0;i < Random.Range(0,4);i++)
                    SpawnBarPatrons(interactPosition);
                break;
            case "DiplomaticEmbassy":
                npcSpawned.GetComponent<DialogueActor>().actor = "Diplomat";
                npcSpawned.GetComponent<DialogueSystemTrigger>().conversation = "Embassy";
                npcSpawned.GetComponent<PrNPCAI>().work = interactPosition.transform;
                break;
            case "ScientificLab":
                npcSpawned.GetComponent<DialogueActor>().actor = "Scientist";
                npcSpawned.GetComponent<DialogueSystemTrigger>().conversation = "Science";
                npcSpawned.GetComponent<PrNPCAI>().work = interactPosition.transform;
                break;
            case "MedicalClinic":
                npcSpawned.GetComponent<DialogueActor>().actor = "Doctor";
                npcSpawned.GetComponent<DialogueSystemTrigger>().conversation = "Clinic";
                npcSpawned.GetComponent<PrNPCAI>().work = interactPosition.transform;
                break;
            default:
                npcSpawned.GetComponent<DialogueActor>().actor = storeType;
                npcSpawned.GetComponent<DialogueSystemTrigger>().conversation = "ShopKeeper";
                npcSpawned.GetComponent<BarkOnIdle>().conversation = "ShopKeeperBarks";
                npcSpawned.GetComponent<PrNPCAI>().work = poi.transform;
                break;
        }

        npcSpawned.GetComponent<PrNPCAI>().doNotAttackTarget = true;
        npcSpawned.name = storeType;
        npcSpawned.GetComponent<PrNPCAI>().assignedWeapon = npcSpawned.GetComponent<PrNPCAI>().weaponChoices[1]; // melee
        _setup.merchants.Add(npcSpawned);
        _setup.totalShops.Add(npcSpawned.transform);
        _setup.totalNpcs.Add(npcSpawned);
        return npcSpawned;
    }

    private GameObject SpawnBarPatrons(Transform interactPosition)
    {
        //print("Spawning BarPatron");
        var i = Random.Range(0, npc.Length - 1);
        var finalSpawnPosition = RandomCircle(interactPosition.position, 3);
        var rot = Quaternion.FromToRotation(Vector3.forward, interactPosition.position - finalSpawnPosition);
        var npcSpawned = Instantiate(npc[i], finalSpawnPosition, rot);
        npcSpawned.GetComponent<PrNPCAI>().faction = ObjectFactory.Instance.Factions[0]; // EA
        npcSpawned.transform.parent = _setup.transform;
        npcSpawned.transform.localScale = Vector3.one;
        npcSpawned.GetComponent<PrNPCAI>().type = PrNPCAI.NpcType.Visitor;
        npcSpawned.GetComponent<PrNPCAI>().currentState = PrNPCAI.AIState.Static;
        npcSpawned.name = "Barpatron"+ "_" +npcSpawned.GetComponent<PrNPCAI>().faction;
        npcSpawned.name = npcSpawned.name.Replace("(Faction)", "");
        npcSpawned.GetComponent<BarkOnIdle>().conversation = "BarpatronBarks";
        npcSpawned.GetComponent<PrNPCAI>().assignedWeapon = npcSpawned.GetComponent<PrNPCAI>().weaponChoices[0]; // none
        _setup.totalNpcs.Add(npcSpawned);
        return npcSpawned;
    }

    private GameObject SpawnCitizen()
    {
        //print("Spawning Citizen");
        var pos = _setup.POIs[Random.Range(0, _setup.POIs.Count)].transform;
        var npcSpawned = Instantiate(npc[0], pos.position, pos.rotation);
        npcSpawned.GetComponent<PrNPCAI>().faction = ObjectFactory.Instance.Factions[0]; // EA
        npcSpawned.transform.parent = _setup.transform;
        npcSpawned.transform.localScale = Vector3.one;
        npcSpawned.GetComponent<PrNPCAI>().type = PrNPCAI.NpcType.Citizen;
        npcSpawned.GetComponent<PrNPCAI>().currentState = PrNPCAI.AIState.Wander;
        npcSpawned.name = "Citizen"+ "_" +npcSpawned.GetComponent<PrNPCAI>().faction;
        npcSpawned.name = npcSpawned.name.Replace("(Faction)", "");
        npcSpawned.GetComponent<PrNPCAI>().assignedWeapon = npcSpawned.GetComponent<PrNPCAI>().weaponChoices[0]; // none
        npcSpawned.GetComponent<PrNPCAI>().doNotAttackTarget = true;
        _setup.citizens.Add(npcSpawned);
        _setup.totalNpcs.Add(npcSpawned);
        return npcSpawned;
    }

    // Only called on demand
    public GameObject SpawnWorker(Transform trans)
    {
        //print("Spawning Worker");
        var npcSpawned = Instantiate(npc[0], trans, false);
        npcSpawned.GetComponent<PrNPCAI>().faction = ObjectFactory.Instance.Factions[0]; // EA
        npcSpawned.transform.localScale = Vector3.one;
        npcSpawned.GetComponent<PrNPCAI>().type = PrNPCAI.NpcType.Worker;
        npcSpawned.GetComponent<PrNPCAI>().currentState = PrNPCAI.AIState.Working;
        npcSpawned.name = "Worker"+ "_" +npcSpawned.GetComponent<PrNPCAI>().subType;
        npcSpawned.GetComponent<PrNPCAI>().assignedWeapon = npcSpawned.GetComponent<PrNPCAI>().weaponChoices[0]; // none
        npcSpawned.GetComponent<PrNPCAI>().doNotAttackTarget = true;
        _setup.workers.Add(npcSpawned);
        _setup.totalNpcs.Add(npcSpawned);
        return npcSpawned;
    }

    private GameObject SpawnVisitor()
    {
        //print("Spawning Visitor");
        var i = Random.Range(0, npc.Length - 1);
        var pos = _setup.POIs[Random.Range(0, _setup.POIs.Count)].transform;
        var npcSpawned = Instantiate(npc[i], pos.position, pos.rotation);
        npcSpawned.GetComponent<PrNPCAI>().faction = ObjectFactory.Instance.Factions[i];
        npcSpawned.transform.parent = _setup.transform;
        npcSpawned.transform.localScale = Vector3.one;
        npcSpawned.GetComponent<PrNPCAI>().type = PrNPCAI.NpcType.Visitor;
        npcSpawned.GetComponent<PrNPCAI>().currentState = PrNPCAI.AIState.Wander;
        npcSpawned.name = npcSpawned.GetComponent<PrNPCAI>().type + "_" +
                          npcSpawned.GetComponent<PrNPCAI>().faction;
        npcSpawned.name = npcSpawned.name.Replace("(Faction)", "");
        npcSpawned.GetComponent<PrNPCAI>().assignedWeapon = npcSpawned.GetComponent<PrNPCAI>().weaponChoices[2]; // pistol
        _setup.visitors.Add(npcSpawned);
        _setup.totalNpcs.Add(npcSpawned);
        return npcSpawned;
    }

    private GameObject SpawnPolice()
    {
        //print("Spawning Police");
        var pos = _setup.POIs[Random.Range(0, _setup.POIs.Count)].transform;
        var npcSpawned = Instantiate(npc[0], pos.position, pos.rotation);
        npcSpawned.GetComponent<PrNPCAI>().faction = ObjectFactory.Instance.Factions[0]; // EA
        npcSpawned.transform.parent = _setup.transform;
        npcSpawned.transform.localScale = Vector3.one;
        npcSpawned.GetComponent<PrNPCAI>().type = PrNPCAI.NpcType.Soldier;
        npcSpawned.GetComponent<PrNPCAI>().currentState = PrNPCAI.AIState.Patrol;
        npcSpawned.name = "Police"+ "_" +npcSpawned.GetComponent<PrNPCAI>().faction;
        npcSpawned.name = npcSpawned.name.Replace("(Faction)", "");
        npcSpawned.GetComponent<PrNPCAI>().assignedWeapon = npcSpawned.GetComponent<PrNPCAI>().weaponChoices[3]; // rifle
        _setup.police.Add(npcSpawned);
        _setup.totalNpcs.Add(npcSpawned);
        return npcSpawned;
    }
}

//using System;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Jumpgate : MonoBehaviour {
    // Unique in-game object ID 
    [Tooltip("DO NOT touch this if you don't have to")]
    public string id;
    public Vector2 NextSector;
    public GameObject[] DockWaypoints;
    public Transform SpawnPos;
    public AnimationCurve cameraFovCurve;
    public bool loopingGate = false;
    public TMP_Text signA;
    public TMP_Text signB;
    
    public List<JumpSequence> jumps;
    public List<GameObject> queuedJumpers;
    
    public class JumpSequence
    {
        private float accelTime = 1f;
        public float Timer;
        public GameObject Ship;
        public float Speed = 10f;

        public JumpSequence(GameObject ship)
        {
            Timer = accelTime;
            Ship = ship;
            ship.GetComponent<Rigidbody>().isKinematic = true;
            ship.GetComponent<ShipPhysics>().enabled = false;
        }
    }

    private void Start()
    {
        jumps = new List<JumpSequence>();
        if (!loopingGate)
        {
            signA.text = "X " + NextSector.x + " Y " + NextSector.y;
            signB.text = signA.text;
        }
        else
        {
            NextSector = SectorNavigation.CurrentSector;
        }
        Player.Instance.currentSector = SectorNavigation.CurrentSector;
    }

    private void OnEnable()
    {
        if (signA != null)
            signA.alpha = signB.alpha = 1;
        if (GetComponent<ShipSpawner>())
            GetComponent<ShipSpawner>().enabled = true;
    }

    private void OnDisable()
    {
        if (signA != null)
            signA.alpha = signB.alpha = 0;
        if (GetComponent<ShipSpawner>())
            GetComponent<ShipSpawner>().enabled = false;
    }

    private void Update()
    {
        if (Ship.PlayerShip == null) return;
        for(int i=0; i<jumps.Count; i++)
        {
            if (jumps[i].Timer < 0)
            {
                if (ParticleController.Instance != null)
                    ParticleController.Instance.CreateJumpFlashAtPos(jumps[i].Ship.transform.position);
                if (jumps[i].Ship == Ship.PlayerShip.gameObject) {
                    //print("Player listed as jump queue #"+i);
                    jumps[i].Ship.GetComponent<Rigidbody>().isKinematic = false;
                    jumps[i].Ship.GetComponent<ShipPhysics>().enabled = true;
                    jumps[i].Ship.transform.position = Vector3.zero;
                    jumps[i].Ship.transform.rotation = quaternion.identity;
                    Player.Instance.currentSector = NextSector;
                    //print("Saving and jumping---------------");
                    SaveGame.SaveAndJump(NextSector);
                    SceneManager.LoadScene("EmptyFlight");
                }
                else
                {
                    GameObject.Destroy(jumps[i].Ship);
                    jumps.RemoveAt(i);
                    queuedJumpers.RemoveAt(i);
                    i--;
                }
                
            }
            else { 
                jumps[i].Timer -= Time.deltaTime;
                jumps[i].Ship.transform.position -= SpawnPos.forward * jumps[i].Speed;
                jumps[i].Speed += 10;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Ship.PlayerShip.isDestroyed)
            return;
        
        if(other.gameObject.CompareTag("Ship"))
        {    
            // NPC
            if (!AlreadyJumping(other.gameObject) && !other.gameObject.GetComponent<Ship>().isPlayerControlled)
            {
                jumps.Add(new JumpSequence(other.gameObject));
                queuedJumpers.Add(other.gameObject);
            }
            
            // Player
            if (other.gameObject.GetComponent<Ship>() == Ship.PlayerShip && !AlreadyJumping(other.gameObject))
            {
                if (loopingGate)
                {
                    TextFlash.ShowYellowText("This gate is restricted to merchants only.");
                    return;
                }

                //print("Player entered jump queue of "+gameObject.name);
                if (Camera.main is not null)
                    Camera.main.GetComponent<CameraController>().SetTargetStation(null, Vector3.zero);
                CanvasViewController.Instance.Hud.SetActive(false);
                // Start FOV animation
                if (jumps.Count > 0)
                    StartCoroutine(AnimateFOV(jumps.Count));
                jumps.Add(new JumpSequence(other.gameObject));
                queuedJumpers.Add(other.gameObject);
            }
        }
    }

    private IEnumerator AnimateFOV(int playerJumpIndex)
    {
        //print("index="+playerJumpIndex+" Count="+jumps.Count);
        float timer = jumps[0/*playerJumpIndex*/].Timer;
        if (Camera.main is not null)
        {
            float initialFov = Camera.main.fieldOfView;

            while(timer > 0) {
                timer = jumps[0/*playerJumpIndex*/].Timer;
                Camera.main.fieldOfView = Mathf.Lerp(initialFov, 170f, cameraFovCurve.Evaluate(1f-timer));
                yield return null;
            }
        }

        yield return null;
    }

    // Check if this ship has already been added to the jump queue (for ships with multiple colliders)
    private bool AlreadyJumping(GameObject ship)
    {
        if (jumps == null) return false;
        return jumps.Any(js => js.Ship == ship);
    }

    #region placementCode
    public void GenerateJumpgate()
    {
        id = "JG-" + GenerateRandomSector.RandomString(4);
        // if (keplarRegion == 0)
        //     return;
        // Vector2 point = GetPointOnRing();
        // float y = Random.Range(-250, 250);
        // this.transform.position = new Vector3(point.x, y, point.y); 
    }
    public void ClearJumpgate(){}
    // private Vector2 GetPointOnRing()
    // {
    //     float innerRange = keplarRegion * 750 + 100;
    //     float outerRange = keplarRegion * 750 + 250;
    //     Vector2 v = Random.insideUnitCircle;
    //     return v.normalized * innerRange + v*(outerRange - innerRange);
    // }
    #endregion
}

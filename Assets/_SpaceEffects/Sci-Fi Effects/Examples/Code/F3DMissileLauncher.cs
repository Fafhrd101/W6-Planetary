using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace FORGE3D
{
    public class F3DMissileLauncher : MonoBehaviour
    {
        public Transform missilePrefab;
        public Transform target;
        public Transform[] socket;
        public Transform explosionPrefab;
        public ShipEquipment shipEQ;
        public Collider shipCollider;
        public Collider shipShields;

        public AudioSource missileFire;
        
        // Spawns explosion
        public void SpawnExplosion(Vector3 position)
        {
            // F3DPoolManager.Pools["GeneratedPool"]
            //     .Spawn(explosionPrefab, position, Quaternion.identity, null);
            Instantiate(explosionPrefab, position, Quaternion.identity, null);
        }
        
        // Processes input for launching missile
        private void ProcessInput()
        {
            if (Ship.IsShipInputDisabled)
                return;
            
            if (Ship.PlayerShip.stationDocked != "none")
                return;
            
            if (Input.GetMouseButtonDown(1))
            {
                if (shipEQ.curMissiles <= 0)
                    return;
                
                //print("input processed");
                if (target != null)
                {
                    var randomSocketId = Random.Range(0, socket.Length);
                    var tMissile = GameObject.Instantiate(missilePrefab,
                        socket[randomSocketId].position, socket[randomSocketId].rotation, null);
                    if (tMissile != null)
                    {
                        var missile = tMissile.GetComponent<F3DMissile>();

                        missile.launcher = this;

                        missile.target = target;
                        missile.missileType = F3DMissile.MissileType.Predictive;
                        missile.firedBy = shipEQ.ship.transform;
                        missile.playerShot = true;
                        missileFire.volume = MusicController.Instance.globalSoundVolume;
                        missileFire.Play();
                    }
                    else Debug.LogError("unable to load missile");
                }
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu")
                return;
            if (InputHandler.Instance.SelectedObject != null)
                target = InputHandler.Instance.SelectedObject.transform;
            ProcessInput();
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using SpaceSimFramework.Code.UI.HUD;

public class PlanetStarport : MonoBehaviour {

    [Tooltip("Waypoints for docking small ships, from farthest to closest")]
    public GameObject waypointHolder;
    public GameObject[] dockWaypoints;
    public Planet planetController;
    [Tooltip("Time in sec after which the docking will be cancelled")]
    public float dockingTimeLimitSec = 60f;
    public float dockTimer;

    [HideInInspector]
    public Queue<GameObject> dockingQueue;
    public GameObject ShipDocking => _shipDocking;
    private GameObject _shipDocking;

    private void Awake()
    {
        dockingQueue = new Queue<GameObject>();
        if (planetController == null)
        {
            planetController = GetComponentInParent<Planet>();
        }
        dockWaypoints = new GameObject[waypointHolder.transform.childCount];
        for (int i=0; i<waypointHolder.transform.childCount; i++) {
            dockWaypoints[i]=waypointHolder.transform.GetChild(i).gameObject;
        }
    }

    private void Update()
    {
        if (_shipDocking == null)
        {
            if(dockingQueue.Count == 0) // No ship docking or waiting to dock
                return;

            _shipDocking = dockingQueue.Dequeue();  // Next ship can now start docking
            dockTimer = dockingTimeLimitSec;   // Reset docking timer
            //foreach (var waypoint in DockWaypoints)
            //    waypoint.GetComponentInChildren<MeshRenderer>().enabled = true;
        }

        if (dockTimer < 0)
        {
            // Docking expired
            Ship ship = _shipDocking.GetComponent<Ship>();
            ship.AIInput.FinishOrder();
            if(ship.faction == Player.Instance.playerFaction)
            {
                ConsoleOutput.PostMessage("[" + name + " Approach Control]: Docking time expired, docking denied.", Color.yellow);
            }

            _shipDocking = null;
            // Disable waypoint indicators
            foreach (var waypoint in dockWaypoints)
                waypoint.GetComponentInChildren<MeshRenderer>().enabled = false;
        }
        else
        {
            dockTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // if (_shipDocking != null && other.gameObject == _shipDocking.gameObject)
        // {
        //     PlanetController.OnDockContact(other);
        //
        //     _shipDocking = null;
        //     // Destroy waypoint indicators
        //     foreach (var waypoint in DockWaypoints)
        //         waypoint.GetComponent<MeshRenderer>().enabled = false;
        // }

        // if (other.gameObject.GetComponent<Ship>() != null)
        // {
        //     PlanetController.OnDockContact(other);
        //
        //     // Destroy waypoint indicators
        //     foreach (var waypoint in DockWaypoints)
        //         waypoint.GetComponentInChildren<MeshRenderer>().enabled = false;      
        // }
    }

    public bool CanProceedWithDocking(GameObject ship)
    {
        return _shipDocking == ship;
    }
}

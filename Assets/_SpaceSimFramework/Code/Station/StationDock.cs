using UnityEngine;
using System.Collections.Generic;
using SpaceSimFramework.Code.UI.HUD;

public class StationDock : MonoBehaviour
{

    [Tooltip("Waypoints for docking small ships, from farthest to closest")]
    public GameObject waypointHolder;
    public GameObject[] DockWaypoints;
    public Station StationController;
    [Tooltip("Time in sec after which the docking will be cancelled")]
    public float DockingTimeLimitSec = 60f;
    public float _dockTimer;

    [HideInInspector]
    public Queue<GameObject> DockingQueue;
    public GameObject ShipDocking
    {
        get { return _shipDocking; }
    }
    public GameObject _shipDocking;

    private void Awake()
    {
        DockingQueue = new Queue<GameObject>();
        if (StationController == null)
        {
            StationController = GetComponentInParent<Station>();
        }
        GameObject[] DockWaypoints = new GameObject[waypointHolder.transform.childCount];
        for (int i=0; i<waypointHolder.transform.childCount; i++) {
            DockWaypoints[i]=waypointHolder.transform.GetChild(i).gameObject;
        }
        // foreach (var waypoint in DockWaypoints)
        //     waypoint.GetComponentInChildren<MeshRenderer>().enabled = false;
    }

    private void Update()
    {
        if (_shipDocking == null)
        {
            if(DockingQueue.Count == 0) // No ship docking or waiting to dock
                return;

            _shipDocking = DockingQueue.Dequeue();  // Next ship can now start docking
            _dockTimer = DockingTimeLimitSec;   // Reset docking timer
            //foreach (var waypoint in DockWaypoints)
            //    waypoint.GetComponentInChildren<MeshRenderer>().enabled = true;
        }

        if (_dockTimer < 0)
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
            foreach (var waypoint in DockWaypoints)
                waypoint.GetComponentInChildren<MeshRenderer>().enabled = false;
        }
        else
        {
            _dockTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
         if (StationController.dockHangerDoor != null)
            StationController.dockHangerDoor.OpenDoor();

         if (_shipDocking == null || other.gameObject != _shipDocking.gameObject) return;

         StationController.OnDockContact(other);
        _shipDocking = null;
        // Destroy waypoint indicators
        foreach (var waypoint in DockWaypoints)
            waypoint.GetComponentInChildren<MeshRenderer>().enabled = false;

        //print("ship docking");
    }

    public bool CanProceedWithDocking(GameObject ship)
    {
        return _shipDocking == ship;
    }
}

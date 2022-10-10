using UnityEngine;

public class StationMooring : MonoBehaviour {

    public Station StationController;
    public GameObject[] Waypoints;
    public GameObject TractorBeam;
    
    [HideInInspector]
    public GameObject ship {
        get
        {
            return _ship;
        }
        set
        {
            _ship = value;
            // Light up the docking waypoints
            if(value != null)
            {
                //foreach (var waypoint in Waypoints)
                //    waypoint.GetComponentInChildren<MeshRenderer>().enabled = true;
            }
        }
    }

    public GameObject _ship;

    private void OnTriggerEnter(Collider other)
    {
        //print("Mooring trigger entered by " + other.gameObject);
        if (StationController != null)
        {
            //print("checkpoint 1");
            if (other.GetComponent<Ship>() == Ship.PlayerShip && other.GetComponent<Ship>().AIInput.currentOrder == Order.ShipOrder.Dock)
            {
                //print("checkpoint 2");
                TractorBeam.SetActive(false);
                ship = null;
                if (other.gameObject.GetComponent<Ship>() != null)
                {
                    StationController.OnMooringContact(this, other.gameObject);
                    other.gameObject.transform.position = transform.position;
                    other.gameObject.transform.rotation = transform.rotation;
                }
                // Turn off the docking waypoints
                // foreach (var waypoint in Waypoints)
                //     waypoint.GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }
    }
}

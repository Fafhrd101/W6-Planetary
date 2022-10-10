using UnityEngine;

public class ThrusterVisuals : MonoBehaviour
{
    public GameObject[] trails;
    private Ship ship;
    private ShipPlayerInput shipInput;
 
    public void Start()
    {
        ship = GetComponent<Ship>();
        shipInput = GetComponent<ShipPlayerInput>();
        
        if (GetComponent<Ship>() != Ship.PlayerShip)
            foreach (var trail in trails)
            {
                trail.GetComponent<TrailRenderer>().colorGradient = ship.faction.trailColoring;
            }
        
    }
    public void Update()
    {
        if (ship == null)
            return;

        // Turn off if no key is being pushed. Thrusters off, we're drifting.
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            foreach (var trail in trails)
            {
                trail.gameObject.SetActive(false);
            }
        else
        {
            if (GetComponent<Ship>() == Ship.PlayerShip)
            {
                foreach (var trail in trails)
                {
                    if (trail != null && trail.GetComponent<Thruster>() != null)
                        trail.GetComponent<Thruster>().thrusterLength = 8.5f + shipInput.throttle * 5;

                    trail.gameObject.SetActive(true);
                }
            }
            else
                foreach (var trail in trails)
                {
                    trail.gameObject.SetActive(true);
                    trail.GetComponent<TrailRenderer>().time = 1f + shipInput.throttle * 2;
                }
        }
    }
}

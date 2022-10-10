using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementThrusters : MonoBehaviour
{
    public Thruster Thruster_Left;
    public Thruster Thruster_Right;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Ship.PlayerShip.isDestroyed)
            return;
        
        if (Ship.PlayerShip.PlayerInput.strafe > 0.3)
        {
            Thruster_Left.transform.localRotation = Quaternion.Euler (0,-90,0);
            Thruster_Left.gameObject.SetActive(true);
            Thruster_Left.thrusterLength = 3;
        }
        else if (Ship.PlayerShip.PlayerInput.strafe < -0.3)
        {
            Thruster_Right.transform.localRotation = Quaternion.Euler (0,90,0);
            Thruster_Right.gameObject.SetActive(true);
            Thruster_Right.thrusterLength = 3;
        }
        else if (Ship.PlayerShip.PlayerInput.throttle < 0)
        {
            Thruster_Left.transform.localRotation = Quaternion.Euler (0,0,0);
            Thruster_Right.transform.localRotation = Quaternion.Euler (0,0,0);
            Thruster_Right.gameObject.SetActive(true);  
            Thruster_Left.gameObject.SetActive(true);
            Thruster_Left.thrusterLength = 10;
            Thruster_Right.thrusterLength = 10;
        }
        else if (Ship.PlayerShip.PlayerInput.roll > 0.3)
        {
            Thruster_Left.transform.localRotation = Quaternion.Euler (-90,0,0);
            Thruster_Left.gameObject.SetActive(true);
            Thruster_Left.thrusterLength = 3;
        }
        else if (Ship.PlayerShip.PlayerInput.roll < -0.3)
        {
            Thruster_Right.transform.localRotation = Quaternion.Euler (-90,0,0);
            Thruster_Right.gameObject.SetActive(true);
            Thruster_Right.thrusterLength = 3;
        }
        else if (Ship.PlayerShip.PlayerInput.yaw > 0.3)
        {
            Thruster_Left.transform.localRotation = Quaternion.Euler (0,-90,0);
            Thruster_Left.gameObject.SetActive(true);
            Thruster_Left.thrusterLength = 3;
        }
        else if (Ship.PlayerShip.PlayerInput.yaw < -0.3)
        {
            Thruster_Right.transform.localRotation = Quaternion.Euler (0,90,0);
            Thruster_Right.gameObject.SetActive(true);
            Thruster_Right.thrusterLength = 3;
        }
        else if (Ship.PlayerShip.PlayerInput.pitch > 0.3)
        {
            Thruster_Left.transform.localRotation = Quaternion.Euler (-90,0,0);
            Thruster_Left.gameObject.SetActive(true);
            Thruster_Left.thrusterLength = 3;
            Thruster_Right.transform.localRotation = Quaternion.Euler (-90,0,0);
            Thruster_Right.gameObject.SetActive(true);
            Thruster_Right.thrusterLength = 3;
        }
        else if (Ship.PlayerShip.PlayerInput.pitch < 0.3)
        {
            Thruster_Left.transform.localRotation = Quaternion.Euler (90,0,0);
            Thruster_Left.gameObject.SetActive(true);
            Thruster_Left.thrusterLength = 4;
            Thruster_Right.transform.localRotation = Quaternion.Euler (90,0,0);
            Thruster_Right.gameObject.SetActive(true);
            Thruster_Right.thrusterLength = 4;
        }
        else
        {
            Thruster_Left.gameObject.SetActive(false);
            Thruster_Right.gameObject.SetActive(false);
        }
    }
}

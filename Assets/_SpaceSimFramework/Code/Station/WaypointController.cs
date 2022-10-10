using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointController : MonoBehaviour
{
    public GameObject TractorBeam;
    public bool toggleTractorBeam = false;
    public bool releaseAI = false;
    private bool Status = false;
    public void Start(){}
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponentInParent<ShipAI>() != null)
        {
            if (toggleTractorBeam)
            {
                if (other.gameObject.GetComponentInParent<ShipAI>().currentOrder == Order.ShipOrder.Dock)
                {
                    TractorBeam.SetActive(!Status);
                    TractorBeam.GetComponent<Tractor_Beam>().beamTarget = Ship.PlayerShip.gameObject;
                    Ship.PlayerShip.controllingTractorBeam = TractorBeam;
                }
            } else if (releaseAI)
            {
                other.gameObject.GetComponentInParent<ShipAI>().FinishOrder();
                TractorBeam.SetActive(false);
                Ship.PlayerShip.controllingTractorBeam = null;
            }
        }
        else if (other.gameObject.GetComponent<ShipAI>() != null)
        {
            if (toggleTractorBeam)
            {
                if (other.gameObject.GetComponent<ShipAI>().currentOrder == Order.ShipOrder.Dock)
                {
                    TractorBeam.SetActive(!Status);
                    TractorBeam.GetComponent<Tractor_Beam>().beamTarget = Ship.PlayerShip.gameObject;
                } else if (releaseAI)
                {
                    other.gameObject.GetComponent<ShipAI>().FinishOrder();
                    TractorBeam.SetActive(false);
                    Ship.PlayerShip.controllingTractorBeam = null;
                }
            }
        }
    }
}

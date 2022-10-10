using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldCode : MonoBehaviour
{
    public GameObject shield;
    private bool active = false;

    public void Start()
    {
        shield.SetActive(false);
    }
    
    public void Action()
    {
        active = !active;
        shield.SetActive(active);
    }
}

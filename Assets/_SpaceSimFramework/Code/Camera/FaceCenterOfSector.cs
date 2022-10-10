using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCenterOfSector : MonoBehaviour
{
    private GameObject sun;
    
    // Start is called before the first frame update
    void Start()
    {
        sun = GameObject.FindGameObjectWithTag("Sun");
    }

    public void Update() {
        transform.rotation = sun.transform.rotation;
    }
}

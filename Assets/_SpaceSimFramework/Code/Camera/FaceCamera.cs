using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Update() {
        transform.rotation = Camera.main.transform.rotation;
    }
}

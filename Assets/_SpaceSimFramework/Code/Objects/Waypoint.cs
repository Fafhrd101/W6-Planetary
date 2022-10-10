using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {

    public bool destroyOnArrival = false;
    public Light light1;
    public Transform distantObject;
    public float distanceToObject;
    public List<Waypoint> neighbors;

    private void Start () 
    {
        if (light1 == null)
            light1 = this.gameObject.GetComponentInChildren<Light>();
        if (light1 != null)
        {
            light1.intensity = 1;
            light1.range = 1;
        }
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if (light1 != null)
        {
            light1.intensity = 3;
            light1.range = 10;
        }

        if (destroyOnArrival)
            GameObject.Destroy(this.gameObject);
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (light1 != null)
        {
            light1.intensity = 1;
            light1.range = 1;
        }
    }

    // public void OnDrawGizmos()
    // {
    //     if (neighbors == null)
    //         return;
    //     Gizmos.color = new Color(255f, 255f, 0f);
    //     foreach (var neighbor in neighbors)
    //     {
    //         if (neighbor != null)
    //             Gizmos.DrawLine(transform.position, neighbor.transform.position);
    //     }
    // }
    public void PerformCalcs()
    {
        if (distantObject != null)
            distanceToObject = Vector3.Distance(transform.position, distantObject.position);
    }
}

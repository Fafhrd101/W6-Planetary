using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDLag : MonoBehaviour {

    private Vector3 prevRotation, rotation;
    private int frameCount;
    private Transform target;

    public float TurningRate = 80f;

    private void Awake()
    {
        target = Camera.main.transform;
    }

    // Update and Lateupdate causes jitter with rotation
    // FixedUpdate causes sporadic  jitter along the movement axis
    private void FixedUpdate()
    {
        // Turn towards our target rotation.
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, TurningRate * Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Copy position
        transform.position = target.position;
    }
}

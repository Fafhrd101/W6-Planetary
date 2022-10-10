using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraScroll : MonoBehaviour
{
    public bool ReverseInputAxis;
    public float PositionLerp = 1f;
    private Vector3 scrollOffset;
    private Vector3 cameraOrigin;

    // Use this for initialization
    private void Awake()
    {
        cameraOrigin = transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        var mult = 1;
        if (ReverseInputAxis) mult = -1;

        scrollOffset.x = Mathf.Clamp(scrollOffset.x + Input.GetAxis("Mouse X") * 0.01f * mult, -1f, 1f);
        scrollOffset.z = Mathf.Clamp(scrollOffset.z + Input.GetAxis("Mouse Y") * 0.01f * mult, -1f, 1f);
        scrollOffset = Vector3.ClampMagnitude(scrollOffset, 0.1f);
        transform.position = Vector3.Lerp(transform.position, cameraOrigin + scrollOffset, Time.deltaTime * PositionLerp);
    }
}
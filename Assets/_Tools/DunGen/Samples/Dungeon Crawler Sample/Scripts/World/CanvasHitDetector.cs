using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasHitDetector : MonoBehaviour {
    private GraphicRaycaster _graphicRaycaster;
    public bool ui;
    public bool freeze = false;
    private void Start()
    {
        // This instance is needed to compare between UI interactions and
        // game interactions with the mouse.
        _graphicRaycaster = GetComponent<GraphicRaycaster>();
    }

    private void Update()
    {
        if (!freeze)
            ui = IsPointerOverUI();
    }

    private bool IsPointerOverUI()
    {
        // Obtain the current mouse position.
        var mousePosition = Input.mousePosition;

        // Create a pointer event data structure with the current mouse position.
        var pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = mousePosition;

        // Use the GraphicRaycaster instance to determine how many UI items
        // the pointer event hits.  If this value is greater-than zero, skip
        // further processing.
        var results = new List<RaycastResult>();
        _graphicRaycaster.Raycast(pointerEventData, results);
        return results.Count > 0;
    }
}
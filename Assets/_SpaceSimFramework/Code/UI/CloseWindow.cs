﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseWindow : MonoBehaviour {

    public void OnCloseWindow()
    {
        CanvasController.Instance.CloseMenu();
    }
	
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CanvasController.Instance.CloseMenu();
            Ship.IsShipInputDisabled = false;
            Cursor.visible = false;
            Ship.PlayerShip.UsingMouseInput = true;
        }
    }

}

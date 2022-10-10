using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComputerMenuController : MonoBehaviour
{
    private Ship ship;
    private Station station;
    public AudioClip soundEffect;
    public Image computerScreenHolder;
    public Sprite errorScreen;
    public Sprite bootingScreen;
    public void OnEnable()
    {
                    Ship.IsShipInputDisabled = true;
        Cursor.visible = true;
        Ship.PlayerShip.UsingMouseInput = false;
        computerScreenHolder.sprite = errorScreen;
    }

    public void buttonCloseMenu()
    {
        // Ship.IsShipInputDisabled = false;
        // Cursor.visible = false;
        // Ship.PlayerShip.UsingMouseInput = true;
        station.OpenStationMenu(ship.gameObject);
        Destroy(this.gameObject);
    }

    public void buttonError()
    {
        computerScreenHolder.sprite = bootingScreen;
        Invoke("Reboot",2);
    }

    void Reboot()
    {
        computerScreenHolder.sprite = errorScreen;   
        MusicController.Instance.PlaySound(soundEffect);
    }
    
    public void PopulateMenu(GameObject shipLandedAtStation, Station shipStation)
    {
        station = shipStation;
        ship = shipLandedAtStation.GetComponent<Ship>();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

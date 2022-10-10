using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpMenuDialog : MonoBehaviour
{
    private int pageNum = 1;

    public GameObject page1;
    public GameObject page2;
    public GameObject page3;
    public Toggle toggle;
    
    public void OnEnable()
    {
        pageNum = 1;
        page1.SetActive(true);
        page2.SetActive(false);
        page3.SetActive(false);
                    Ship.IsShipInputDisabled = true;
        Cursor.visible = true;
        Ship.PlayerShip.UsingMouseInput = false;
    }

    public void buttonCloseMenu()
    {
        //PlayerPrefs.SetInt("helpScreenDone", 1);
        PlayerPrefs.SetInt("helpScreenDone", toggle.isOn ? 1 : 0);
        
        Ship.IsShipInputDisabled = false;
        Cursor.visible = false;
        Ship.PlayerShip.UsingMouseInput = true;
        IngameMenuController.Instance.closeMenu();
        Destroy(this.gameObject);
    }

    public void buttonNextPage()
    {
        if (pageNum == 1)
        {
            page1.SetActive(false);
            page2.SetActive(true);
            page3.SetActive(false);
            pageNum = 2;
        } 
        else if (pageNum == 2)
        {
            page1.SetActive(false);
            page2.SetActive(false);
            page3.SetActive(true);
            pageNum = 3;
        } 
        else //if (pageNum == 3)
        {
            page1.SetActive(true);
            page2.SetActive(false);
            page3.SetActive(false);
            pageNum = 1;
        } 
    }
    // Update is called once per frame
    void Update()
    {
        if (!Ship.PlayerShip.isDestroyed) return;
        IngameMenuController.Instance.closeMenu();
        Destroy(this.gameObject);
    }
}

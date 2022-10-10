using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasController : Singleton<CanvasController> {

    public GameObject IngameMenu;
    
    private Stack<GameObject> _openMenus;
    private System.EventHandler _onClickDelegate;

    private void Awake()
    {
        _openMenus = new Stack<GameObject>();
        _onClickDelegate = new EventHandler(OnCloseClicked);
        EventManager.CloseClicked += _onClickDelegate;
        //Ship.IsShipInputDisabled = false;
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        CloseMenu();
    }
    private void OnEnable()
    {
        
        
    }
    private void OnDestroy()
    {
        EventManager.CloseClicked -= _onClickDelegate;
    }

    // Either the ingame menu or popup menus can be open
    void Update ()
    {
        if (SceneManager.GetActiveScene().name == "EmptyPlanet")
        {
            Player.Instance.inputDisabled = GetNumberOfOpenMenus() > 0;

            return;
        }
        if(GetNumberOfOpenMenus() > 0)
        {
            //print("Menus open?");
            Ship.PlayerShip.UsingMouseInput = false;
            Ship.IsShipInputDisabled = true;
        }
        
        if (Ship.IsShipInputDisabled)
        {
            Ship.IsShipInputDisabled = false;
        }
    }

    private void OpenMainMenuPopup()
    {
        var popupMenu = OpenMenuAtPosition(UIElements.Instance.SimpleMenu, new Vector2(Screen.width / 2, Screen.height / 2), true)
               .GetComponent<SimpleMenuController>();

        popupMenu.HeaderText.text = "";
        popupMenu.AddMenuOption("Save game").AddListener(() =>
        {
            SaveGame.SaveAutosave(SectorNavigation.UNSET_SECTOR);
        });
        popupMenu.AddMenuOption("Exit to Main Menu").AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
    }
    
    /// Opens a requested menu and returns the reference to the UI gameobject.
    public GameObject OpenMenu(GameObject menu)
    {
        GameObject menuInstance = GameObject.Instantiate(menu, this.transform);
        // Set currently open menu to inactive
        if (_openMenus.Count > 0)
            _openMenus.Peek().SetActive(false);
        // Add new open menu
        _openMenus.Push(menuInstance);

        //Debug.Log("OPENED MENU " + menuInstance.name + " ,open menus: " + GetNumberOfOpenMenus());

        return menuInstance;
    }
    
    /// Opens a requested menu at a given 2D position (usually mousePosition) 
    /// and returns the reference to the UI gameobject.
    public GameObject OpenMenuAtPosition(GameObject menu, Vector2 position, bool hideMenuBelow = true)
    {
        GameObject menuInstance = null;

        if (!hideMenuBelow)
        {
            menuInstance = GameObject.Instantiate(menu, this.transform);

            // Add new open menu
            _openMenus.Push(menuInstance);
        }
        else
            menuInstance = OpenMenu(menu);

        menuInstance.GetComponent<RectTransform>().anchoredPosition = position;

        //Debug.Log("OPENED MENU " + menuInstance.name + " ,open menus: " + GetNumberOfOpenMenus());

        return menuInstance;
    }

   

    /// <summary>
    /// Closes the currently visible (open) menu (and opens a menu one layer below, if such
    /// exists)
    /// </summary>
    public void CloseMenu()
    {
        if (_openMenus.Count > 0 && _openMenus.Peek().GetComponent<StationMainMenu>())
            return; // Do not close station main menu. Ever.

        // if (_openMenus.Count == 0)
        // {
        //     IngameMenu.SetActive(false);
        // }
        // if (GetNumberOfOpenMenus() == 0/* && !IngameMenu.activeInHierarchy*/)
        // {
        //     Ship.IsShipInputDisabled = false;
        // }

        if (_openMenus.Count > 0) {

            GameObject menu = _openMenus.Pop();
            GameObject.Destroy(menu);
        }
        if (_openMenus.Count > 0)
            _openMenus.Peek().SetActive(true);
    }

    /// <summary>
    /// Closes all active menus, including the Station Menu.
    /// </summary>
    public void CloseAllStationMenus()
    {
        while (_openMenus.Count > 0)
        {

            GameObject menu = _openMenus.Pop();
            GameObject.Destroy(menu);
        }
        Ship.IsShipInputDisabled = false;
        Cursor.visible = false;
        if (Camera.main is not null) Camera.main.GetComponent<CameraController>().ToggleFlightMode();
    }

    public int GetNumberOfOpenMenus()
    {
        return _openMenus.Count;
    }

    /// <summary>
    /// Closes all opened menus, including the special menus (ingame menu and map)
    /// </summary>
    public void CloseAllMenus()
    {
        while (GetNumberOfOpenMenus() > 0)
            CloseMenu();
    }  
}

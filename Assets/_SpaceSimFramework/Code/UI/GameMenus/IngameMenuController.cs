using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IngameMenuController : Singleton<IngameMenuController> {


    private int _selectedItem = -1;
    private List<ClickableImage> _menuItems;
    private static bool subMenusOpen = false;
    private GameObject[] _openedMenus;   // Tracks which menus are currently open

    void Awake () {
        _menuItems = new List<ClickableImage>();        
    
        foreach (Transform child in transform)
        {
            _menuItems.Add(child.gameObject.GetComponent<ClickableImage>());
        }
    
        _openedMenus = new GameObject[_menuItems.Count];
    }
	
    private void Update ()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            DisplayPlayerInfo();
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            DisplayShipInfo();
        }
    }

    /// <summary>
    /// This method contains the functionality of the menu, and performs the 
    /// desired operation depending on which option was selected by the user.
    /// </summary>
    public void OnItemSelected()
    {
        if (_openedMenus[_selectedItem] != null)
            return;

        if (_selectedItem == 0)   // Current ship menu
        {
            _openedMenus[0] = CanvasController.Instance.OpenMenu(UIElements.Instance.TargetMenu);
            TargetScrollMenu menu = _openedMenus[0].GetComponent<TargetScrollMenu>();

            menu.PopulateMenuOptions(Ship.PlayerShip.gameObject);
        }
        if (_selectedItem == 1)  // Sector navigation menu
        {
            _openedMenus[1] = CanvasController.Instance.OpenMenuAtPosition(UIElements.Instance.SimpleMenu, new Vector2(Screen.width/2, Screen.height/2));
            SimpleMenuController navMenu = _openedMenus[1].GetComponent<SimpleMenuController>();
            _openedMenus[1].GetComponent<InterfaceAnimManager>().startAppear();
            navMenu.HeaderText.text = "Select map type";

            navMenu.AddMenuOption("Sector Map").AddListener(() => {
                CanvasController.Instance.CloseAllMenus();
                CanvasViewController.Instance.ToggleMap();
            });
            navMenu.AddMenuOption("Universe Map").AddListener(() => {
                CanvasController.Instance.CloseMenu();
                _openedMenus[1] = CanvasController.Instance.OpenMenu(UIElements.Instance.UniverseMap);
            });
        }
        if (_selectedItem == 2)  // Target Menu
        {
            GameObject target = InputHandler.Instance.GetCurrentSelectedTarget();
            if (target == null)
            {
                MusicController.Instance.PlaySound(AudioController.Instance.ScrollSound);
                return;
            }

            _openedMenus[2] = CanvasController.Instance.OpenMenu(UIElements.Instance.TargetMenu);
            TargetScrollMenu menu = _openedMenus[2].GetComponent<TargetScrollMenu>();

            menu.PopulateMenuOptions(target);
        }
        if (_selectedItem == 3)  // Reputation menu
        {
            _openedMenus[3] = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText);
            ScrollTextController reputationMenu = _openedMenus[3].GetComponent<ScrollTextController>();

            reputationMenu.HeaderText.text = "Player Information";

            reputationMenu.AddMenuItem("Level: " + Progression.Level, true, Color.white);
            reputationMenu.AddMenuItem("Experience: " + Progression.Experience, false, Color.white);
            reputationMenu.AddMenuItem("", false, Color.white);
            reputationMenu.AddMenuItem("Credits: "+Player.Instance.credits, false, Color.white);
            reputationMenu.AddMenuItem("Ships owned: " + (Player.Instance.Ships.Count+Player.Instance.OOSShips.Count), false, Color.white);
            reputationMenu.AddMenuItem("", false, Color.white);

            reputationMenu.AddMenuItem("Player reputation", true, Color.white);
            foreach (Faction otherFaction in ObjectFactory.Instance.Factions)
            {
                if (otherFaction == Player.Instance.playerFaction)
                    continue;

                reputationMenu.AddMenuItem(otherFaction.name + ": " + Player.Instance.playerFaction.RelationWith(otherFaction).ToString("0.0"),
                    false, Player.Instance.playerFaction.GetRelationColor(otherFaction));
            }

            reputationMenu.AddMenuItem("",false, Color.white);
            reputationMenu.AddMenuItem("Fighter and capital ship kills by faction:", true, Color.white);
            foreach(var killsByFaction in Player.Instance.Kills)
            {
                reputationMenu.AddMenuItem(killsByFaction.Key.name+" --- "+ killsByFaction.Value.x+", "+ killsByFaction.Value.y, false, Color.white);
            }

            if(MissionControl.CurrentJob != null)
            {
                reputationMenu.AddMenuItem("", false, Color.white);
                reputationMenu.AddMenuItem("Current mission: " + MissionControl.CurrentJob.Type+ " for "+
                    MissionControl.CurrentJob.Employer, true, Color.white);
                reputationMenu.AddMenuItem("Time remaining: " +
                    (MissionControl.CurrentJob.Duration - Time.time - MissionControl.CurrentJob.TimeStarted), true, Color.white);
            }
        }
        if (_selectedItem == 4)
        {
            //print("Help");
            displayHelp();
        }
        MusicController.Instance.PlaySound(AudioController.Instance.SelectSound);
    }

    public void displayHelp()
    {
        if (subMenusOpen)
            return;
        var menuController = CanvasController.Instance.OpenMenu(UIElements.Instance.HelpMenuDialog);
        subMenusOpen = true;
    }

    public void displaySettings()
    {
        if (subMenusOpen)
            return;
        var menuController = CanvasController.Instance.OpenMenu(UIElements.Instance.SettingsDialog);
        subMenusOpen = true;
    }

    public void closeMenu()
    {
        subMenusOpen = false;
    }
    public void OnItemSelected(int itemID)
    {
        _selectedItem = itemID;
        OnItemSelected();
    }

    public void DisplayPlayerInfo()
    {
        _openedMenus[0] = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText);
        ScrollTextController reputationMenu = _openedMenus[0].GetComponent<ScrollTextController>();

        reputationMenu.HeaderText.text = "Player Information";

        reputationMenu.AddMenuItem("Level: " + Progression.Level, true, Color.white);
        reputationMenu.AddMenuItem("Experience: " + Progression.Experience, false, Color.white);

        reputationMenu.AddMenuItem("Player reputation", true, Color.white);
        foreach (Faction otherFaction in ObjectFactory.Instance.Factions)
        {
            if (otherFaction == Player.Instance.playerFaction)
                continue;

            reputationMenu.AddMenuItem(otherFaction.name + ": " + Player.Instance.playerFaction.RelationWith(otherFaction).ToString("0.0"),
                false, Player.Instance.playerFaction.GetRelationColor(otherFaction));
        }

        reputationMenu.AddMenuItem("",false, Color.white);
        reputationMenu.AddMenuItem("Fighters and station kills by faction:", true, Color.white);

        foreach(var killsByFaction in Player.Instance.Kills)
        {
            reputationMenu.AddMenuItem(killsByFaction.Key.name+" --- "+ killsByFaction.Value.x+", "+ killsByFaction.Value.y, false, Color.white);
        }

        if(MissionControl.CurrentJob != null)
        {
            reputationMenu.AddMenuItem("", false, Color.white);
            reputationMenu.AddMenuItem("Current mission: " + MissionControl.CurrentJob.Type+ " for "+
                                       MissionControl.CurrentJob.Employer, true, Color.white);
            reputationMenu.AddMenuItem("Time remaining: " +
                                       (MissionControl.CurrentJob.Duration - Time.time - MissionControl.CurrentJob.TimeStarted), true, Color.white);
        }
    }

    public void DisplayShipInfo()
    {
        _openedMenus[0] = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollText);
        ScrollTextController shipInfoMenu = _openedMenus[0].GetComponent<ScrollTextController>();

        shipInfoMenu.HeaderText.text = "Ship Information";

        shipInfoMenu.AddMenuItem("Base model: " + Ship.PlayerShip.shipModelInfo.modelName, true, Color.white);
        shipInfoMenu.AddMenuItem("Cargo: " + Ship.PlayerShip.ShipCargo.cargoSize+"/"+Ship.PlayerShip.ShipCargo.cargoOccupied, true, Color.white);
        foreach (var content in Ship.PlayerShip.ShipCargo.cargoContents)
        {
            shipInfoMenu.AddMenuItem(content.itemName + "   " + content.amount, true, Color.white);
        }
        // shipInfoMenu.AddMenuItem("Experience: " + Progression.Experience, false, Color.white);
        //
        // shipInfoMenu.AddMenuItem("Player reputation", true, Color.white);
        // foreach (Faction otherFaction in ObjectFactory.Instance.Factions)
        // {
        //     if (otherFaction == Player.Instance.PlayerFaction)
        //         continue;
        //
        //     shipInfoMenu.AddMenuItem(otherFaction.name + ": " + Player.Instance.PlayerFaction.RelationWith(otherFaction).ToString("0.0"),
        //         false, Player.Instance.PlayerFaction.GetRelationColor(otherFaction));
        // }        
    }
}

using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class StationMainMenu : ScrollMenuController
{
    private GameObject shipLandedAtStation;

    public void PopulateMenuOptions(GameObject ship, Station station)
    {
        shipLandedAtStation = ship;
        var dealer = station.gameObject.GetComponent<StationDealer>();
        HeaderText.text = station.stationName;
        
        //ShowDockedShips(station);
        if(station.hasCargoDealer)       
            AddMenuOption("Station Trading Post").AddListener(() => {
                var menu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationTradeMenu);
                menu.GetComponent<StationTradeMenu>().PopulateMenu(shipLandedAtStation, station);
            });
        if(station.hasEquipmentDealer)
            AddMenuOption("Shipyard Equipment Store").AddListener(() => {
                var menu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationEquipmentMenu);
                menu.GetComponent<StationEquipmentMenu>().PopulateMenu(shipLandedAtStation, station);
            });
        if(station.hasShipDealer)
            AddMenuOption("Shipyard Paint Shop").AddListener(() => {
                //var tradeMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationNFTMenu);
                //tradeMenu.GetComponent<StationNFT>().PopulateMenu(shipLandedAtStation, station);
            
                var menu = CanvasController.Instance.OpenMenu(UIElements.Instance.ClosedShopDialog);
                menu.GetComponent<PortraitDialogueMenu>().PopulateMenu("Paint Shop", station);
            });
        if(station.hasShipDealer)
            AddMenuOption("Shipyard Sales").AddListener(() =>
            {
                var shipDealerMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationDealershipMenu);
                shipDealerMenu.GetComponent<StationShipDealerMenu>().PopulateMenu(shipLandedAtStation, station);
            
                // var menu = CanvasController.Instance.OpenMenu(UIElements.Instance.ClosedShopDialog);
                // menu.GetComponent<PortraitDialogueMenu>().PopulateMenu("Sales", station);
            });
        if(station.hasInfoBooth)
            AddMenuOption("Station Library Computers").AddListener(() =>
            {
                var menu = CanvasController.Instance.OpenMenu(UIElements.Instance.ComputerLibraryMenu);
                menu.GetComponent<ComputerMenuController>().PopulateMenu(shipLandedAtStation, station);
            });
        if(station.hasInfoBooth)
            AddMenuOption("Station Job Board").AddListener(() => {
                // var menu = CanvasController.Instance.OpenMenu(UIElements.Instance.ClosedShopDialog);
                // menu.GetComponent<PortraitDialogueMenu>().PopulateMenu("Job Board", station);  
                
                var jobMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationJobsMenu);
                ScrollMenuController jobsBoard = jobMenu.GetComponent<ScrollMenuController>();
                //jobMenu.PopulateMenu("Station Dive Bar", station);
                
                int numJobs = Random.Range(0, 5);
                if (numJobs == 0)
                {
                    jobsBoard.HeaderText.text = "No jobs are offered currently.";
                    return;
                }
            
                jobsBoard.HeaderText.text = "Select a job to view details:";
                for (int i = 0; i < numJobs; i++)
                {
                    Mission m_i = MissionControl.GetNewMission(station.faction);
                    jobsBoard.AddMenuOption(
                        m_i.Type + " (" + m_i.Payout + " credits, time: " + m_i.Duration / 60 + " minutes)",
                        Color.white,
                        IconManager.Instance.GetMissionIcon(m_i.Type.ToString()),
                        1, 80).AddListener(() => {
                        // Player accepts mission, start timer
                        m_i.TimeStarted = Time.time;
                        MissionControl.CurrentJob = m_i;
                        m_i.OnMissionStarted();                                      
            
                        OnCloseClicked();
                    });
                }
            });
        if(station.hasBarDealer)
            AddMenuOption("Station Bar").AddListener(() =>
            {
                // var menu = CanvasController.Instance.OpenMenu(UIElements.Instance.ClosedShopDialog);
                // menu.GetComponent<PortraitDialogueMenu>().PopulateMenu("Station Bar", station);
            
                var rumourMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.RumourDialog);
                RumourDialogueMenu rumourBoard = rumourMenu.GetComponent<RumourDialogueMenu>();
                rumourBoard.PopulateMenu("Station Dive Bar", station);
            
                int numRumours = UnityEngine.Random.Range(0, 10);
                if (numRumours == 0)
                {
                    rumourBoard.HeaderText.text = "Nothing interesting overheard";
                    return;
                }
                //rumourBoard.HeaderText.text = "Select a rumour to hear more:";
                for (int i = 0; i < numRumours; i++)
                {
                    var rumour = Rumours.GenerateRumour();
                    //rumourBoard.messageBody.text = rumour;
                    // rumourBoard.AddMenuOption(rumour).AddListener(() => {
                    //     var rumourMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.PortraitDialog);
                    //     rumourMenu.GetComponent<PortraitDialogueMenu>().PopulateMenu(rumour, station, "");
                    //     //OnCloseClicked();
                    // });
                }
            });
        if(station.hasRepairDealer)
            AddMenuOption("Repair Ship").AddListener(() =>
            {
                var repairMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationRepairMenu);
                var repairDialog = repairMenu.GetComponent<RepairDialogueMenu>();
                repairDialog.PopulateMenu("", station, null, "");
            });
        
        AddMenuOption("Undock From Station").AddListener(() => {
            CanvasController.Instance.CloseMenu();
            station.UndockShip(Ship.PlayerShip.gameObject);
        });

        //AddMenuOption("Attempt Sabotage").AddListener(() => { });
    }

    public void ShowDockedShips(Station station)
    {
        AddMenuOption("Show Docked Ships").AddListener(() => {
        var infoMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollMenu);
        ScrollMenuController dockedList = infoMenu.GetComponent<ScrollMenuController>();
            dockedList.HeaderText.text = "Ships docked at " + station.name;

            for(int ship_i=0; ship_i<station.dockedShips.Count; ship_i++)
            {
                GameObject ship = station.dockedShips[ship_i];
                dockedList.AddMenuOption(ship.name).AddListener(() =>
                {
                    var submenu = CanvasController.Instance.OpenMenu(UIElements.Instance.ScrollMenu);
                    ScrollMenuController dockedShipMenu = submenu.GetComponent<ScrollMenuController>();

                    // General options for any ship
                    dockedShipMenu.AddMenuOption("Info").AddListener(() =>
                    {
                        TargetScrollMenu.OpenInfoMenu(ship);
                    });

                    if (ship.GetComponent<Ship>().faction.name != Ship.PlayerShip.faction.name)
                        return;

                    // Options for player-owned ships
                    dockedShipMenu.AddMenuOption("Undock Ship").AddListener(() =>
                    {
                        station.UndockShip(ship);
                        ship.GetComponent<ShipPlayerInput>().throttle = 0.5f;
                    });

                    dockedShipMenu.AddMenuOption("Change Ship").AddListener(() =>
                    {                        
                        Ship otherShip = ship.GetComponent<Ship>();
                        if (otherShip == Ship.PlayerShip)
                            return;

                        // Reset camera to follow in case ship doesn't have a cockpit
                        Camera.main.GetComponent<CameraController>().State = CameraController.CameraState.Chase;

                        Ship.PlayerShip.isPlayerControlled = false;
                        Ship.PlayerShip = otherShip;
                        Ship.PlayerShip.isPlayerControlled = true;
                        shipLandedAtStation = ship;

                        OnCloseClicked();
                    });
                });
            }
        });
    }
}

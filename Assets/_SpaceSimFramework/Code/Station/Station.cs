
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Forge3D;
using SpaceSimFramework.Code.UI.HUD;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum StationClass
{
    Military,
    Trade,
    Research,
    Entertainment,
    Shipyard,
    Diplomatic
}

public class Station : MonoBehaviour {

    /// <summary>
    /// Used to notify Map Markers that a ship's marker should be removed
    /// </summary>
    public static event EventHandler ShipDockedEvent;

    // Unique in-game object ID 
    public string id;
    public string stationName;
    public string ownerID;
    public StationClass stationClass;
    [Tooltip("Docks for small ships")]
    public StationDock dock;
    [Tooltip("Mooring points for large ships")]
    public StationMooring[] moorings;
    public List<GameObject> dockedShips;
    public GameObject[] shields;
    [Tooltip("Station owner faction")]
    public Faction faction;

    //[Tooltip("Animator of the dock doors")]
    //public Animator dockAnimator;
    public DoorCode dockHangerDoor;
    [Tooltip("Station type data holder")]
    public StationLoadout loadout;
 
    [Header("Station Facilities")]
    public bool hasInfoBooth = true;
    public bool hasStationChief = true;
    public Sprite stationChief;
    public bool hasCargoDealer = true;
    public Sprite cargoSales;
    public bool hasEquipmentDealer = true;
    public Sprite equipmentSales;
    public bool hasShipDealer = false;
    public Sprite shipSales;
    public bool hasBarDealer;
    public Sprite barTender;
    public bool hasRepairDealer = true;
    public Sprite repairDealer;
    private GameObject _shipUnmooring;
    public float maxArmor;
    public float armor;
    private bool _isDestroyed = false;
    private GameObject _fireEffect;

    public GameObject[] debrisObjects;

    public float distanceToPlayerShip;
    // Weapon systems
    public StationEquipment Equipment
    {
        get
        {
            if (_stationEquipment == null)
                _stationEquipment = GetComponent<StationEquipment>();
            return _stationEquipment;
        }
    }
    private StationEquipment _stationEquipment;

    public void Awake ()
    {
        armor = maxArmor;
        dockedShips = new List<GameObject>();
        // Mooring points and their docked ships
        if (dockHangerDoor != null)
            dockHangerDoor.CloseDoor();
    }

    private void Start()
    {
        if(loadout != null)
            StationLoadout.ApplyLoadoutToStation(loadout, this);
        
        // var mat = ShieldMaterials.Instance.materials[Random.Range(0, ShieldMaterials.Instance.materials.Length)];
        // foreach (var shield in shields)
        // {
        //     shield.GetComponent<MeshRenderer>().material = mat;
        // }
    }

    private void Update()
    {
        if (Ship.PlayerShip != null)
        {
            distanceToPlayerShip = Vector3.Distance(Ship.PlayerShip.transform.position, this.transform.position);
            if (distanceToPlayerShip < 500)
            {
                Ship.PlayerShip.isSpeedLimited = true;
                Ship.PlayerShip.inSupercruise = false;
            } else Ship.PlayerShip.isSpeedLimited = false;
        }
        
        // Keep dock open until ship leaves vicinity
        if (_shipUnmooring == null) return;
        if (Vector3.Distance(_shipUnmooring.transform.position, dock.transform.position) > 150f)
        {
            if (dockHangerDoor != null)
                dockHangerDoor.CloseDoor();

            _shipUnmooring = null;
        }
    }


    /// <summary>
    /// Invoked when a smaller ship has entered the dock and docked.
    /// </summary>
    /// <param name="other">Ship which docked</param>
    public void OnDockContact(Collider other)
    {
        GameObject ship = other.gameObject;
        Ship shipComponent = ship.GetComponent<Ship>();
        if (dockedShips.Contains(ship)) // OnTriggerEnter will be called multiple times
            return;
        if (shipComponent.shipModelInfo.ExternalDocking)
            return; // refuse docking for large vessels

        ShipDockedEvent?.Invoke(ship, EventArgs.Empty);
        if (dockHangerDoor != null)
            dockHangerDoor.CloseDoor();
        dockedShips.Add(ship);
        ship.SetActive(false);
        shipComponent.stationDocked = id;
        if(shipComponent == Ship.PlayerShip)
        {
            print("player attempting to dock");
            DockShip(ship);
        }
        else if(shipComponent.faction != Ship.PlayerShip.faction)
        {
            print("undocking AI ship "+shipComponent.gameObject.name);
            StartCoroutine(UndockAIShip(ship));
        }
        else
        {
            print("unknown docking situation");
        }
        if (shipComponent.AIInput.CurrentOrder is {Name: "Trade"})
        {
            ((OrderTrade)shipComponent.AIInput.CurrentOrder).PerformTransaction(shipComponent.AIInput);
            StartCoroutine(UndockAIShip(ship));
        }
        foreach (var shield in shields)
        {
            shield.SetActive(true);
            //print("reactivating shields");
        }
    }  

    /// <summary>
    /// Invoked when a capital ship has connected to the mooring point.
    /// </summary>
    public void OnMooringContact(StationMooring mooring, GameObject ship)
    {
        if (dockedShips.Contains(ship)) // OnTriggerEnter will be called multiple times
            return;

        Ship shipComponent = ship.GetComponent<Ship>();
        if (!shipComponent.shipModelInfo.ExternalDocking)
            return; // refuse mooring for small vessels

        shipComponent.stationDocked = id;
        shipComponent.PlayerInput.throttle = 0;
        shipComponent.PlayerInput.strafe = 0;
        shipComponent.Physics.enabled = false;
        ship.GetComponent<Rigidbody>().isKinematic = true;

        dockedShips.Add(ship);

        ShipDockedEvent?.Invoke(ship, EventArgs.Empty);

        if (shipComponent == Ship.PlayerShip)
        {
            DockShip(ship);
        }
        else if (shipComponent.faction != Ship.PlayerShip.faction)
        {
            StartCoroutine(UndockAIShip(ship));
        }

        if (shipComponent.AIInput.CurrentOrder is {Name: "Trade"})
        {
            ((OrderTrade)shipComponent.AIInput.CurrentOrder).PerformTransaction(shipComponent.AIInput);
            StartCoroutine(UndockAIShip(ship));
        }
        foreach (var shield in shields)
        {
            shield.SetActive(true);
           // print("reactivating shields");
        }
    }

    private void DockShip(GameObject ship)
    {
        CanvasController.Instance.CloseAllMenus();
        CanvasController.Instance.CloseMenu();  // Close ingame menu as well, if open
        if (Camera.main is not null)
        {
            var cam = Camera.main.GetComponent<CameraController>();
            cam.State = CameraController.CameraState.Chase;
            cam.SetTargetStation(this.transform, new Vector3(0, 50, -500));
        }

        CanvasViewController.Instance.SetHUDActive(false);
        if (CanvasViewController.IsMapActive)
        {
            if (Camera.main is not null) Camera.main.GetComponent<MapCameraController>().CanMove = false;
            CanvasViewController.Instance.TacticalCanvas.gameObject.SetActive(false);
        }
        InputHandler.Instance.gameObject.SetActive(false);

        OpenStationMenu(ship);
        //print("Station menu opened");
    }

    public StationMooring GetFreeMooringPoint()
    {
        moorings ??= GetComponentsInChildren<StationMooring>();
        foreach (var moor in moorings)
        {
            if (moor.ship)
            {
                //print("Occupied, attempting to boot");
                StartCoroutine(UndockAIShip(moor.ship));
            }
        }
        return moorings.FirstOrDefault(mooring => mooring.ship == null);
    }

    public void OpenStationMenu(GameObject ship)
    {
        var stationMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationMainMenu)
           .GetComponent<StationMainMenu>();
                    Ship.IsShipInputDisabled = true;
        Cursor.visible = true;
        Ship.PlayerShip.UsingMouseInput = false;
        stationMenu.PopulateMenuOptions(ship, this);     
    }

    private IEnumerator UndockAIShip(GameObject ship)
    {
        yield return new WaitForSeconds(3);
        UndockShip(ship);
    }

    /// <summary>
    /// Requested undocking, handle ship undocking
    /// </summary>
    /// <param name="ship"></param>
    public void UndockShip(GameObject ship)
    {
        Ship shipComp = ship.GetComponent<Ship>();

        dockedShips.Remove(ship);
        shipComp.stationDocked = "none";

        if (shipComp.shipModelInfo.ExternalDocking)
        {
            shipComp.Physics.enabled = true;
            ship.transform.rotation = moorings[0].Waypoints[0].transform.localRotation;
            ship.GetComponent<Rigidbody>().isKinematic = false;
            ship.GetComponent<ShipAI>().currentOrder = Order.ShipOrder.None;
            shipComp.AIInput.CurrentOrder = null;
            StationMooring mooring = moorings.FirstOrDefault(pair => pair.ship == ship);   // This is the mooring point for the docked ship
            if (mooring != null)
            {
                mooring.ship = null;
            }
        }
        else
        {
            ship.SetActive(true);
            var dockTrans = dock.transform;
            ship.transform.rotation = dockTrans.rotation;
            ship.transform.position = dockTrans.position;
            if (dockHangerDoor != null)
                dockHangerDoor.OpenDoor();
            _shipUnmooring = ship;
        }

        ship.GetComponent<Rigidbody>().velocity = Vector3.zero;

        if (Ship.PlayerShip.gameObject == ship)
        {
            CanvasController.Instance.CloseAllStationMenus();
            InputHandler.Instance.SelectedObject = null;
            InputHandler.Instance.gameObject.SetActive(true);
            if (Camera.main is not null) Camera.main.GetComponent<CameraController>().SetTargetPlayerShip();
            CanvasViewController.Instance.SetHUDActive(!CanvasViewController.IsMapActive);
            if (CanvasViewController.IsMapActive)
            {
                CanvasViewController.Instance.TacticalCanvas.gameObject.SetActive(true);
                InputHandler.Instance.SelectedObject = null;
            }
            //EquipmentIconUI.Instance.SetIconsForShip(shipComp);

            if (shipComp.AIInput.CurrentOrder is not {Name: "Trade"}) { 
                Ship.PlayerShip.isPlayerControlled = true;
                Ship.IsShipInputDisabled = false;
            }
            TextFlash.ShowYellowText("Auto-undock engaged\n control will be returned momentarily.");
        }
        
        foreach (var shield in shields)
        {
            shield.SetActive(false);
            //print("Shields off");
        }
        StartCoroutine(FlyShipAwayFromDock(shipComp));
    }

    /// <summary>
    /// Takes over ship control while undocking to ensure safe distance from dock.
    /// Wait 3 second for dock doors to open, then 2 seconds of full throttle.
    /// then stop.
    /// </summary>
    private IEnumerator FlyShipAwayFromDock(Ship shipComp)
    {
        bool wasPlayerControlled = shipComp.isPlayerControlled;
        shipComp.isPlayerControlled = false;

        shipComp.AIInput.isUndocking = true;
        float timer = 6.0f;

        while (timer > 0) {
            shipComp.AIInput.angularTorque = Vector3.zero;
            if (timer < 3)
            {
                shipComp.AIInput.throttle = -0.2f;
            }
            timer -= Time.deltaTime;
            yield return null;
        }
        shipComp.AIInput.throttle = 0f;
        shipComp.AIInput.isUndocking = false;
        shipComp.isPlayerControlled = wasPlayerControlled;
        Ship.PlayerShip.UsingMouseInput = true;
        foreach (var shield in shields)
        {
            shield.SetActive(true);
            //print("reactivating shields");
        }
    }

    /// <summary>
    /// Allows a requester to dock. If docking a fighter, waits for OnDockContact.
    /// If docking a cap ship, waits for OnMooringContact
    /// </summary>
    /// <param name="ship"></param>
    /// <returns>Docking pattern if docking is granted; null otherwise</returns>
    public GameObject[] RequestDocking(GameObject ship) 
    {
        Ship shipComponent = ship.GetComponent<Ship>();
        // Check reputation
        if (shipComponent.faction.RelationWith(faction) < 0 && shipComponent == Ship.PlayerShip)
        {
            ConsoleOutput.PostMessage("[" + name + " Approach Control]: Docking denied, please leave the area.", Color.red);
            TextFlash.ShowYellowText("[" + name + " Approach Control]\n Docking denied, please leave the area.");
            throw new DockingForbiddenException(this);
        }

        // if (shipComponent.ShipModelInfo.ExternalDocking && shipComponent == Ship.PlayerShip)
        // {
            StationMooring mooring = GetFreeMooringPoint();
            if (mooring == null)
            {
                if (shipComponent == Ship.PlayerShip)
                {
                    ConsoleOutput.PostMessage(
                        "[" + name + " Approach Control]: Docking denied - All mooring points occupied!", Color.red);
                    TextFlash.ShowYellowText("Docking denied - All mooring points occupied!");
                    shipComponent.doNotFireOnMe = false;
                    Cursor.visible = true;
                    shipComponent.UsingMouseInput = true;
                }
                throw new MooringUnavailableException(this); // Moorings full, cannot dock capital ship
            }
            else
            {
                if (shipComponent == Ship.PlayerShip)
                {
                    TextFlash.ShowYellowText("Auto-docking engaged");
                    ConsoleOutput.PostMessage(
                        "[" + name + " Approach Control]: Docking granted, proceed to mooring point.", Color.green);
                }
                mooring.ship = ship;
                if (shipComponent != Ship.PlayerShip) return mooring.Waypoints;
                foreach (var shield in shields)
                {
                    shield.SetActive(false);
                    //print("Shields off 2");
                    //Physics.IgnoreCollision(shipComponent.GetComponent<Collider>(), GetComponent<Collider>(), true);
                }
                return mooring.Waypoints;
            }
        // }
        // else
        // {
        //     if (docks.Length <= 0 && shipComponent == Ship.PlayerShip)
        //     {
        //         ConsoleOutput.PostMessage(
        //             "[" + name + " Approach Control]: Docking denied - No services available!", Color.red);
        //         TextFlash.ShowYellowText("[" + name +
        //                                  " Approach Control]\n Docking denied - No services available!");
        //         throw new MooringUnavailableException(this); // Moorings full, cannot dock capital ship
        //     }
        //
        //     dock.DockingQueue.Enqueue(ship);
        //     if (shipComponent == Ship.PlayerShip)
        //     {
        //         ConsoleOutput.PostMessage("[" + name + " Approach Control]: Docking granted, proceed to docking bay.",
        //             Color.green);
        //         //TextFlash.ShowYellowText("[Approach Control]\n Docking granted, proceed to docking bay.");
        //     }
        //
        //     if (dockHangerDoor != null)
        //         dockHangerDoor.OpenDoor();
        //     
        //     return dock.DockWaypoints;
        // }
    }

    /// <summary>
    /// Forces docking of a ship to a station. Used for loading docked ships.
    /// </summary>
    /// <param name="ship">Ship that will be docked to this station</param>
    public void ForceDockShip(Ship ship)
    {
        if (ship == Ship.PlayerShip)
            // Make sure ingame menu is closed
            CanvasController.Instance.IngameMenu.SetActive(false);

        // Find free docking slot
        if (ship.shipModelInfo.ExternalDocking)
        {
            StationMooring mooring = GetFreeMooringPoint();
            if(mooring != null)
            {
                mooring.ship = ship.gameObject;
                GameObject[] waypoints = mooring.Waypoints;
                ship.transform.position = waypoints[^1].transform.position;
            }
            // OnMooringContact happens now
        }
        else
        {
            dock.DockingQueue.Clear();
            dock.DockingQueue.Enqueue(ship.gameObject);
            ship.transform.position = dock.DockWaypoints[^1].transform.position;
            // OnDockContact happens now
        }
    }

    public void TakeDamage(float damage, Ship owner, bool isPlayerShot, bool shouldShake=true)
    {
        if (_isDestroyed)
            return;

        //print("Station hit by "+owner);
        // alert all the turrets
        if (_stationEquipment.mountedTurrets.Count > 0 && owner != null)
        {
            foreach (var turret in _stationEquipment.mountedTurrets)
            {
                turret.target = owner.transform;
                //turret.SetTarget(owner.gameObject.transform);
                turret.turretController.SetAimpoint(turret.target.position);
                turret.command = TurretCommands.TurretOrder.AttackTarget;
            }
        }

        armor -= damage;
        if(armor < 0)
        {
            _isDestroyed = true;

            ParticleController.Instance.CreateStationExplosionAtPos(transform.position);
            if (InputHandler.Instance.GetCurrentSelectedTarget() == this.gameObject)
                InputHandler.Instance.SelectedObject = null;

            foreach (var debris in debrisObjects)
            {
                var go = Instantiate(debris, this.transform);
                print("Loaded debris "+go.name);
                go.transform.SetParent(null);
            }

            if (isPlayerShot) {
                // Broadcast kill
                Progression.RegisterKill(this);
                MissionControl.RegisterKill(this);

                // Mark player kill
                Player.Instance.Kills[faction] += new Vector3(0,0,1);
                    TextFlash.ShowYellowText(faction.name + " station destroyed! "+Player.Instance.Kills[faction]);
                    ConsoleOutput.PostMessage(faction.name + " station destroyed!");

                    if(faction != Player.Instance.playerFaction)
                    Player.Instance.AddFactionPenalty(faction);
            }
            else
            {
                TextFlash.ShowYellowText("An unknown unit was just destroyed by "+stationName+"!");
            }
            GameObject.Destroy(this.gameObject);
        }
        else if (armor < 0.25 * maxArmor && _fireEffect == null)
        {
            var fireEffect = GameObject.Instantiate(ParticleController.Instance.ShipDamageEffect, transform);
        }
        else if (_fireEffect != null)
        {
            GameObject.Destroy(_fireEffect);
        }

    }

    // Used by AI generic move command, exchanges the stations result with a waypoints to prevent crashing.
    public Transform GetFirstWaypoint()
    {
        if (moorings.Length > 0)
            return (moorings[0].Waypoints[0].transform);
        else if (dock != null)
            return (dock.DockWaypoints[0].transform);
        return null;
    }
    
    public void GenerateStation()
    {
        id = "ST-" + GenerateRandomSector.RandomString(4);
        stationName = RandomNameGenerator.GetRandomStationName();
    }
    public void ClearStation() {}
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TurretOrder = TurretCommands.TurretOrder;

public partial class ShipEquipment : MonoBehaviour
{
    
    #region weapons
    public List<GunHardpoint> Guns
    {
        get { return _guns ??= new List<GunHardpoint>(); }
    }
    // Guns are player/ship controlled
    private List<GunHardpoint> _guns;

    // Turrets are individually/automatically controlled
    public List<TurretHardpoint> Turrets
    {
        get { return _turrets ??= new List<TurretHardpoint>(); }
    }
    private List<TurretHardpoint> _turrets;
 
    // Set by AI ship controller when firing conditions are met
    [HideInInspector] public bool isFiring = false;

    private TurretOrder _turretCmd = TurretOrder.AttackEnemies;
    #endregion

    #region energy management
    [Header("Loadout set these, no mess with")]
    public float energyCapacity;
    public float energyRegenRate;
    public float energyAvailable;
    #endregion

    public bool shieldActive = false;
    
    #region equipment
    
    public List<Equipment> MountedEquipment => mountedEquipment;
    [Header("Set this in case loadout doesn't.")]
    public List<Equipment> mountedEquipment;
    public List<GunHardpoint> mountedGuns;
    public List<TurretHardpoint> mountedTurrets;
    [Header("Game sees this")]
    public int curMissiles;
    #endregion equipment
    [HideInInspector]
    public Ship ship;
    private int _uiLayer;
    
    private void Awake()
    {
        ship = gameObject.GetComponent<Ship>();
        energyCapacity = ship.shipModelInfo.GeneratorPower;
        energyRegenRate = ship.shipModelInfo.GeneratorRegen;
        energyAvailable = energyCapacity;
        //mountedEquipment = new List<Equipment>();
        _uiLayer = LayerMask.NameToLayer("UI");
    }

    private void Update()
    {
        CheckWeaponInput();
        ComputeEnergyRegen();
        UpdateMountedEquipment();
        UpdateMissileCount();
    }

    #region weapons
    private void CheckWeaponInput()
    {

        // Player input
        if (Ship.PlayerShip != null && this.gameObject == Ship.PlayerShip.gameObject && !ship.InSupercruise) {
            if (Ship.IsShipInputDisabled)
                return;
            if (Ship.PlayerShip.stationDocked != "none")
                return;
            if (Input.GetMouseButton(0) && !CanvasViewController.IsMapActive)
            {
                //print("yes its a player");
                foreach (GunHardpoint gun in Guns)
                    gun.OnTriggerFireGun(true);

                if (_turretCmd == TurretOrder.Manual)
                    foreach (TurretHardpoint turret in Turrets)
                        turret.OnTriggerFireGun(true);

                isFiring = false;
            }
            else if (Input.GetMouseButton(1) && !CanvasViewController.IsMapActive)
            {
                foreach (GunHardpoint gun in Guns)
                    gun.OnTriggerFireGun(false);

                isFiring = false;
            }
        }

        // AI input, not in start scene though
        if (isFiring && SceneManager.GetActiveScene().buildIndex > 0)
        {
            //print("Fire!");
            foreach (GunHardpoint gun in Guns)
                gun.OnTriggerFireGun(true);
            // foreach (TurretHardpoint turret in Turrets)
            //     turret.OnTriggerFireGun(true);
            isFiring = false;
        }
    }

    /// <summary>
    /// Sets all turrets to a given state.
    /// </summary>
    /// <param name="order">New order issued to all turrets</param>
    public void SetTurretCommand(TurretOrder order)
    {
        _turretCmd = order;
        foreach (TurretHardpoint turret in Turrets)
            turret.command = _turretCmd;
    }

    /// <summary>
    /// Get the range of the ship's forward mounted weapons array.
    /// </summary>
    /// <returns></returns>
    public float GetWeaponRange()
    {
        foreach (GunHardpoint gun in Guns)
            if(gun.mountedWeapon != null)
                return gun.mountedWeapon.Range;

        return 0;
    }

    /// <summary>
    /// Adds a gun to the weapons control of this ship. Should be invoked by the hardpoint itself upon start.
    /// </summary>
    public void AddGun(GunHardpoint gun)
    {
        Guns.Add(gun);
    }

    /// <summary>
    /// Adds a turret to the weapons control of this ship. Should be invoked by the hardpoint itself upon start.
    /// </summary>
    public void AddTurret(TurretHardpoint turret)
    {
        Turrets.Add(turret);
    }
    #endregion weapons

    #region energy

    /// <summary>
    /// Apply the energy drain caused by firing the weapon by reducing the available power 
    /// </summary>
    /// <param name="drain">Amount of energy used by the weapon fired</param>
    public void WeaponFired(float drain)
    {
        energyAvailable = ship.faction == Player.Instance.playerFaction ? Mathf.Clamp(energyAvailable - drain, 0, energyCapacity) : Mathf.Clamp(energyAvailable - drain*1.5f, 0, energyCapacity);
    }

    private void ComputeEnergyRegen()
    {
        if (ship.InSupercruise)
            return;
        energyAvailable = Mathf.Clamp(energyAvailable + Time.deltaTime * energyRegenRate, 0, energyCapacity);
    }

    public void SupercruiseDrain()
    {
        if(energyAvailable > 0)
            energyAvailable = Mathf.Clamp(energyAvailable - Time.deltaTime * energyRegenRate * 2.75f, 0, energyCapacity);
        else
            ship.PlayerInput.ToggleSupercruise();

    }
    #endregion energy

    #region equipment
    private void UpdateMountedEquipment()
    {
        // Apply all mounted items
        foreach (var t in mountedEquipment)
        {
            t.UpdateItem(ship);
        }
    }

    /// <summary>
    /// Mounts the specified equipment on the ship, filling an equipment slot
    /// </summary>
    /// <param name="item">Equipment item to mount</param>
    public bool MountEquipmentItem(Equipment item)
    {
        print("Ship has "+mountedEquipment.Count+" items mounted, and "+ship.shipModelInfo.EquipmentSlots+" total");
        // Check if all slots are full
        if(mountedEquipment.Count < ship.shipModelInfo.EquipmentSlots)
        {
            mountedEquipment.Add(item);
            item.InitItem(ship);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes the equipment item from the ship. This is invoked when selling equipment
    /// and when saving game.
    /// </summary>
    /// <param name="item">Equipment item to unmount</param>
    public void UnmountEquipmentItem(Equipment item)
    {
        mountedEquipment.Remove(item);
        item.RemoveItem(ship);
    }

    private void UpdateMissileCount()
    {
        curMissiles = 0;
        foreach (GunHardpoint gun in Guns)
            if (gun.mountedWeapon.Type == WeaponData.WeaponType.Missile)
            {
                foreach (var holdItem in ship.ShipCargo.cargoContents)
                {
                    if (holdItem.itemName != gun.mountedWeapon.GetProjectileName()) continue;
                    curMissiles = holdItem.amount;
                    break;
                }
                break;
            }
    }
    #endregion equipment

    #region pointerUI  
    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
 
 
    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
    {
        foreach (var curRaycastResult in eventSystemRaycastResults)
        {
            if (curRaycastResult.gameObject.layer == _uiLayer)
                return true;
        }

        return false;
    }
 
 
    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults;
    }
    #endregion
}

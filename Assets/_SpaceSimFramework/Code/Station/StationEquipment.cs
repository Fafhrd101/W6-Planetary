using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TurretOrder = TurretCommands.TurretOrder;

public partial class StationEquipment : MonoBehaviour {

    #region weapons

    // Turrets are individually/automatically controlled
    public List<TurretHardpoint> Turrets
    {
        get {
            if (_turrets == null)
            {
                _turrets = new List<TurretHardpoint>();
            }
            return _turrets;
        }
    }
    private List<TurretHardpoint> _turrets;
    public List<TurretHardpoint> mountedTurrets;
    // Set by AI ship controller when firing conditions are met
    [HideInInspector] public bool IsFiring = false;
    private TurretOrder TurretCmd = TurretOrder.AttackEnemies;
    #endregion

    #region energy management
    [HideInInspector] public float energyCapacity;
    [HideInInspector] public float energyRegenRate;
    [HideInInspector] public float energyAvailable;
    #endregion

    #region equipment
    public List<Equipment> MountedEquipment
    {
        get { return mountedEquipment; }
    }
    public List<Equipment> mountedEquipment;

    #endregion equipment

    private Station station;

    private void Awake()
    {
        station = gameObject.GetComponent<Station>();
        // energyCapacity = station.GeneratorPower;
        // energyRegenRate = station.GeneratorRegen;
        mountedEquipment = new List<Equipment>();
    }

    private void Update()
    {
        ComputeEnergyRegen();
        UpdateMountedEquipment();
        if (mountedTurrets.Count <= 0) return;
        var enemies = SectorNavigation.GetClosestNPCShip(this.transform, 2500);
        foreach (var turret in mountedTurrets)
        {
            if (enemies.Count > 0)
            {
                turret.target = enemies[0].transform;
                turret.turretController.SetAimpoint(turret.target.position);
                turret.command = TurretCommands.TurretOrder.AttackTarget;
            }
        }
        // For testing, only target players. Later, we'll check any enemies.
        //if (enemies.Count > 0)
        if (Ship.PlayerShip)
        {
            station.GetComponent<StationAI>().closetTurret =
                GetClosestTurret(Ship.PlayerShip.transform /*enemies[0].transform*/);
            //print("Just in case this shits actually working..."+station.GetComponent<StationAI>().closetTurret[0].name);
        }
    }

    #region weapons

    // Of all the turrets we have, return the one closest to the antagonizing ship.
    private List<TurretHardpoint> GetClosestTurret(Transform target)
    {
        mountedTurrets = mountedTurrets.OrderBy(
            x => Vector2.Distance(this.transform.position,target.position)
        ).ToList();
        // Why does this LINQ shit never seem to work??
        return mountedTurrets;
    }
    
    /// <summary>
    /// Sets all turrets to a given state.
    /// </summary>
    /// <param name="order">New order issued to all turrets</param>
    public void SetTurretCommand(TurretOrder order)
    {
        TurretCmd = order;
        foreach (TurretHardpoint turret in Turrets)
            turret.command = TurretCmd;
    }

    /// <summary>
    /// Adds a turret to the weapons control of this ship. Should be invoked by the hardpoint itself upon start.
    /// </summary>
    public void AddTurret(TurretHardpoint turret)
    {
        mountedTurrets.Add(turret);
    }
    #endregion weapons

    #region energy

    private void ComputeEnergyRegen()
    {
        energyAvailable = Mathf.Clamp(energyAvailable + Time.deltaTime * energyRegenRate, 0, energyCapacity);
    }
    /// <summary>
    /// Apply the energy drain caused by firing the weapon by reducing the available power 
    /// </summary>
    /// <param name="drain">Amount of energy used by the weapon fired</param>
    public void WeaponFired(float drain)
    {
        if(station.faction == Player.Instance.playerFaction)
            energyAvailable = Mathf.Clamp(energyAvailable - drain, 0, energyCapacity);
        else
            energyAvailable = Mathf.Clamp(energyAvailable - drain*1.5f, 0, energyCapacity);
    }
    #endregion energy

    #region equipment
    private void UpdateMountedEquipment()
    {
        // // Apply all mounted items
        // for(int i=0; i<mountedEquipment.Count; i++)
        // {
        //     mountedEquipment[i].UpdateItem(station);
        // }
    }

    #endregion equipment
}

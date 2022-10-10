using UnityEngine;

/// <summary>
/// Contains specifications of a certain station type
/// </summary>
[CreateAssetMenu(menuName = "DataHolders/StationLoadout")]
public class StationLoadout: ScriptableObject
{
    [Tooltip("Station type name")]
    public string modelName;
    [Tooltip("Owner faction of the station")]
    public Faction faction;

    [Tooltip("Equipment items for sale.")]
    public Equipment[] equipmentForSale;
    [Tooltip("Weapons for sale.")]
    public WeaponData[] weaponsForSale;

    [Tooltip("Is the cargo dealer available on this station type")]
    public bool hasCargoDealer = true;
    public bool hasRepairDealer = true;
    public bool hasShipDealer = false;
    public bool hasWeaponDealer = false;
    public bool hasEquipmentDealer = false;
    public bool hasBarDealer = false;
    public bool hasInfoBooth = true;
    
    [Tooltip("Ships for sale on this station. If empty or null, no ship dealer on station")]
    public GameObject[] shipsForSale;


    /// <summary>
    /// Gives a specific loadout to the specified ship
    /// </summary>
    /// <param name="loadout">The loadout to be applied on the station</param>
    /// <param name="station">The station to receive the loadout</param>
    public static void ApplyLoadoutToStation(StationLoadout loadout, Station station)
    {
        if (loadout == null)
            return;

        if (station.loadout.modelName != loadout.modelName)
        {
            Debug.LogWarning("Warning: Trying to apply " + loadout.modelName +
                " loadout to " + station.loadout.modelName);
            return;
        }
    }

    public static StationLoadout GetLoadoutByName(string name)
    {
        return Resources.Load<StationLoadout>("Stations/"+name);
    }
  
}

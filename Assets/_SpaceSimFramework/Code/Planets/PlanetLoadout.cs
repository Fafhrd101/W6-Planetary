using UnityEngine;

/// <summary>
/// Contains specifications of a certain station type
/// </summary>
[CreateAssetMenu(menuName = "DataHolders/PlanetLoadout")]
public class PlanetLoadout: ScriptableObject
{
    [Tooltip("Planet type name")]
    public string modelName;
    [Tooltip("Owner faction of the planet")]
    public Faction faction;

    [Header("Planetary Stats")] 
    public PlanetaryTypes planetType;
    public Material planetMaterial;
    
    [Range(0, 2)]
    public int numMoons = 0;
    [Range(0, 5)]
    public int size = 0;
    [Range(0, 11)]
    public int population = 0;
    public PlanetaryFunction function = PlanetaryFunction.None;
    [Range(0, 5)]
    public int lawLevel = 0;
    [Range(0, 5)]
    public int techLevel = 0;
    public PlanetaryShipyard shipyard = PlanetaryShipyard.None;
    public bool ring;
    public bool gasGiant;
    public bool storm;
    public bool clouds;
    public bool atmosphere;
    
    // Gives a specific loadout to the specified planet
    public static void ApplyLoadoutToPlanet(PlanetLoadout loadout, Planet planet)
    {
        if (loadout == null)
            return;

        if (planet.loadout.modelName != loadout.modelName)
        {
            Debug.LogWarning("Warning: Trying to apply " + loadout.modelName +
                " loadout to " + planet.loadout.modelName);
        }
    }
    // Gets a random ship loadout
    // public ShipLoadout GetRandomLoadout(string modelName)
    // {
    //     var possibleLoadouts = Array.FindAll(Loadouts, loadout => loadout.ShipModel == modelName);
    //     return possibleLoadouts.Length == 0 ? null : possibleLoadouts[UnityEngine.Random.Range(0, possibleLoadouts.Length)];
    // }
    public static PlanetLoadout GetLoadoutByName(string name)
    {
        return Resources.Load<PlanetLoadout>("Planets/"+name);
    }
}

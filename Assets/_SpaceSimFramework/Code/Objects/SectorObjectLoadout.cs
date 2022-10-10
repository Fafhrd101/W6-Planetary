using UnityEngine;

/// <summary>
/// Contains specifications of a certain station type
/// </summary>
[CreateAssetMenu(menuName = "DataHolders/SectorObjectLoadout")]
public class SectorObjectLoadout: ScriptableObject
{
    [Tooltip("SectorObject type name")]
    public string modelName;
    public static void ApplyLoadoutToSectorObject(SectorObjectLoadout loadout, SectorObject planet)
    {
        if (loadout == null)
            return;
    }

    public static SectorObjectLoadout GetLoadoutByName(string name)
    {
        return Resources.Load<SectorObjectLoadout>("SectorObjects/"+name);
    }
}

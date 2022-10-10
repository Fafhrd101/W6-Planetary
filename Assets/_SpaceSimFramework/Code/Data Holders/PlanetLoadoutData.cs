using UnityEngine;

[CreateAssetMenu(menuName = "DataHolders/PlanetLoadoutData")]
public class PlanetLoadoutData: ScriptableObject
{
    public PlanetLoadout[] planets;

    private static PlanetLoadoutData _instance;

    public static PlanetLoadoutData Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<PlanetLoadoutData>("Planet Loadout Data");

            if (_instance == null)
                Debug.LogError("ERROR: PlanetLoadoutData not found! Asset must be in the Resources folder!");
            return _instance;
        }
    }

}

using UnityEngine;

[CreateAssetMenu(menuName = "DataHolders/SectorVisualData")]
public class SectorVisualData: ScriptableObject
{
    public Flare[] flares;
    public Material[] skybox;

    private static SectorVisualData _instance;

    public static SectorVisualData Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<SectorVisualData>("Sector Visual Data");

            if (_instance == null)
                Debug.LogError("ERROR: SectorVisualData not found! Asset must be in the Resources folder!");
            return _instance;
        }
    }

    public int GetSkyboxIndex()
    {
        for (int x = 0; x < skybox.Length; x++)
            if (skybox[x].name == RenderSettings.skybox.name)
                return x;
        return 0;
    }
    public int GetFlareIndex()
    {
        Flare flare = GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare;
        for (int x = 0; x < flares.Length; x++)
            if (flares[x].name == flare.name)
                return x;
        return 0;
    }
    
}

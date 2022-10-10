using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SectorSaver
{
   
    public static void SaveSectorToFile(GameObject[] stations, GameObject[] planets, GameObject[] jumpgates, GameObject[] fields, GameObject[] sectorobjects, int sectorSize, string fileName)
    {
        if (!PlayArcadeIntegration.Instance.devBuild)
            return;
        
        var path = Application.persistentDataPath + fileName;
        SaveSectorToPath(stations, planets, jumpgates, fields, sectorobjects, sectorSize, path, fileName);
    }

    public static void SaveSectorToPath(GameObject[] stations, GameObject[] planets, GameObject[] jumpgates, GameObject[] fields, GameObject[] sectorobjects, int sectorSize, string path, string fileName = "")
    {
        SerializableSectorData data = new SerializableSectorData();

        // OBJECTS
        data.sectorObjects = new List<SerializableSectorObjectData>();
        foreach (var sectorobject in sectorobjects)
        {
            data.sectorObjects.Add(SerializableSectorObjectData.FromSectorObject(sectorobject.GetComponent<SectorObject>()));
            //Debug.Log("AO Saved");
        }
        
        // STATIONS
        data.stations = new List<SerializableStationData>();
        foreach (var station in stations)
        {
            data.stations.Add(SerializableStationData.FromStation(station.GetComponent<Station>()));
        }
        // PLANETS
        data.planets = new List<SerializablePlanetData>();
        foreach (var planet in planets)
        {
            data.planets.Add(SerializablePlanetData.FromPlanet(planet.GetComponent<Planet>()));
        }
        // ENVIRONMENT
        data.fields = new List<SerializableFieldData>();
        foreach (var fieldObj in fields)
        {
            data.fields.Add(SerializableFieldData.FromField(fieldObj.GetComponent<AsteroidField>()));
        }
        // JUMPGATES
        data.jumpgates = new List<SerializableGateData>();
        foreach (var gate in jumpgates)
        {
            data.jumpgates.Add(SerializableGateData.FromGate(gate.GetComponent<Jumpgate>()));
        }
        // NEBULAS
        Nebula nebula = Object.FindObjectOfType<Nebula>();
        if (nebula != null)
        {
            data.nebula = SerializableNebulaData.FromNebula(nebula);
        }
        // SYSTEM
        data.skyboxIndex = GetskyboxIndex();
        data.starIndex = GetSunIndex();
        //data.Size = sectorSize;

        Color skyboxColor = Color.white;
        if (RenderSettings.skybox.HasProperty("_Tint"))
            skyboxColor = RenderSettings.skybox.GetColor("_Tint");
        else if (RenderSettings.skybox.HasProperty("_SkyTint"))
            skyboxColor = RenderSettings.skybox.GetColor("_SkyTint");
        data.skyboxTint = new SerializableVector3(skyboxColor.r, skyboxColor.g, skyboxColor.b);
        
        
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.OpenOrCreate);
        formatter.Serialize(stream, data);
        stream.Close(); 
        Debug.Log("Sector saved to "+path);
    }

    private static int GetskyboxIndex()
    {
        for (int i = 0; i < SectorVisualData.Instance.skybox.Length; i++)
            if (SectorVisualData.Instance.skybox[i] == RenderSettings.skybox)
                return i;
        return 0;
    }

    private static int GetSunIndex()
    {
        Flare sun = GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare;

        for (int i = 0; i < SectorVisualData.Instance.flares.Length; i++)
            if (SectorVisualData.Instance.flares[i] == sun)
                return i;
        return 0;
    }

}

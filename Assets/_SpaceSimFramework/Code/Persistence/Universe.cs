using System;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

public class Universe : MonoBehaviour
{
    #region static functionality
    public static Dictionary<SerializableVector2, SerializableUniverseSector> Sectors;

    static Universe()
    {
        Sectors = LoadUniverse();
    }
    #endregion static functionality

    public static void ClearUniverse()
    {
        Universe.Sectors = new Dictionary<SerializableVector2, SerializableUniverseSector>();
    }
    public static Dictionary<SerializableVector2, SerializableUniverseSector> LoadUniverse()
    {
        string path = Path.Combine(Application.persistentDataPath, "Universe");
        //Debug.LogWarning("System Editor attempting to load local Universe file");

        string file = /*PlayArcadeIntegration.Instance.localLoads ? Utils.UNIVERSE_FILE : */path;

        Dictionary<SerializableVector2, SerializableUniverseSector> data =
            (Dictionary<SerializableVector2, SerializableUniverseSector>)Utils.LoadBinaryFile(file);
        if (data == null)
        {
            Debug.LogError("Universe data was null! Forced to create a new file!!");
            data = new Dictionary<SerializableVector2, SerializableUniverseSector>();
        } 
        // else
        //     print("Added " + data.Count + " sectors to Universe.");

        return data;
    }

    private static void SaveUniverse()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(Utils.UNIVERSE_FILE, FileMode.OpenOrCreate);
        formatter.Serialize(stream, Universe.Sectors);
        stream.Close();
        //print("Universe file saved with "+ (Universe.Sectors.Count) +" sectors");
    }

    public static void AddSector(Vector2 sectorPosition, List<GameObject> jumpgates, string name = "")
    {
        SerializableUniverseSector sectorData = new SerializableUniverseSector(
            name == "" ? "_x"+sectorPosition.x+"y"+sectorPosition.y : name,
            (int)sectorPosition.x,
            (int)sectorPosition.y,
            "Neutral"
            );

        foreach (GameObject gate in jumpgates)
        {
            sectorData.Connections.Add(gate.GetComponent<Jumpgate>().NextSector);
        }

        if (Sectors.ContainsKey(sectorPosition))
        {
            Sectors[sectorPosition] = sectorData;
        }
        else
        {
            Sectors.Add(sectorPosition, sectorData);
        }

        SaveUniverse();
    }

    public static void AddCurrentSector(Vector2 sectorPosition)
    {
        Sectors = LoadUniverse();
        AddSector(
            sectorPosition,
            SectorNavigation.Jumpgates,"_x" + sectorPosition.x + "y" + sectorPosition.y);        
    }

    public static List<SerializableUniverseSector> GetAdjacentSectors(Vector2 position)
    {
        List<SerializableUniverseSector> adjacent = new List<SerializableUniverseSector>();

        for(int i=-1; i<=1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if(i != 0 && j != 0)
                {
                    Sectors.TryGetValue(new Vector2(i, j), out var sector);
                    if (sector != null)
                        adjacent.Add(sector);
                }
            }
        }
        return adjacent;
    }
}

using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;

public class SaveGame
{
    public static void SaveAutosave(Vector2 nextSector)
    {
        if ((int)nextSector.x == -1 && (int)nextSector.y == -1)
        {
            //Debug.Log("No need to save lobby level!");
            return;
        }
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = null;

        try
        {
            // Save progress
            stream = new FileStream(Application.persistentDataPath + "PlayerData", FileMode.OpenOrCreate);
            SerializablePlayerData data = GetPlayerData(nextSector);
            formatter.Serialize(stream, data);
            stream.Close();
            //Debug.Log("Saved PlayerData");
            
            stream = new FileStream(Application.persistentDataPath + "Knowledge", FileMode.OpenOrCreate);
            formatter.Serialize(stream, UniverseMap.Knowledge);
            stream.Close();
            //Debug.Log("Saved Knowledge (" + UniverseMap.Knowledge.Count + ")");

        }
        catch (Exception e)
        {
            Debug.LogError("Error while saving Knowledge! (" + e.Message + ")\n" + e.StackTrace);
        }
        finally
        {
            if(stream != null)  // Clean up
            {
                stream.Close();
            }
        }
    }

    // Simply sets the sector we wish to jump to, so we can immediately load it after the scene
    public static void SaveAndJump(Vector2 nextSector)
    {
        SaveAutosave(nextSector);
    }

    private static SerializablePlayerData GetPlayerData(SerializableVector2 nextSector)
    {
        var data = new SerializablePlayerData
        {
            // General player info
            Name = Player.Instance.serverName,
            Credits = Player.Instance.credits,
            Rank = Progression.Level,
            Experience = Progression.Experience,
            CurrentSector = SectorNavigation.CurrentSector,
            pilotIconNumber = Player.Instance.pilotIconNumber,
            Kills = GetKillData(),
            Reputation = Player.Instance.GetReputations()
        };

        // Mission data
        if(MissionControl.CurrentJob != null) {
            data.Mission = SerializableMissionData.FromMission();
        }

        data.Ships = new List<SerializablePlayerShip>();
        foreach (var shipObj in Player.Instance.Ships)
        {
            Ship ship = shipObj.GetComponent<Ship>();

            RemoveEquipmentModifiers(ship);
            SerializablePlayerShip shipModel = SerializablePlayerShip.FromShip(ship, nextSector);
            ReturnEquipmentModifiers(ship);
            data.Ships.Add(shipModel);
        }

        // foreach (Player.ShipDescriptor OOSShip in Player.Instance.OOSShips)
        // {
        //     SerializablePlayerShip shipModel = SerializablePlayerShip.FromOOSShip(OOSShip);
        //     data.Ships.Add(shipModel);
        // }

        if(nextSector != SectorNavigation.UNSET_SECTOR)
            data.CurrentSector = nextSector;

        return data;
    }

    private static SerializableVector3[] GetKillData()
    {
        SerializableVector3[] kills = new SerializableVector3[ObjectFactory.Instance.Factions.Length];
        for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
        {
            Faction f = ObjectFactory.Instance.Factions[i];
            kills[i] = new SerializableVector3(Player.Instance.Kills[f].x, Player.Instance.Kills[f].y, Player.Instance.Kills[f].z);
        }
        return kills;
    }

    /// <summary>
    /// Remove effects of mounted equipment before saving to prevent tampering with saving and loading
    /// </summary>
    private static void RemoveEquipmentModifiers(Ship ship)
    {
        foreach (var item in ship.Equipment.MountedEquipment)
            item.RemoveItem(ship);
    }

    /// <summary>
    /// Return effects of mounted equipment after saving in case game was saved manually (scene continues)
    /// </summary>
    private static void ReturnEquipmentModifiers(Ship ship)
    {
        foreach (var item in ship.Equipment.MountedEquipment)
            item.InitItem(ship);
    }

}

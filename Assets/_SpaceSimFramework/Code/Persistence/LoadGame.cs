using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class LoadGame {

    public static void LoadAutosave()
    {
        // Player.Instance.Name = PlayerPrefs.GetString("name");
        // Player.Instance.Credits = PlayerPrefs.GetInt("credits");
        // Player.Instance.Level = PlayerPrefs.GetInt("rank");
        // Player.Instance.Experience = PlayerPrefs.GetInt("exp");
        // var x = PlayerPrefs.GetFloat("sector_x");
        // var y = PlayerPrefs.GetFloat("sector_y");
        // Vector2 currentSector = new Vector2(x, y);
        // Player.Instance.CurrentSector = currentSector;
        
        //Debug.Log("Autosave has loaded the player values. Current sector: " + currentSector);

        //return;
        BinaryFormatter formatter = new BinaryFormatter();
        
        if (!File.Exists(Application.persistentDataPath + "PlayerData"))
        {
            Debug.Log("PlayerData file doesn't exist!");
            PlayArcadeIntegration.Instance.loadAutosave = false;
            return;
        }

        FileStream stream = new FileStream(Application.persistentDataPath + "PlayerData", FileMode.Open);

        SerializablePlayerData data = formatter.Deserialize(stream) as SerializablePlayerData;
        stream.Close();

        if (data != null)
        {
            Player.Instance.serverName = data.Name;
            Player.Instance.level = data.Rank;
            Player.Instance.experience = data.Experience;
            Player.Instance.credits = (int) data.Credits;
            // Slight hack. We couldn't save the icon when created; we had to carry it over. So we'll just 
            // pretend 0 is an invalid number.
            if (data.pilotIconNumber > 0)
                Player.Instance.pilotIconNumber = data.pilotIconNumber;
            //Debug.Log("Loading "+data.pilotIconNumber+ "saved icon");
            Player.Instance.currentSector = data.CurrentSector;
            Player.Instance.previousSector = data.previousSector;
            Player.Instance.previousPosition = data.previousPosition;
            // Parse and set reputation
            Faction PF = Player.Instance.playerFaction;
            var r = 0;
            for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
            {
                if (ObjectFactory.Instance.Factions[i] != Player.Instance.playerFaction)
                {
                    ObjectFactory.Instance.Factions[i].Cache[PF] = data.Reputation[r];
                    PF.Cache[ObjectFactory.Instance.Factions[i]] = data.Reputation[r++];
                }
            }


            Player.Instance.Kills = new Dictionary<Faction, Vector3>();
            for (int i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
            {
                Player.Instance.Kills.Add(ObjectFactory.Instance.Factions[i], data.Kills[i]);
            }
            
            // Mission loading
            if (data.Mission != null)
            {
                // Generic Mission data
                Faction employer = ObjectFactory.Instance.GetFactionFromName(data.Mission.Employer);
                var payout = data.Mission.Payout;
                var timestamp = data.Mission.TimeStarted;
                Vector2 sector = data.Mission.Sector;
            
                if (data.Mission.Type == Mission.JobType.Assassinate.ToString())
                {
                    MissionControl.CurrentJob = new Assassination(employer, payout, timestamp, sector);
                }
                else if (data.Mission.Type == Mission.JobType.Patrol.ToString())
                {
                    MissionControl.CurrentJob = new Patrol(employer, payout, timestamp, sector);
            
                    SerializableVector2 killCount = (SerializableVector2) data.Mission.GetData("KillCount");
                    ((Patrol) MissionControl.CurrentJob).Kills = new Vector2(killCount.x, killCount.y);
                }
                else if (data.Mission.Type == Mission.JobType.CargoDelivery.ToString())
                {
                    MissionControl.CurrentJob = new CargoDelivery(employer, payout, timestamp, sector);
            
                    CargoDelivery job = ((CargoDelivery) MissionControl.CurrentJob);
                    job.StationID = (string) data.Mission.GetData("StationID");
                    job.Station = sector == SectorNavigation.CurrentSector
                        ? SectorNavigation.GetStationByID(job.StationID)
                        : null;
                    job.Amount = (int) data.Mission.GetData("Amount");
                    job.Ware = (string) data.Mission.GetData("Ware");
                }
                else if (data.Mission.Type == Mission.JobType.Courier.ToString())
                {
                    MissionControl.CurrentJob = new Courier(employer, payout, timestamp, sector);
            
                    Courier job = ((Courier) MissionControl.CurrentJob);
                    job.StationID = (string) data.Mission.GetData("StationID");
                    job.Station = sector == SectorNavigation.CurrentSector
                        ? SectorNavigation.GetStationByID(job.StationID)
                        : null;
                    job.Amount = (int) data.Mission.GetData("Amount");
                    job.Ware = (string) data.Mission.GetData("Ware");
                }
            
                MissionControl.CurrentJob.Duration = data.Mission.Duration;
            }

            // Load Ships
            /*foreach (var shipModel in data.Ships)
            {
                // Spawn ship in-sector
                if (shipModel.Sector == SectorNavigation.CurrentSector)
                {
                    Vector3 position = shipModel.Position;
                    var rotation = Quaternion.Euler(shipModel.Rotation);
                    var shipObj = GameObject.Instantiate(ObjectFactory.Instance.GetShipByName(shipModel.Model),
                        position, rotation, Player.Instance.transform);

                    Ship ship = shipObj.GetComponent<Ship>();
                    ship.faction = Player.Instance.PlayerFaction;
                    ship.armor = shipModel.Armor;
                    ship.isPlayerControlled = shipModel.IsPlayerShip;
                    DockShipToStation(ship, shipModel.StationDocked);

                    // Weapons
                    GunHardpoint hardpoint;
                    int w_i = 0;
                    foreach (string weaponName in shipModel.Guns)
                    {
                        hardpoint = ship.Equipment.Guns[w_i];
                        hardpoint.SetWeapon(ObjectFactory.Instance.GetWeaponByName(weaponName));
                        w_i++;
                    }

                    w_i = 0;
                    foreach (string weaponName in shipModel.Turrets)
                    {
                        hardpoint = ship.Equipment.Turrets[w_i];
                        hardpoint.SetWeapon(ObjectFactory.Instance.GetWeaponByName(weaponName));
                        w_i++;
                    }

                    // Equipment
                    int item_i = 0;
                    foreach (string itemName in shipModel.Equipment)
                    {
                        ship.Equipment.MountEquipmentItem(ObjectFactory.Instance.GetEquipmentByName(itemName));
                        item_i++;
                    }

                    // Cargo
                    ship.ShipCargo.RemoveCargo();
                    foreach (SerializableCargoItem cargoItem in shipModel.Cargo)
                    {
                        ship.ShipCargo.AddWare(
                            (HoldItem.CargoType) Enum.Parse(typeof(HoldItem.CargoType), cargoItem.Type),
                            cargoItem.Item, cargoItem.Amount);
                    }

                    if (ship.isPlayerControlled)
                    {
                        Camera.main.GetComponent<CameraController>().SetTargetShip(ship);
                        Ship.PlayerShip = ship;
                        if (ship.transform.position == Vector3.zero && SectorNavigation.PreviousSector != null)
                        {
                            foreach (GameObject jg in GameObject.FindGameObjectsWithTag("Jumpgate"))
                            {
                                if (jg.GetComponent<Jumpgate>().NextSector != SectorNavigation.PreviousSector) continue;
                                ship.transform.position = jg.GetComponent<Jumpgate>().SpawnPos.position;
                                break;
                            }
                        }

                        EquipmentIconUI.Instance.SetIconsForShip(ship);
                    }
                }
                else
                {
                    // Spawn ship out-of-sector
                    var shipOOS = new Player.ShipDescriptor
                    {
                        Armor = shipModel.Armor,
                        modelName = shipModel.Model,
                        Sector = shipModel.Sector,
                        StationDocked = shipModel.StationDocked,
                        Position = shipModel.Position,
                        Rotation = Quaternion.Euler(shipModel.Rotation)
                    };

                    var w_i = 0;
                    shipOOS.Guns = new WeaponData[shipModel.Guns.Length];
                    foreach (string weaponName in shipModel.Guns)
                    {
                        shipOOS.Guns[w_i++] = ObjectFactory.Instance.GetWeaponByName(weaponName);
                    }

                    w_i = 0;
                    shipOOS.Turrets = new WeaponData[shipModel.Turrets.Length];
                    foreach (string weaponName in shipModel.Turrets)
                    {
                        shipOOS.Turrets[w_i++] = ObjectFactory.Instance.GetWeaponByName(weaponName);
                    }

                    shipOOS.MountedEquipment = new Equipment[shipModel.Equipment.Length];
                    var item_i = 0;
                    foreach (string eqItem in shipModel.Equipment)
                    {
                        shipOOS.MountedEquipment[item_i++] = ObjectFactory.Instance.GetEquipmentByName(eqItem);
                    }

                    shipOOS.CargoItems = new HoldItem[shipModel.Cargo.Count];
                    var cargo_i = 0;
                    foreach (SerializableCargoItem cargoitem in shipModel.Cargo)
                    {
                        shipOOS.CargoItems[cargo_i++] = new HoldItem(cargoitem.Type, cargoitem.Item, cargoitem.Amount);
                    }

                    Player.Instance.OOSShips.Add(shipOOS);
                }
            }*/
        }

        //Debug.Log("PlayerData loaded");
    }

    /// <summary>
    /// Spawns the ship as docked to the station
    /// </summary>
    private static void DockShipToStation(Ship ship, string stationID)
    {
        ship.stationDocked = stationID;
        if (stationID == "none")
            return;

        // Find station with name
        var dockables = GameObject.FindGameObjectsWithTag("Station");
        foreach(var obj in dockables)
        {
            if(obj.name == stationID)
            {
                // If station is found, dock ship to station
                obj.GetComponent<Station>().ForceDockShip(ship);
            }
        }
    }

    public static void LoadPlayerKnowledge()
    {
        Dictionary<SerializableVector2, SerializableSectorData> data = 
            (Dictionary<SerializableVector2, SerializableSectorData>)Utils.LoadBinaryFile(Application.persistentDataPath + "Knowledge");
        if (data == null)
        {
            Debug.Log("Loading NewKnowledge");
            data = new Dictionary<SerializableVector2, SerializableSectorData>();
        }

        UniverseMap.Knowledge = data;
    }

    private static void ParseReputation(string rep)
    {
        var relations = rep.Split(' ');
        var r = 0;
        var pf = Player.Instance.playerFaction;
        foreach (var t in ObjectFactory.Instance.Factions)
        {
            if (t != Player.Instance.playerFaction)
            {
                t.Cache[pf] = float.Parse(relations[r]);
                pf.Cache[t] = float.Parse(relations[r++]);
            }
        }
    }

    private static void ParseKills(string kills)
    {
        var relations = kills.Split(' ');

        Player.Instance.Kills = new Dictionary<Faction, Vector3>();
        for (var i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
        {
            var fighterKills = int.Parse(relations[i].Split('-')[0]);
            var capKills = int.Parse(relations[i].Split('-')[1]);
            var statKills = int.Parse(relations[i].Split('-')[2]);
            Player.Instance.Kills.Add(ObjectFactory.Instance.Factions[i], new Vector3(fighterKills, capKills, statKills));
        }
    }
}

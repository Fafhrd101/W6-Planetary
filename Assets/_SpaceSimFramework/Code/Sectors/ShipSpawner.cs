using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

/// <summary>
/// Attach to jumpgates. Spawns ships periodically.
/// </summary>
public class ShipSpawner : MonoBehaviour 
{
    public int shipNumberLimit = 30;
    [Header("Spawn properties")]
    public float spawnTimeSpacing = 10f;
    public float probabilitySquadron = 0.4f;
    [Tooltip("Escorts, if needed, are always small fighters")]
    public GameObject[] fighterPrefabs;
    //public bool 
    private float _spawnTimer;
    private Transform _spawnPos;
    private readonly Vector3[] _escortOffsets = { new Vector3(30, 0, 0), new Vector3(30, 30, 0), new Vector3(0, 30, 0)};
    private GameObject[] _shipPrefabs;
    public Jumpgate GO;
    
    private void Start()
    {
        _spawnTimer = spawnTimeSpacing + Random.value * 10;
        _spawnPos = GetComponent<Jumpgate>().SpawnPos;
        _shipPrefabs = ObjectFactory.Instance.Ships;
    }

    private void Update()
    {
        bool isRaider = false;
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer < 0)
        {
            _spawnTimer = spawnTimeSpacing + Random.Range(-2, 2);
            if (SectorNavigation.Ships.Count > shipNumberLimit)
                return;

            for (var x = 0; x < Random.Range(1, 3); x++)
            {
                if (SpawnRandomShip())
                    isRaider = true;
            }

            List<GameObject> stations = SectorNavigation.Instance.GetClosestStation(this.transform, Ship.PlayerShip.shipModelInfo.ScannerRange, 1);
            
            if (stations.Count > 0 && isRaider)
            {
                var broadcaster = SectorNavigation.Instance.GetRandomStation();
                if (Ship.PlayerShip != null && 
                    Vector3.Distance(broadcaster.transform.position, Ship.PlayerShip.transform.position) < 1000)
                    StationBroadcastAnnouncement.Instance.StationAnnouncement(broadcaster.GetComponent<Station>(),
                        "...All nearby ships...be advised Raiders have entered our local gate...");
            }
        }
    }

    // returns true if it spawns a Raider
    private bool SpawnRandomShip()
    {
        bool isRaider = false;
        string debugText = "";
        int shipToSpawn;
        while (true)
        {
            shipToSpawn = Random.Range(0, _shipPrefabs.Length);
            if (Random.value < _shipPrefabs[shipToSpawn].GetComponent<Ship>().shipModelInfo.rarity)
                break;
        }

        Ship ship;
        if (SceneManager.GetActiveScene().name == "StartScenario")
            ship = GameObject.Instantiate(
                fighterPrefabs[Random.Range(0, fighterPrefabs.Length)],
                _spawnPos.position,
                _spawnPos.rotation).GetComponent<Ship>();
        else
            ship = GameObject.Instantiate(
            _shipPrefabs[shipToSpawn],
            _spawnPos.position,
            _spawnPos.rotation).GetComponent<Ship>();

        ship.isPlayerControlled = false;

        if (Random.value < 0.3)
        {
            ship.faction = ObjectFactory.Instance.GetFactionFromName("Raider");
            isRaider = true;
        }
        else if (Random.value < 0.2)
            ship.faction = ObjectFactory.Instance.GetFactionFromName("Freelancer");
        else if (Random.value < 0.4)
            ship.faction = ObjectFactory.Instance.GetFactionFromName("EarthAlliance");
        else
            ship.faction = GetShipFaction();
        
        var shipGO = ship.gameObject;
        shipGO.name = ship.shipModelInfo.modelName;
        ship.name = ship.faction.name + " "+ ship.shipModelInfo.modelName;
        debugText += "Spawning " + ship.name;
        // Give a random loadout
        //ShipLoadout.ApplyLoadoutToShip(shipFaction.GetRandomLoadout(ship.ShipModelInfo.modelName), ship);

        debugText += ", faction is " + ship.faction.name;

        // Assign order to ship
        debugText = AIShipController.IssueOrder(shipGO, debugText, this.gameObject);
        //print(debugText);
        // Generate escort ships, if needed
        if (Random.value < probabilitySquadron && ship.name.Contains("Freighter"))
        {
            SpawnEscortsFor(ship);
        }

        return isRaider;
    }

    /// <summary>
    /// Gets the ship's faction taking into account sector ownership.
    /// If sector is under complete ownership, 50% chance of spawning owner's ship.
    /// If sector is under no control, 100% random selection
    /// </summary>
    private static Faction GetShipFaction()
    {
        Faction[] factions = ObjectFactory.Instance.Factions;

        // Start sector only gets hostiles
        if (SceneManager.GetActiveScene().name != "StartScenario")
            return factions[Random.Range(0, factions.Length - 1)];  // Do not spawn player ships
        return ObjectFactory.Instance.GetFactionFromName("Raider");
    }

    private void SpawnEscortsFor(Ship escortLeader)
    {
        int numEscorts = Random.Range(1, 3); // 5

        for (int e_i = 0; e_i < numEscorts; e_i++)
        {
            // Spawn random ship
            var escort = GameObject.Instantiate(
                fighterPrefabs[Random.Range(0, fighterPrefabs.Length)],
                _spawnPos.position,
                _spawnPos.rotation).GetComponent<Ship>();
            escort.isPlayerControlled = false;
            // Generate random faction
            escort.faction = escortLeader.faction;
            // Give a random loadout
            // ShipLoadout.ApplyLoadoutToShip(escortLeader.faction.GetRandomLoadout(escort.ShipModelInfo.modelName), escort);
            // Shouldn't see Delta ships...
            string letter = e_i == 0 ? "Alpha" : e_i == 1 ? "Beta" : e_i == 2 ? "Gamma" : e_i == 3 ? "Delta"
                : e_i == 4 ? "Epsilon" : "Zeta" ;
            var escortGO = escort.gameObject;
            escortGO.name = "Escort" + letter + "-" + escort.shipModelInfo.modelName;
            escort.name = "Escort" + letter + "-" + escort.shipModelInfo.modelName;
            // Assign order to ship
            escortGO.GetComponent<ShipAI>().Follow(escortLeader.transform);
            escortGO.transform.position += _escortOffsets[e_i];
        }
        
    }
    
    public GameObject SpawnMissionTarget(Faction faction)
    {
        var ship = GameObject.Instantiate(
            _shipPrefabs[Random.Range(0, _shipPrefabs.Length)],
            _spawnPos.position,
            _spawnPos.rotation).GetComponent<Ship>();
        ship.isPlayerControlled = false;

        // Generate random faction
        var factions = ObjectFactory.Instance.Factions;        
        ship.faction = faction;
        ship.gameObject.name = faction.name + " " + ship.shipModelInfo.modelName + "*";

        // Assign order to ship
        ship.gameObject.GetComponent<ShipAI>().AttackAll();

        // Generate escort ships
        SpawnEscortsFor(ship);

        return ship.gameObject;
    }

}

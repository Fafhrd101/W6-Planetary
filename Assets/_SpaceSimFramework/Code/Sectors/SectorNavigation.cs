using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Keeps references to all interactable game objects in the current sector (scene).
/// Is used to obtain ships, stations, jumpgates, cargo crates, etc.
/// </summary>
public class SectorNavigation : Singleton<SectorNavigation> {

    public static Vector2 UNSET_SECTOR = new Vector2(9999, 9999);
    public static int SECTORSIZE = 2500; // for streaming to new sector

    public static Vector2 CurrentSector => _currentSector;
    private static Vector2 _currentSector = Vector2.zero;
    public Vector2 thisSector;
    public Faction thisFaction;
    public static Vector2 PreviousSector => _previousSector;
    private static Vector2 _previousSector = UNSET_SECTOR;

    public static int sectorSize;
    public static List<GameObject> Ships;
    public List<GameObject> knownShips;
    public List<GameObject> closestShips;
    public static List<GameObject> Stations;
    public List<GameObject> knownStations;
    public List<GameObject> closestStations;
    public static List<GameObject> Planets;
    public List<GameObject> knownPlanets;
    public static List<GameObject> Cargo;
    public static List<GameObject> Jumpgates;
    public List<GameObject> knownJumpgates;
    public static List<GameObject> Fields;
    public static List<GameObject> SectorObjects;
    
    private static Dictionary<string, GameObject> _stationIDs;
    private static Dictionary<string, GameObject> _planetIDs;
    private static Dictionary<string, GameObject> _fieldIDs;
    private static Dictionary<string, GameObject> _jumpgateIDs;
    private static Dictionary<string, GameObject> _sectorObjectIDs;

    public List<string> abundantGoods;
    public List<string> desiredGoods;


    /// <summary>
    /// Sets the current sector when jumping or loading game.
    /// </summary>
    /// <param name="newSector">Sector to be set</param>
    /// <param name="markPreviousSector">Set true if jumping and false if loading</param>
    public static void ChangeSector(Vector2 newSector, bool markPreviousSector)
    {
        if(markPreviousSector)
            _previousSector = _currentSector;
        _currentSector = newSector;
        Ships = new List<GameObject>();
        Stations = new List<GameObject>();
        Planets = new List<GameObject>();
        Cargo = new List<GameObject>();
        Jumpgates = new List<GameObject>();
        Fields = new List<GameObject>();
        SectorObjects = new List<GameObject>();

        Instance.Awake();
    }

    private static void GetExistingObjects()
    {
        //print("Counting loaded objs in sector");
        Ships = new List<GameObject>();
        Stations = new List<GameObject>();
        Planets = new List<GameObject>();
        Cargo = new List<GameObject>();
        Jumpgates = new List<GameObject>();
        Fields = new List<GameObject>();
        SectorObjects = new List<GameObject>();
        
        _stationIDs = new Dictionary<string, GameObject>();
        _fieldIDs = new Dictionary<string, GameObject>();
        _jumpgateIDs = new Dictionary<string, GameObject>();
        _planetIDs = new Dictionary<string, GameObject>();
        _sectorObjectIDs = new Dictionary<string, GameObject>();
        
        // Find all pre-existing sector entities
        Ships.AddRange(GameObject.FindGameObjectsWithTag("Ship"));
        Stations.AddRange(GameObject.FindGameObjectsWithTag("Station"));
        Planets.AddRange(GameObject.FindGameObjectsWithTag("Planet"));
        Jumpgates.AddRange(GameObject.FindGameObjectsWithTag("Jumpgate"));
        Cargo.AddRange(GameObject.FindGameObjectsWithTag("Cargo"));
        Fields.AddRange(GameObject.FindGameObjectsWithTag("AsteroidField"));
        SectorObjects.AddRange(GameObject.FindGameObjectsWithTag("SectorObject"));
        
        var count = 0;
        foreach (var station in Stations)
        {
            _stationIDs.Add(station.GetComponent<Station>().id, station);
            count++;
        }
        foreach (var planet in Planets)
        {
            _planetIDs.Add(planet.GetComponent<Planet>().id, planet);
            count++;
        }
        foreach (var jumpgate in Jumpgates)
        {
            _jumpgateIDs.Add(jumpgate.GetComponent<Jumpgate>().id, jumpgate);
            count++;
        }
        foreach (var field in Fields)
        {
            _fieldIDs.Add(field.GetComponent<AsteroidField>().ID, field);
            count++;
        }
        foreach (var obj in SectorObjects)
        {
            _sectorObjectIDs.Add(obj.GetComponent<SectorObject>().ID, obj);
            count++;
        }
        //Debug.Log("Counted "+count+" objs");
    }

    /*
     * Getters for objects in sector, by ID. Return null if object not found in sector, gameObject otherwise.
     */
    public static GameObject GetStationByID(string id)
    {
        if (string.IsNullOrEmpty(id) || id=="none")
            return null;

        return _stationIDs.ContainsKey(id) ? _stationIDs[id] : null;
    }
    public static GameObject GetPlanetByID(string id)
    {
        if (string.IsNullOrEmpty(id) || id=="none")
            return null;

        return _planetIDs.ContainsKey(id) ? _planetIDs[id] : null;
    }
    public static GameObject GetJumpgateByID(string id)
    {
        return _jumpgateIDs.ContainsKey(id) ? _jumpgateIDs[id] : null;
    }

    public static GameObject GetFieldByID(string id)
    {
        return _fieldIDs.ContainsKey(id) ? _fieldIDs[id] : null;
    }
    [Tooltip("How often do player ships check their surroundings to discover new sector objects?")]
    public float objectDiscoveryInterval = 3f;
    private float _discoveryTimer;
    private List<string> _knownIds;
    private SerializableSectorData _sectorKnowledge;

    public void Awake()
    {
        if (SceneManager.GetActiveScene().name == "EmptyPlanet")
            return;
        GetExistingObjects();
        SceneManager.sceneLoaded += OnSceneLoaded;
        _discoveryTimer = objectDiscoveryInterval;
        thisSector = _currentSector;

        if (UniverseMap.Knowledge == null)
        {
            LoadGame.LoadPlayerKnowledge();
        }

        if (UniverseMap.Knowledge != null && UniverseMap.Knowledge.ContainsKey(Player.Instance.currentSector))
        {
            _sectorKnowledge = UniverseMap.Knowledge[Player.Instance.currentSector];
            //print("Knowledge = "+UniverseMap.Knowledge.Count);
        }
        else
        {
            _sectorKnowledge = new SerializableSectorData();
            _sectorKnowledge.jumpgates ??= new List<SerializableGateData>();
            _sectorKnowledge.stations ??= new List<SerializableStationData>();
            _sectorKnowledge.planets ??= new List<SerializablePlanetData>();
            _sectorKnowledge.fields ??= new List<SerializableFieldData>();

            if (UniverseMap.Knowledge != null) 
                UniverseMap.Knowledge.Add(Player.Instance.currentSector, _sectorKnowledge);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Usually called twice. Once immediately upon loading the scene, and again when we're actually ready for it.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GetExistingObjects();
        StationContentGenerator.OnSectorChanged(_currentSector, Stations);
    }

    private void Update()
    {
        if (Ship.PlayerShip.isDestroyed)
            return;
        
        // Check for new detected objects each few seconds
        _discoveryTimer -= Time.deltaTime;

        if(_discoveryTimer < 0)
        {
            _discoveryTimer = objectDiscoveryInterval;

            if (_knownIds == null)
                GetKnownObjectIds(_sectorKnowledge);

            foreach (var ship in Player.Instance.Ships)
            {
                // Ignore docked ships
                if (!ship.activeInHierarchy)
                    continue;

                Ship shipScript = ship.GetComponent<Ship>();
                // Get all stations/jumpgates/ships/cargo/planets in scanner range of this ship
                var objectsInRange = 
                    GetClosestObjects(ship.transform, shipScript.shipModelInfo.ScannerRange, Int32.MaxValue);

                foreach(GameObject objFound in objectsInRange)
                {
                    Station station = objFound.GetComponent<Station>();
                    if (station != null && !_knownIds.Contains(station.id))
                    {
                        SerializableStationData stationDescriptor = new SerializableStationData
                        {
                            ID = station.id,
                            loadoutName = station.loadout.name
                        };
                        var transform1 = station.transform;
                        stationDescriptor.position = transform1.position;
                        stationDescriptor.rotation = transform1.rotation.eulerAngles;
                        // Add station
                        _sectorKnowledge.stations.Add(stationDescriptor);
                        // Add ID
                        _knownIds.Add(station.id);

                        continue;
                    }
                    Planet planet = objFound.GetComponent<Planet>();
                    if (planet != null && !_knownIds.Contains(planet.id))
                    {
                        SerializablePlanetData planetDescriptor = new SerializablePlanetData
                        {
                            ID = planet.id
                        };
                        if (planet.loadout)
                            planetDescriptor.loadoutName = planet.loadout.name;
                        var transform1 = planet.transform;
                        planetDescriptor.position = transform1.position;
                        planetDescriptor.rotation = transform1.rotation.eulerAngles;
                        // Add station
                        _sectorKnowledge.planets.Add(planetDescriptor);
                        // Add ID
                        _knownIds.Add(planet.id);

                        continue;
                    }
                    Jumpgate gate = objFound.GetComponent<Jumpgate>();
                    if (gate != null && !_knownIds.Contains(gate.id))
                    {
                        var transform1 = gate.transform;
                        SerializableGateData gateDescriptor = new SerializableGateData
                        {
                            ID = gate.id,
                            sector = gate.NextSector,
                            position = transform1.position,
                            rotation = transform1.rotation.eulerAngles
                        };
                        // Add gate
                        _sectorKnowledge.jumpgates.Add(gateDescriptor);
                        // Add ID
                        _knownIds.Add(gate.id);
                    }
                }

                foreach (var field in GameObject.FindGameObjectsWithTag("AsteroidField"))
                {
                    float dist = Vector3.Distance(field.transform.position, ship.transform.position);
                    AsteroidField fieldProps = field.GetComponent<AsteroidField>();
                    if (dist < shipScript.shipModelInfo.ScannerRange && !_knownIds.Contains(fieldProps.ID))
                    {
                        var transform1 = fieldProps.transform;
                        SerializableFieldData fieldDescriptor = new SerializableFieldData
                        {
                            ID = fieldProps.ID,
                            position = transform1.position,
                            rotation = transform1.rotation.eulerAngles
                        };
                        // Add Field
                        _sectorKnowledge.fields.Add(fieldDescriptor);
                        // Add ID
                        _knownIds.Add(fieldDescriptor.ID);
                    }
                }
            }

            knownJumpgates = Jumpgates;
            knownPlanets = Planets;
            knownShips = Ships;
            knownStations = Stations;
        }
    }

    private void GetKnownObjectIds(SerializableSectorData sectorKnowledge)
    {
        _knownIds = new List<string>();

        if(sectorKnowledge.stations != null)
        {
            foreach (var station in sectorKnowledge.stations)
                _knownIds.Add(station.ID);
        }
        if(sectorKnowledge.planets != null)
        {
            foreach (var planet in sectorKnowledge.planets)
                _knownIds.Add(planet.ID);
        }
        if (sectorKnowledge.fields != null)
        {
            foreach (var field in sectorKnowledge.fields)
                _knownIds.Add(field.ID);
        }
           
        if(sectorKnowledge.jumpgates != null) { 
            foreach (var gate in sectorKnowledge.jumpgates)
                _knownIds.Add(gate.ID);
        }
    }


    /// <summary>
    /// Returns a required number of selectable objects (ships, stations, loot, etc.)
    /// within a desired range of a given object.
    /// </summary>
    /// <param name="shipPosition">Source of the scanner</param>
    /// <param name="scannerRange">Range of the scanner</param>
    /// <param name="num">Maximum number of required targets</param>
    /// <returns></returns>
    private List<GameObject> GetClosestObjects(Transform shipPosition, float scannerRange, int num)
    {
        var objectsInRange = new List<GameObject>();
        foreach (var cargo in Cargo.Where(cargo => Vector3.Distance(shipPosition.position, cargo.transform.position) < scannerRange))
        {
            objectsInRange.Add(cargo);
            num--;

            if (num <= 0)
                return objectsInRange;
        }       

        objectsInRange.AddRange(GetShipsInRange(shipPosition, scannerRange, num));

        foreach (GameObject obj in Stations)
        {
            if (Vector3.Distance(shipPosition.position, obj.transform.position) < scannerRange)
            {
                objectsInRange.Add(obj);
                num--;

                if (num <= 0)
                    return objectsInRange;
            }
        }
        foreach (GameObject obj in Planets)
        {
            if (Vector3.Distance(shipPosition.position, obj.transform.position) < scannerRange)
            {
                objectsInRange.Add(obj);
                num--;

                if (num <= 0)
                    return objectsInRange;
            }
        }
        foreach (GameObject obj in Jumpgates)
        {
            if (Vector3.Distance(shipPosition.position, obj.transform.position) < scannerRange)
            {
                objectsInRange.Add(obj);
                num--;

                if (num <= 0)
                    return objectsInRange;
            }
        }

        return objectsInRange;
    }
	
    public List <GameObject> GetClosestStation(Transform shipPosition, float scannerRange, int num)
    {
        List<GameObject> objectsInRange = new List<GameObject>();

        foreach (GameObject obj in Stations)
        {
            if (Vector3.Distance(shipPosition.position, obj.transform.position) < scannerRange)
            {
                objectsInRange.Add(obj);
                // num--;
                //
                // if (num <= 0)
                //     return objectsInRange;
            }
        }
        objectsInRange = objectsInRange.OrderBy(
            x => x.GetComponent<Station>().distanceToPlayerShip
        ).ToList();
        closestStations = objectsInRange;
        return objectsInRange;
    }
    public GameObject GetRandomStation()
    {
        return Stations[Random.Range(0, Stations.Count)];
    }
	
    /// <summary>
    /// Returns a required number of dynamic selectable objects (ships and loot)
    /// within a desired range of a given object.
    /// </summary>
    /// <param name="shipPosition">Source of the scanner</param>
    /// <param name="scannerRange">Range of the scanner</param>
    /// <param name="num">Maximum number of required targets</param>
    /// <returns></returns>
    public List<GameObject> GetClosestShipsAndCargo(Transform shipPosition, float scannerRange, int num)
    {
        List<GameObject> objectsInRange = new List<GameObject>();
        foreach (GameObject cargo in Cargo)
        {
            if (Vector3.Distance(shipPosition.position, cargo.transform.position) < scannerRange)
            {
                objectsInRange.Add(cargo);
                num--;

                if (num <= 0)
                    return objectsInRange;
            }
        }

        objectsInRange.AddRange(GetShipsInRange(shipPosition, scannerRange, num));
        return objectsInRange;
    }
    
    public static List<GameObject> GetShipsInRange(Transform shipPosition, float scannerRange, int num)
    {
        var objectsInRange = new List<GameObject>();

        foreach (GameObject ship in Ships)
        {
            if (shipPosition != null && ship != null)
            {
                var shipPos = shipPosition.position;
                shipPos.y = 0;
                var mePos = ship.transform.position;
                mePos.y = 0;
                if (Vector3.Distance(shipPos, mePos) < scannerRange)
                {
                    if (ship != shipPosition.gameObject && ship.activeInHierarchy)
                    {
                        objectsInRange.Add(ship);
                        num--;
                    }

                    if (num <= 0)
                        return objectsInRange;
                }
            }
        }
        return objectsInRange;
    }
    
    public static List<GameObject> GetClosestNPCShip(Transform shipPosition, float scannerRange)
    {
        var shipDistances = new Dictionary<GameObject, float>();
        Faction myfaction = null;
        if (shipPosition.gameObject.GetComponent<Ship>() != null)
            myfaction = shipPosition.gameObject.GetComponent<Ship>().faction;
        else if (shipPosition.gameObject.GetComponent<Station>() != null)
            myfaction = shipPosition.gameObject.GetComponent<Station>().faction;
        else
            myfaction = ObjectFactory.Instance.GetFactionFromName("Freelancer");

        foreach (GameObject ship in Ships)
        {
            if (ship.GetComponent<Ship>().stationDocked != "none")
                continue;

            var distance = Vector3.Distance(shipPosition.position, ship.transform.position);
            if (distance < scannerRange)
            {
                if (ship == shipPosition.gameObject)
                    continue;

                Faction shipFaction = ship.GetComponent<Ship>().faction;

                if(myfaction.RelationWith(shipFaction) < 0)
                    if (!shipDistances.ContainsKey(ship))
                        shipDistances.Add(ship, distance);
            }
        }

        // Sort by distance to get closest targets
        List<KeyValuePair<GameObject, float>> shipList = shipDistances.ToList();

        shipList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

        List<GameObject> closestContacts = new List<GameObject>();

        foreach(KeyValuePair<GameObject, float> pair in shipList){
            closestContacts.Add(pair.Key);
        }

        return closestContacts; 
    }

    public static List<GameObject> GetClosestVisibleTarget(Transform shipPosition, float scannerRange)
    {
        var objectsInRange = new List<GameObject>();
        foreach (GameObject ship in Ships)
        {
            if (shipPosition != null && ship != null)
            {
                var shipPos = shipPosition.position;
                shipPos.y = 0;
                var mePos = ship.transform.position;
                mePos.y = 0;
                if (Vector3.Distance(shipPos, mePos) < scannerRange)
                {
                    if (ship != shipPosition.gameObject && ship.activeInHierarchy && ship.GetComponentInChildren<Renderer>().isVisible)
                    {
                        objectsInRange.Add(ship);
                    }
                }
            }
        }
        return objectsInRange;
    }
    
    public static int GetTotalNPCShips()
    {
        int count = 0;
        Faction myfaction = ObjectFactory.Instance.GetFactionFromName("Player");
        foreach (GameObject ship in Ships)
        {
            Faction shipFaction = ship.GetComponent<Ship>().faction;
            if (myfaction.RelationWith(shipFaction) < 0)
                count++;
        }

        return count;
    }

    public static Transform[] GetPatrolWaypoints()
    {
        var waypoints = new List<Transform>();

        foreach (GameObject child in GameObject.FindGameObjectsWithTag("Waypoint"))
        {
             waypoints.Add(child.transform);
        }

        if(waypoints.Count == 0)    // Create some waypoints based on interesting points of the sector
        {
            foreach(var station in Stations)
            {
                Transform wp = GameObject.Instantiate(ObjectFactory.Instance.WaypointPrefab).transform;
                wp.position = station.transform.position + Vector3.up * 200;
                waypoints.Add(wp);
            }
            foreach (var gate in Jumpgates)
            {
                Transform wp = GameObject.Instantiate(ObjectFactory.Instance.WaypointPrefab).transform;
                wp.position = gate.transform.position - Vector3.up * 200;
                waypoints.Add(wp);
            }
            return waypoints.ToArray();
        }

        return waypoints.ToArray();
    }

    public static GameObject[] GetJumpgates()
    {
        // Why is this happening? TODO
        if (Jumpgates != null && Jumpgates.Count != 0) return Jumpgates.ToArray();
        Jumpgates = new List<GameObject>();
        Jumpgates.AddRange(GameObject.FindGameObjectsWithTag("Jumpgate"));

        return Jumpgates.ToArray();
    }

    public static List<GameObject> GetDockableObjects()
    {
        var dockables = new List<GameObject>();
        
        dockables.AddRange(Stations);
        // dockables.AddRange(Planets);
        dockables.AddRange(Jumpgates);
        
        return dockables;
    }
    public static List<GameObject> GetTradeableObjects()
    {
        var dockables = new List<GameObject>();
        
        dockables.AddRange(Stations);
        // dockables.AddRange(Planets);

        return dockables;
    }
}

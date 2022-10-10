using System.Collections.Generic;
using System.Linq;
using SpaceGraphicsToolkit;
using UnityEngine;
using Random = UnityEngine.Random;

public class GenerateRandomSector
{
    public static int SectorSize
    {
        get { return _sectorSize; }
    }
    private static int _sectorSize = 3000;

    private const float MINEABLE_FIELD_PROBABILITY = 0.5f;
    private const float FIELD_SPAWN_PROBABILITY = 0.5f;
    private const float NEBULA_SECTOR_PROBABILITY = 0.15f;
    private static int maxGrid;
    private const float obstacleCheckRadius = 250f;
    private const int maxSpawnAttempts = 10; 
    public static void GenerateSectorAtPosition(Vector2 position, Vector2 previousSector, int range = 2)
    {
        // just saving the gridsize so we can test against elsewhere
        maxGrid = range;

        Flare[] flares = SectorVisualData.Instance.flares;
        Material[] skybox = SectorVisualData.Instance.skybox;
        Color skyboxTint = new Color(Random.Range(0.25f, 0.75f), Random.Range(0.25f, 0.75f), Random.Range(0.25f, 0.75f), 1f);
        Faction[] factions = ObjectFactory.Instance.Factions;
        GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare = flares[Random.Range(0, flares.Length)];
        RenderSettings.skybox = skybox[Random.Range(0, skybox.Length)];
        if (RenderSettings.skybox.HasProperty("_Tint"))
            RenderSettings.skybox.SetColor("_Tint", skyboxTint);
        else if (RenderSettings.skybox.HasProperty("_SkyTint"))
            RenderSettings.skybox.SetColor("_SkyTint", skyboxTint);
        // This fixes the ambient light problem when dynamically changing skyboxes
        DynamicGI.UpdateEnvironment();

        _sectorSize = Random.Range(3, 10) * 1000;
        
        // Spawn jumpgates
        var jumpgates = GenerateJumpgates(position);
        if (jumpgates.Count is > 4 or < 1)
            Debug.LogError("Error in jumpgate creation! Not an appropriate count for this sector!");
        
        if (Random.value < NEBULA_SECTOR_PROBABILITY && position.x > 0 && position.y > 0)
        {
            var nebula = Object.Instantiate(ObjectFactory.Instance.NebulaPrefab).GetComponent<Nebula>();
            nebula.name = "Nebula";
            nebula.AmbientLight = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 0.35f);
            nebula.FogEnd = Random.Range(1.5f, 3f)*1000f;
            nebula.FogStart = 150;
            nebula.MaxViewDistance = nebula.FogEnd;
            // nebula.NebulaColor = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 0.35f); 
            // nebula.Clouds.PuffColor = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 0.35f); 
            // nebula.Particles.PuffColor = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 0.35f); 

            if(Random.value < 0.25)
            {
                nebula.Resource = "Helium";
                nebula.YieldPerSecond = Random.Range(1,3);
            }
            else if(Random.value < 0.25)
            {
                nebula.Resource = "Fuel";
                nebula.YieldPerSecond = Random.Range(1,3);
            }
            else
            {
                nebula.Resource = "";
            }
            nebula.CorrosionDamagePerSecond = Random.Range(0,5);
            nebula.IsSensorObscuring = Random.value < .7f;
        }

        // Spawn stations
        for(int i = 0; i < Random.Range(2,5); i++)
        {
            StationLoadout stationData = GetStationData(GetRandomFaction(factions));
            Station station = Object.Instantiate(ObjectFactory.Instance.GetStationByName(stationData.modelName), GetRandomPosition(), GetRandomRotation()).GetComponent<Station>();
            station.faction = GetRandomFaction(factions);
            station.id = "ST-" + RandomString(4);
            station.ownerID = "";
            station.loadout = stationData;
            station.gameObject.name = station.faction.name + " Station (" + station.id + ")";
            station.stationName = RandomNameGenerator.GetRandomStationName();
        }

        // Spawn asteroid fields
        for (int i = 0; i < Random.Range(1,3); i++)
        {
            if (Random.value > FIELD_SPAWN_PROBABILITY) 
                continue;

            var field = Object.Instantiate(ObjectFactory.Instance.AsteroidFieldPrefab, GetRandomPosition(), Quaternion.identity).GetComponent<AsteroidField>();
            field.name = "AsteroidField";
            field.ID = "AF-" + RandomString(4);
            field.range = Random.Range(800, 3500);
            field.velocity = Random.Range(0, 15);
            field.asteroidCount = Random.Range(2, 15) * 100;
            int rockSizeMin = Random.Range(2, 15);
            field.scaleRange = new Vector2(rockSizeMin, rockSizeMin+Random.Range(2, 15));

            // Material (determines the  and yield
            field.MineableResource = Random.value > 0.5f ? "Ore" : "Water";
            if(Random.value > MINEABLE_FIELD_PROBABILITY)
            {
                // Make a mine-able field
                float minYield = Random.Range(1, 5);
                field.YieldMinMax = new Vector2(minYield, minYield + Random.Range(4, 10));
            }
        }

        // Spawn planets
        if (Random.value < 0.65)
        {
            for (int i = 0; i < Random.Range(1, 2); i++)
            {
                var planet = Object
                    .Instantiate(
                        ObjectFactory.Instance.Planets[Random.Range(0, ObjectFactory.Instance.Planets.Length - 1)],
                        GetRandomPosition(true), GetRandomRotation()).GetComponent<Planet>();
                planet.faction = GetRandomFaction(factions);
                planet.id = "PL-" + RandomString(4);
                planet.loadout = GetRandomPlanetLoadout();
                    //PlanetLoadoutData.Instance.planets[Random.Range(0, PlanetLoadoutData.Instance.planets.Length - 1)];
                planet.planetName = RandomNameGenerator.GetRandomPlanetName();
                planet.gameObject.name = planet.faction.name + " Planet (" + planet.id + ")";
                if (planet.ringsphere != null)
                    planet.ringsphere.SetActive(planet.loadout.ring);
                if (planet.stormsphere != null)
                    planet.stormsphere.SetActive(planet.loadout.storm);
                if (planet.cloudSphere != null)
                    planet.cloudSphere.SetActive(planet.loadout.clouds);
                if (planet.stormsphere != null)
                    planet.stormsphere.SetActive(planet.loadout.atmosphere);
                // Should first check type, ie water planet
                planet.GetComponent<SgtPlanet>().WaterLevel = Random.value;
                planet.gameObject.transform.localScale *= planet.loadout.size + 1;
            }
        }

         // sector objects
         var obj = Object.Instantiate(ObjectFactory.Instance.GetSectorObjectByName("SectorShipLoader"), 
             GetRandomPosition(true), GetRandomRotation());

         if (position.x == 0 && position.y == 0)
         {
             Object.Instantiate(ObjectFactory.Instance.GetSectorObjectByName("PlanetaryExplosion"),
                 new Vector3(0,0,750), Quaternion.identity);
            // Debug.Log("Explosion added");
         }

         // Done
         Universe.AddSector(new Vector2(position.x, position.y), jumpgates);
    }

    #region MainMenuGenerator
    public static void GenerateMainMenuSectorAtPosition(Vector2 position, Vector2 previousSector)
    {
        Flare[] flares = SectorVisualData.Instance.flares;
        Material[] skybox = SectorVisualData.Instance.skybox;
        Color skyboxTint = new Color(Random.Range(0.25f, 0.75f), Random.Range(0.25f, 0.75f), Random.Range(0.25f, 0.75f), 1f);

        var val1 = flares[Random.Range(0, flares.Length)];
        GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare = val1;

        var val2 = skybox[Random.Range(0, skybox.Length)];
        RenderSettings.skybox = val2;
        if (RenderSettings.skybox.HasProperty("_Tint"))
            RenderSettings.skybox.SetColor("_Tint", skyboxTint);
        else if (RenderSettings.skybox.HasProperty("_SkyTint"))
            RenderSettings.skybox.SetColor("_SkyTint", skyboxTint);

        SectorStartScenario.Instance.flare = val1;
        SectorStartScenario.Instance.skybox = val2;
        SectorStartScenario.Instance.skyboxTint = skyboxTint;
        
        // This fixes the ambient light problem when dynamically changing skyboxes
        DynamicGI.UpdateEnvironment();

        _sectorSize = Random.Range(3, 10) * 1000;

        // Spawn asteroid fields
        // AsteroidField field;
        // for (int i = 0; i < 3; i++)
        // {
        //     if (Random.value > FIELD_SPAWN_PROBABILITY) 
        //         continue;
        //
        //     field = GameObject.Instantiate(ObjectFactory.Instance.AsteroidFieldPrefab, GetRandomPosition(), Quaternion.identity).GetComponent<AsteroidField>();
        //     field.ID = "f" + RandomString(4);
        //     field.range = Random.Range(800, 3500);
        //     field.velocity = Random.Range(0, 15);
        //     field.asteroidCount = Random.Range(2, 15) * 100;
        //     int rockSizeMin = Random.Range(2, 15);
        //     field.scaleRange = new Vector2(rockSizeMin, rockSizeMin+Random.Range(2, 15));
        //
        //     // Material (determines the  and yield
        //     field.MineableResource = Random.value > 0.5f ? "Ore" : "Water";
        //     if(Random.value > MINEABLE_FIELD_PROBABILITY)
        //     {
        //         // Make a mineable field
        //         float minYield = Random.Range(1, 5);
        //         field.YieldMinMax = new Vector2(minYield, minYield + Random.Range(4, 10));
        //     }
        // }
    }
    #endregion

    private static List<GameObject> GenerateJumpgates(Vector2 position)
    {
        List<GameObject> jumpgates = new List<GameObject>();
        GameObject gate = null;
        
        Vector2 adjacentSectorPosition = new Vector2(position.x, position.y + 1);
        // North sector
        gate = GetJumpgateToPosition(position, adjacentSectorPosition,0);
        if (gate != null) jumpgates.Add(gate);
        
        adjacentSectorPosition = new Vector2(position.x + 1, position.y);
        // East sector
        gate = GetJumpgateToPosition(position, adjacentSectorPosition,1);
        if (gate != null) jumpgates.Add(gate);
        
        adjacentSectorPosition = new Vector2(position.x, position.y - 1);
        // South sector
        gate = GetJumpgateToPosition(position, adjacentSectorPosition,2);
        if (gate != null) jumpgates.Add(gate);
        
        adjacentSectorPosition = new Vector2(position.x - 1, position.y);
        // West sector
        gate = GetJumpgateToPosition(position, adjacentSectorPosition,3);
        if (gate != null) jumpgates.Add(gate);
        
        return jumpgates;
    }

    #region Utils
    private static GameObject GetJumpgateToPosition(Vector2 jumpgateSector, Vector2 targetSector, int dir)
    {
        MazeGenerator maze = GameObject.Find("MazeGenerator").GetComponent<MazeGenerator>();

        // No gates outside our grid, no gates back to "lobby" level (-1,-1)
        if (targetSector.x < 0 || targetSector.x > maxGrid || targetSector.y < 0 || targetSector.y > maxGrid)
            return null;
        
        Vector3 pos = Vector3.zero;
        switch (dir)
        {
            case 0:
                pos = new Vector3(5000, 0, 0);
                break;
            case 1:
                pos = new Vector3(0, 0, 5000);
                break;
            case 2:
                pos = new Vector3(0, 0, -5000);
                break;
            case 3:
                pos = new Vector3(-5000, 0, 0);
                break;
        }

        Jumpgate jumpgate = GameObject.Instantiate(ObjectFactory.Instance.JumpGatePrefab, GetRandomPosition(), Quaternion.Euler(270, Random.Range(0, 360), 0)).GetComponent<Jumpgate>();
        jumpgate.NextSector = targetSector;
        jumpgate.id = "JG-" + RandomString(4);
        jumpgate.signA.text = "X " + jumpgate.NextSector.x + " Y " + jumpgate.NextSector.y;
        jumpgate.signB.text = jumpgate.signA.text;
        jumpgate.gameObject.name = "Jumpgate To (" + targetSector.x + ", " + targetSector.y + ")";
        jumpgate.transform.position = pos;
        
        string tempName = "x" + jumpgateSector.x + "y" + jumpgateSector.y;
            foreach(Transform child in maze.transform)
            {
                if (child.gameObject.name == tempName)
                {
                    //Debug.Log("Exit "+dir+" is "+child.GetChild(dir).gameObject.activeSelf);
                    if (!child.GetChild(dir).gameObject.activeSelf)
                        return jumpgate.gameObject;
                    else
                        return null;
                }
            }
            return jumpgate.gameObject;
    }

    public static string RandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[Random.Range(0, s.Length)]).ToArray());
    }

    private static Faction GetRandomFaction(Faction[] factions)
    {
        return factions[Random.Range(0, factions.Length - 1)];
    }

    public static Vector3 GetRandomPosition(bool isPlanet = false)
    {
        Vector3 pos = Vector3.zero;

        bool validPosition = false;
        int spawnAttempts = 0;
        while(!validPosition && spawnAttempts < maxSpawnAttempts)
        {
            pos = new Vector3(
                Random.Range(-3000, 3000),
                Random.Range(-750, 750), // Ensure sectors are more or less oriented on a 2D plane (XY)
                Random.Range(-3000, 3000)
            );
            if (isPlanet)
            {
                if (pos.x > 0)
                    pos.x += 5000;
                if (pos.z > 0)
                    pos.z += 5000;
                if (pos.x < 0)
                    pos.x -= 5000;
                if (pos.z < 0)
                    pos.z -= 5000;
            }

            // Collect all colliders within our Obstacle Check Radius
            Collider[] colliders = Physics.OverlapSphere(pos, obstacleCheckRadius);
            if (colliders.Length > 0)
                validPosition = false;
            else validPosition = true;
            spawnAttempts++;
        }
        return pos;
    }

    public static Color GetRandomColor()
    {
        return new Color(
            Random.value*0.5f + 0.5f,
            Random.value * 0.5f + 0.5f,
            Random.value * 0.5f + 0.5f
            );
    }

    private static StationLoadout GetStationData(Faction sectorFaction)
    {
        if(Random.value > 0.5)
        {
            return GetRandomStationLoadout();
        }

        int numberOfFactionStations;

        if (sectorFaction != null)
        {
            numberOfFactionStations = sectorFaction.stations.Length;
            if(numberOfFactionStations > 0)
                return sectorFaction.stations[Random.Range(0, numberOfFactionStations - 1)];
        }

        Faction randomFaction = ObjectFactory.Instance.Factions[Random.Range(0, ObjectFactory.Instance.Factions.Length - 1)];
        numberOfFactionStations = randomFaction.stations.Length;
        if(numberOfFactionStations > 0)
        {
            StationLoadout loadout = randomFaction.stations[Random.Range(0, numberOfFactionStations - 1)];
            return loadout;
        }

        return GetRandomStationLoadout();
    }

    private static StationLoadout GetRandomStationLoadout()
    {
        StationLoadout[] loadouts = Resources.LoadAll<StationLoadout>("Stations/");
        return loadouts[Random.Range(0, loadouts.Length)];
    }
    private static PlanetLoadout GetRandomPlanetLoadout()
    {
        PlanetLoadout[] loadouts = Resources.LoadAll<PlanetLoadout>("Planets/");
        return loadouts[Random.Range(0, loadouts.Length)];
    }
    private static Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
    }
    
    private Vector2 GetPointOnRing(int keplarRegion)
    {
        float innerRange = keplarRegion * 750 + 100;
        float outerRange = keplarRegion * 750 + 250;
        Vector2 v = Random.insideUnitCircle;
        return v.normalized * innerRange + v*(outerRange - innerRange);
    }
    #endregion Utils
}

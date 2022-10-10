using System;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SectorLoader : MonoBehaviour
{
    private static readonly int Tint = Shader.PropertyToID("_Tint");
    private static readonly int SkyTint = Shader.PropertyToID("_SkyTint");

    public static void LoadSectorData(string sectorName)
    {
        if (PlayArcadeIntegration.Instance.localLoads)
        {
            string path = Utils.SECTORS_FOLDER + sectorName;
            LoadSectorIntoScene(path);
        } 
        else
            PlayArcadeIntegration.Instance.LoadSectorIntoScene(sectorName);
    }

    public static void LoadSectorIntoScene(string filepath)
    {
        //print("LoadSectorIntoScene(): loading "+filepath+" sector data into scene");
        Flare[] flares = SectorVisualData.Instance.flares;
        Material[] skybox = SectorVisualData.Instance.skybox;
 
        if (!File.Exists(filepath))
        {
            Debug.LogError("LoadSectorIntoScene(): Tried to load sector but file " + filepath + " was not found!");
            TextFlash.ShowYellowText("Error in the Matrix! Stopgap measure tossing you back to start!");
            SceneManager.LoadScene("StartScenario");
            return;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filepath, FileMode.Open);

        SerializableSectorData data = formatter.Deserialize(stream) as SerializableSectorData;
        stream.Close();

        //SectorNavigation.SectorSize = data.Size;
        if (data != null)
        {
            GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare = flares[data.starIndex];
            RenderSettings.skybox = skybox[data.skyboxIndex];
            Color skyboxColor = new Color(data.skyboxTint.x, data.skyboxTint.y, data.skyboxTint.z);
            if (RenderSettings.skybox.HasProperty("_Tint"))
                RenderSettings.skybox.SetColor(Tint, skyboxColor);
            else if (RenderSettings.skybox.HasProperty("_SkyTint"))
                RenderSettings.skybox.SetColor(SkyTint, skyboxColor);
            // This fixes the ambient light problem when dynamically changing skyboxes
            DynamicGI.UpdateEnvironment();

            // Spawn nebula, if one exists
            if (data.nebula != null)
            {
                Nebula nebula = GameObject.Instantiate(ObjectFactory.Instance.NebulaPrefab).GetComponent<Nebula>();
                nebula.AmbientLight = data.nebula.ambientLight;
                nebula.FogEnd = data.nebula.fogEnd;
                nebula.FogStart = data.nebula.fogStart;
                nebula.MaxViewDistance = data.nebula.maxViewDistance;
                nebula.NebulaColor = data.nebula.nebulaColor;
                nebula.Clouds.PuffColor = data.nebula.nebulaCloudColor;
                nebula.Particles.PuffColor = data.nebula.nebulaParticleColor;

                nebula.YieldPerSecond = data.nebula.yieldPerSecond;
                nebula.CorrosionDamagePerSecond = data.nebula.corrosionDps;
                nebula.Resource = data.nebula.resource;
                nebula.IsSensorObscuring = data.nebula.isSensorObscuring;
            }

            GameObject spawnedObject;
            Vector3 spawnPosition;
            Quaternion spawnRotation;
            foreach (var stationData in data.stations)
            {
                // Get station loadout
                StationLoadout loadout = StationLoadout.GetLoadoutByName(stationData.loadoutName);

                // Spawn the station
                spawnPosition = stationData.position;
                spawnRotation = Quaternion.Euler(stationData.rotation);
                spawnedObject = GameObject.Instantiate(
                    ObjectFactory.Instance.GetStationByName(loadout.modelName),
                    spawnPosition, spawnRotation);

                // Fill data from loadout
                Station station = spawnedObject.GetComponent<Station>();
                station.loadout = loadout;
                station.id = stationData.ID;

                station.hasCargoDealer = loadout.hasCargoDealer;
                station.hasEquipmentDealer = loadout.hasEquipmentDealer;
                station.hasShipDealer = loadout.hasShipDealer;
                station.hasBarDealer = loadout.hasBarDealer;
                station.hasRepairDealer = loadout.hasRepairDealer;

                station.gameObject.name = station.faction.name + " Station (" + station.id + ")";
                station.stationName = RandomNameGenerator.GetRandomStationName();

                if (Application.isPlaying)
                    SectorNavigation.Stations.Add(spawnedObject);
            }

            foreach (var planetData in data.planets)
            {
                // Get planet loadout
                var loadout = PlanetLoadout.GetLoadoutByName(planetData.loadoutName);

                // Spawn the planet
                spawnPosition = planetData.position;
                spawnRotation = Quaternion.Euler(planetData.rotation);
                spawnedObject = GameObject.Instantiate(
                    ObjectFactory.Instance.GetPlanetByName("Planet" /*loadout.modelName*/),
                    spawnPosition, spawnRotation);

                // Fill data from loadout
                Planet planet = spawnedObject.GetComponent<Planet>();
                planet.loadout = loadout;
                planet.id = planetData.ID;
                planet.faction = loadout.faction;
                planet.planetName = RandomNameGenerator.GetRandomPlanetName();
                planet.gameObject.name = planet.faction.name + " Planet (" + planet.id + ")";
planet.ringsphere.SetActive(false);
planet.stormsphere.SetActive(false);
            #region PlanetGoods
            
            // Okay, our logic here should have been applied earlier, and tested
            if (planet.loadout.function == PlanetaryFunction.NaturalResources)
            {
                SectorNavigation.Instance.abundantGoods.Add("NaturalResources");
                SectorNavigation.Instance.abundantGoods.Add("Water");
                SectorNavigation.Instance.desiredGoods.Add("ConsumerGoods");
            }
            else if (planet.loadout.function == PlanetaryFunction.Agriculture)
            {
                SectorNavigation.Instance.abundantGoods.Add("Food");
                SectorNavigation.Instance.abundantGoods.Add("Water");
                SectorNavigation.Instance.desiredGoods.Add("Electronics");
            }
            else if (planet.loadout.function == PlanetaryFunction.Manufacturing)
            {
                if (planet.loadout.techLevel >= 2)
                {
                    SectorNavigation.Instance.abundantGoods.Add("ConsumerGoods");
                    SectorNavigation.Instance.desiredGoods.Add("Ore");
                    SectorNavigation.Instance.desiredGoods.Add("NaturalResources");
                }
                if (planet.loadout.techLevel >= 3)
                {
                    SectorNavigation.Instance.abundantGoods.Add("Electronics");
                    SectorNavigation.Instance.desiredGoods.Add("Ore");
                    SectorNavigation.Instance.desiredGoods.Add("Alloys");
                }
                if (planet.loadout.techLevel >= 4)
                {
                    SectorNavigation.Instance.desiredGoods.Add("Alloys");
                }
                if (planet.loadout.techLevel == 5)
                {
                    SectorNavigation.Instance.abundantGoods.Add("ComputerComponents");
                    SectorNavigation.Instance.desiredGoods.Add("Alloys");
                    SectorNavigation.Instance.desiredGoods.Add("Gold");
                }
            }
            else if (planet.loadout.function == PlanetaryFunction.Mining)
            {                
                SectorNavigation.Instance.abundantGoods.Add("Gold");
                if (planet.loadout.techLevel >= 1)
                    SectorNavigation.Instance.abundantGoods.Add("Ore");
                if (planet.loadout.techLevel >= 2)
                    SectorNavigation.Instance.abundantGoods.Add("Alloys");
                SectorNavigation.Instance.desiredGoods.Add("Food");
                SectorNavigation.Instance.desiredGoods.Add("Water");

            }
            #endregion

                if (Application.isPlaying)
                    SectorNavigation.Planets.Add(spawnedObject);
            }

            //print(data.Jumpgates.Count+" jumpgates added");
            foreach (var jumpgateData in data.jumpgates)
            {
                spawnPosition = jumpgateData.position;
                spawnRotation = Quaternion.Euler(jumpgateData.rotation);
                spawnedObject = GameObject.Instantiate(ObjectFactory.Instance.JumpGatePrefab,
                    spawnPosition, spawnRotation);

                spawnedObject.name = "Jumpgate To (" + jumpgateData.sector.x + ", " + jumpgateData.sector.y + ")";
                spawnedObject.GetComponent<Jumpgate>().NextSector = jumpgateData.sector;
                spawnedObject.GetComponent<Jumpgate>().id = jumpgateData.ID;
                if (Application.isPlaying)
                    SectorNavigation.Jumpgates.Add(spawnedObject);
            }

            if (data.jumpgates.Count > 4 || data.jumpgates.Count < 1)
                Debug.LogError("Error in jumpgate reading from sector file! Not an appropriate count for this sector!");

            foreach (var fieldData in data.fields)
            {
                spawnPosition = fieldData.position;
                spawnRotation = Quaternion.Euler(fieldData.rotation);
                spawnedObject = GameObject.Instantiate(ObjectFactory.Instance.AsteroidFieldPrefab,
                    spawnPosition, spawnRotation);

                AsteroidField asteroidField = spawnedObject.GetComponent<AsteroidField>();
                asteroidField.ID = fieldData.ID;
                asteroidField.range = fieldData.range;
                asteroidField.asteroidCount = fieldData.rockCount;
                asteroidField.scaleRange = fieldData.rockScaleMinMax;
                asteroidField.velocity = fieldData.velocity;
                asteroidField.angularVelocity = fieldData.angularVelocity;
                asteroidField.MineableResource = fieldData.resource;
                asteroidField.YieldMinMax = fieldData.yieldMinMax;
            }

            foreach (var sectorObjectData in data.sectorObjects)
            {
                //print("Should have loaded an sector object");
                SectorObjectLoadout loadout = SectorObjectLoadout.GetLoadoutByName(sectorObjectData.loadoutName);
                spawnPosition = sectorObjectData.position;
                spawnRotation = Quaternion.Euler(sectorObjectData.rotation);
                spawnedObject = GameObject.Instantiate(
                    ObjectFactory.Instance.GetSectorObjectByName(loadout.modelName),
                    spawnPosition, spawnRotation);

                // Fill data from loadout
                SectorObject obj = spawnedObject.GetComponent<SectorObject>();
                obj.loadout = loadout;
                obj.ID = sectorObjectData.ID;
                if (Application.isPlaying)
                    SectorNavigation.SectorObjects.Add(spawnedObject);
            }
        }

        if (!Application.isPlaying) return;
        // These must be called, as those functions rang long ago. We weren't ready until just now.
        SectorNavigation.Instance.Awake();
        ObjectUIMarkers.Instance.Start();
    }

}

using System;
using System.Collections.Generic;

[Serializable]
public class SerializableSectorData
{
    public int size;
    public int starIndex;
    public int skyboxIndex;
    public SerializableVector3 skyboxTint;
    public SerializableNebulaData nebula;
    public List<SerializableStationData> stations;
    public List<SerializablePlanetData> planets;
    public List<SerializableGateData> jumpgates;
    public List<SerializableFieldData> fields;
    public List<SerializableSectorObjectData> sectorObjects;
}

[Serializable]
public class SerializableStationData
{
    public SerializableVector3 position;
    public SerializableVector3 rotation;
    public string loadoutName;
    public string ID;
    public string stationName;
    public string ownerID;
    
    public static SerializableStationData FromStation(Station station)
    {
        SerializableStationData data = new SerializableStationData();

        data.position = station.transform.position;
        data.rotation = station.transform.rotation.eulerAngles;
        data.ID = station.id;
        data.stationName = station.stationName;
        data.ownerID = station.ownerID;
        data.loadoutName = station.loadout.name;

        return data;
    }
}
[Serializable]
public class SerializablePlanetData
{
    public SerializableVector3 position;
    public SerializableVector3 rotation;
    public string loadoutName;
    public string ID;
    public string planetName;
    public string ownerID;

    public static SerializablePlanetData FromPlanet(Planet planet)
    {
        SerializablePlanetData data = new SerializablePlanetData();

        data.position = planet.transform.position;
        data.rotation = planet.transform.rotation.eulerAngles;
        data.ID = planet.id;
        data.planetName = planet.planetName;
        data.ownerID = planet.ownerID;
        data.loadoutName = planet.loadout.name;

        return data;
    }
}
[Serializable]
public class SerializableGateData
{
    public SerializableVector3 position;
    public SerializableVector3 rotation;
    public SerializableVector2 sector;
    public string ID;

    public static SerializableGateData FromGate(Jumpgate gate)
    {
        SerializableGateData data = new SerializableGateData();

        data.position = gate.transform.position;
        data.rotation = gate.transform.rotation.eulerAngles;
        data.ID = gate.id;
        data.sector = gate.NextSector;

        return data;
    }
}

[Serializable]
public class SerializableSectorObjectData
{
    public SerializableVector3 position;
    public SerializableVector3 rotation;
    public string sectorName;
    public string ID;
    public string loadoutName;
    public string ownerID;

    public static SerializableSectorObjectData FromSectorObject(SectorObject obj)
    {
        SerializableSectorObjectData data = new SerializableSectorObjectData();

        data.position = obj.transform.position;
        data.rotation = obj.transform.rotation.eulerAngles;
        data.ID = obj.ID;
        data.sectorName = obj.sectorName;
        data.loadoutName = obj.loadout.name;
        data.ownerID = obj.ownerID;

        return data;
    }
}

[Serializable]
public class SerializableFieldData
{
    public SerializableVector3 position;
    public SerializableVector3 rotation;
    public string ID;
    public int rockCount;
    public float range;
    public SerializableVector2 rockScaleMinMax;
    public float velocity;
    public float angularVelocity;
    public string resource;
    public SerializableVector2 yieldMinMax;

    public static SerializableFieldData FromField(AsteroidField field)
    {
        SerializableFieldData data = new SerializableFieldData();

        data.position = field.transform.position;
        data.rotation = field.transform.rotation.eulerAngles;
        data.ID = field.ID;
        data.range = field.range;
        data.rockCount = field.asteroidCount;
        data.rockScaleMinMax = field.scaleRange;
        data.velocity = field.velocity;
        data.angularVelocity = field.angularVelocity;
        data.resource = field.MineableResource;
        data.yieldMinMax = field.YieldMinMax;

        return data;
    }
}

[Serializable]
public class SerializableNebulaData
{
    public SerializableColor ambientLight;
    public float fogStart;
    public float fogEnd;
    public float maxViewDistance;
    public SerializableColor nebulaColor;
    public SerializableColor nebulaCloudColor;
    public SerializableColor nebulaParticleColor;

    public float corrosionDps;
    public bool isSensorObscuring;
    public string resource;
    public int yieldPerSecond;

    public static SerializableNebulaData FromNebula(Nebula nebula)
    {
        SerializableNebulaData data = new SerializableNebulaData();

        data.ambientLight = nebula.AmbientLight;
        data.fogStart = nebula.FogStart;
        data.fogEnd = nebula.FogEnd;
        data.maxViewDistance = nebula.MaxViewDistance;
        data.nebulaColor = nebula.NebulaColor;
        data.nebulaCloudColor = nebula.Clouds.PuffColor;
        data.nebulaParticleColor = nebula.Particles.PuffColor;

        data.corrosionDps = nebula.CorrosionDamagePerSecond;
        data.isSensorObscuring = nebula.IsSensorObscuring;
        data.resource = nebula.Resource;
        data.yieldPerSecond = nebula.YieldPerSecond;

        return data;
    }
}
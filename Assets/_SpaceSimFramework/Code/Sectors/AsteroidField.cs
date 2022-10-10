using UnityEngine;

public enum RandomSpawnerShape
{
    Box,
    Sphere,
    Belt
}

// Used mostly for testing to provide stuff to fly around and into.
public class AsteroidField : MonoBehaviour
{   
    // Unique in-game object ID 
    [Tooltip("DO NOT touch this if you don't have to")]
    [Header("Sector object ID")]
    public string ID;

    [Header("General settings:")]
    [Tooltip("XZ of the sun")]
    public Vector2 systemCenter;

    [Tooltip("Prefab to spawn.")]
    public Transform[] prefabs;
    public Material OreMaterial; 
    public Material IceMaterial;
    public Material MineralMaterial;

    [Tooltip("Shape to spawn the prefabs in.")]
    public RandomSpawnerShape spawnShape = RandomSpawnerShape.Sphere;

    [Tooltip("Multiplier for the spawn shape in each axis.")]
    public Vector3 shapeModifiers = Vector3.one;

    [Tooltip("How many prefab to spawn.")]
    public int asteroidCount = 50;
    
    [Range(0, 5)]
    public int keplarRegion = 0;
    [Tooltip("Distance from the center of the gameobject that prefabs will spawn")]
    public float range = 1000.0f;
    [Tooltip("Belt shape will use these. But still set the single range...")]
    public float innerRange;
    public float outerRange;
    public float offsetRange;
    
    [Tooltip("Should prefab have a random rotation applied to it.")]
    public bool randomRotation = true;

    [Tooltip("Random min/max scale to apply.")]
    public Vector2 scaleRange = new Vector2(1.0f, 3.0f);

    [Header("Rigidbody settings:")]

    [Tooltip("Apply a velocity from 0 to this value in a random direction.")]
    public float velocity = 0.0f;

    [Tooltip("Apply an angular velocity (deg/s) from 0 to this value in a random direction.")]
    public float angularVelocity = 0.0f;

    [Tooltip("If true, raise the mass of the object based on its scale.")]
    public bool scaleMass = true;

    [Header("Mining properties")]

    [Tooltip("If null field, it's is not mineable, otherwise enter commodity name")]
    public string MineableResource = "";

    [Tooltip("Minimum and maximum yield (if mineable) in cargo units. Must be integer.")]
    public Vector2 YieldMinMax;

    // Use this for initialization
    void Start()
    {
        if (prefabs.Length > 0)
        {
            for (int i = 0; i < asteroidCount; i++)
                CreateAsteroid();
        }
    }

    private void CreateAsteroid()
    {
        Vector3 spawnPos = Vector3.zero;
         
        // Create random position based on specified shape and range.
        if (spawnShape == RandomSpawnerShape.Box)
        {
            spawnPos.x = Random.Range(-range, range) * shapeModifiers.x;
            spawnPos.y = Random.Range(-range, range) * shapeModifiers.y;
            spawnPos.z = Random.Range(-range, range) * shapeModifiers.z;
        }
        else if (spawnShape == RandomSpawnerShape.Sphere)
        {
            spawnPos = Random.insideUnitSphere * range;
            spawnPos.x *= shapeModifiers.x;
            spawnPos.y *= shapeModifiers.y;
            spawnPos.z *= shapeModifiers.z;
        }
        else if (spawnShape == RandomSpawnerShape.Belt)
        {
            Vector2 point = GetPointOnRing();
            float y = Random.Range(-offsetRange, offsetRange);
            spawnPos = new Vector3(point.x, y, point.y); 
        }

        // Offset position to match position of the parent gameobject.
        spawnPos += transform.position;

        // Apply a random rotation if necessary.
        Quaternion spawnRot = (randomRotation) ? Random.rotation : Quaternion.identity;

        // Create the object and set the parent to this gameobject for scene organization.
        var t = Instantiate(prefabs[Random.Range(0, prefabs.Length-1)], spawnPos, spawnRot) as Transform;
        t.SetParent(transform);

        // Asteroid properties
        Asteroid asteroid = t.GetComponentInChildren<Asteroid>();
        asteroid.Yield = Random.Range((int)YieldMinMax.x, (int)YieldMinMax.y);
        asteroid.Resource = MineableResource;
        if(MineableResource != "")
        {
            asteroid.ApplyMaterial(MineableResource == "Ore" ? OreMaterial : IceMaterial);
        }

        // Apply scaling.
        float scale = Random.Range(scaleRange.x, scaleRange.y);
        t.localScale = Vector3.one * scale;

        // Apply rigidbody values.
        Rigidbody r = t.GetComponent<Rigidbody>();
        if (r)
        {
            if (scaleMass)
                r.mass *= scale * scale * scale;

            r.AddRelativeForce(Random.insideUnitSphere * velocity, ForceMode.VelocityChange);
            r.AddRelativeTorque(Random.insideUnitSphere * angularVelocity * Mathf.Deg2Rad, ForceMode.VelocityChange);
        }
    }

    public void CreateNewAsteroid()
    {
        CreateAsteroid();
    }

    private Vector2 GetPointOnRing()
    {
        var v = Random.insideUnitCircle;
        return v.normalized * innerRange + v*(outerRange - innerRange);
    }
    
    public void GenerateAsteroidField()
    {
        ID = "ST-" + GenerateRandomSector.RandomString(4);
        if (keplarRegion == 0)
            return;
        Vector2 point = GetPointOnRing();
        float y = Random.Range(-250, 250);
        this.transform.position = new Vector3(point.x, y, point.y); 
    }
    public void ClearAsteroidField() {}
}

using UnityEngine;

/// <summary>
/// Contains specifications of a certain ship model
/// </summary>
[CreateAssetMenu(menuName = "ModelInfo")]
public class ModelInfo : ScriptableObject
{

    [Tooltip("Ship model name")]
    public string modelName;
    [Tooltip("Chance of spawning. 0 = common, 1 = rare")]
    [Range(0,1)]
    public float rarity = 1;
    [Tooltip("Does this ship use external docking or docking bays?")]
    public bool ExternalDocking;
    public enum mooringType { OrbitalMooringOnly = 1, DockOrLand = 2 }
    public mooringType mooring;
    [Tooltip("Distance at which the camera follows the ship (for different ship sizes)")]
    public float CameraOffset;
    [Tooltip("Ship class determines maximum level of equipment mountable to it")]
    public int Class;
    [Tooltip("Ship cost in credits")]
    public int Cost;
    [Tooltip("Armor (hull integrity) value")]
    public int MaxArmor;
    [Tooltip("Total generator power output")]
    public int GeneratorPower;
    [Tooltip("Generator power regen rate per second")]
    public float GeneratorRegen;
    [Tooltip("Cargo size in container units")]
    public int CargoSize;
    [Tooltip("X: Linear thrust\nY: Vertical thrust\nZ: Longitudinal Thrust")]
    public Vector3 LinearForce;
    [Tooltip("X: Angular thrust:\nX: Pitch\nY: Yaw\nZ: Roll")]
    public Vector3 AngularForce;
    [Tooltip("Number of equipment items this model can equip")]
    public int EquipmentSlots;
    public int ScannerRange;
    public int totalMass;
    public int maxGunMounts = 4;
    public int maxTurretMounts = 4;
}

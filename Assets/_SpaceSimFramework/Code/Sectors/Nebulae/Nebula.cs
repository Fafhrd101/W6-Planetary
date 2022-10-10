using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class Nebula : MonoBehaviour
{
    // Unique in-game object ID 
    [HideInInspector]
    public string ID;
    [Space(20)]
    public NebulaPuffs Clouds;
    public NebulaPuffs Particles;
    public Color AmbientLight = Color.white;
    public Color NebulaColor = Color.white;
    public float MaxViewDistance = 1000f;
    public float FogStart = 0f;
    public float FogEnd = 1000f;

    [Header("Gameplay")]
    [Tooltip("If true, sensors will be obscured while inside nebula")]
    public bool IsSensorObscuring = false;
    [Tooltip("Corrosive nebula deals a certain damage per minute to ships within")]
    public float CorrosionDamagePerSecond = 0;
    [Tooltip("Resource mineable in this nebula, null if none")]
    public string Resource;
    [Tooltip("Max yield provided by mining this nebula")]
    public int YieldPerSecond;
    private float _currentYieldCount = 0;
    private float _currentYieldTotal = 0;
    
    private Light[] _sunLights;
    private Color _sunColor = Color.white;

    public static Nebula Instance
    {
        get { return _instance; }
    }
    private static Nebula _instance = null;

    private void Awake()
    {
        if (_instance != null)
        {            
            //Destroy(_instance);
            Debug.LogError("Two nebulae instances found in scene, removing one.");
        }
        _instance = this;
    }

    private void Start()
    {
        GameObject[] obj = GameObject.FindGameObjectsWithTag("Sun");
        if (obj != null)
        {
            _sunLights = new Light[obj.Length];

            for (int i = 0; i < obj.Length; i++)
                _sunLights[i] = obj[i].GetComponent<Light>();

            if (_sunLights.Length > 0)
                _sunColor = _sunLights[0].color;
        }

        ApplyNebulaVisualEffects();
        ApplyNebulaGameEffects();
    }

    private void Update()
    {
        if (CorrosionDamagePerSecond > 0)
        {
            WarningUI.Instance.corrosionDps.GetComponent<TMP_Text>().text =
                CorrosionDamagePerSecond.ToString(CultureInfo.CurrentCulture);
            Ship.PlayerShip.armor -= CorrosionDamagePerSecond * Time.deltaTime;
        }
        if (YieldPerSecond > 0)
        {
            ControlStatusUI.Instance.ramScoopYield.GetComponent<TMP_Text>().text = _currentYieldTotal.ToString(CultureInfo.CurrentCulture);
            ControlStatusUI.Instance.miningType.GetComponent<TMP_Text>().text = Resource;
            if (Ship.PlayerShip.GetComponent<ShipCargo>().cargoOccupied < 
                Ship.PlayerShip.GetComponent<ShipCargo>().cargoSize)
            {
                _currentYieldCount += YieldPerSecond * Time.deltaTime;
                if (_currentYieldCount > 1f)
                {
                    Ship.PlayerShip.GetComponent<ShipCargo>().AddWare(HoldItem.CargoType.Ware, Resource, 1);
                    _currentYieldCount -= 1f;
                    _currentYieldTotal += 1;
                }
            }
        }
    }

    private void ApplyNebulaGameEffects()
    {
        if (IsSensorObscuring)
        {
            foreach(GameObject ship in SectorNavigation.Ships)
            {
                var shipComponent = ship.GetComponent<Ship>();
                shipComponent.shipModelInfo.ScannerRange = (int)(shipComponent.shipModelInfo.ScannerRange*0.2f);
            }

            WarningUI.Instance.sensorsObscured = true;
        }

        if (CorrosionDamagePerSecond > 0)
        {
            WarningUI.Instance.corrosionOn = true;
            WarningUI.Instance.blinkSpeed = CorrosionDamagePerSecond;
        }
        if (YieldPerSecond > 0)
        {
            ControlStatusUI.Instance.ramScoopActive = true;
        }
    }

    private void ApplyNebulaVisualEffects()
    {
        //RenderSettings.ambientLight = AmbientLight;

        // Camera.main.farClipPlane = MaxViewDistance;
        // Camera.main.backgroundColor = NebulaColor;
        // Camera.main.clearFlags = CameraClearFlags.Color;

        // RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        // RenderSettings.ambientSkyColor = AmbientLight;

        // RenderSettings.fog = true;
        // RenderSettings.fogMode = FogMode.Linear;
        // RenderSettings.fogColor = NebulaColor;
        // RenderSettings.fogStartDistance = FogStart;
        // RenderSettings.fogEndDistance = FogEnd;

        // Fade the light because the nebula is blocking most of it.
        foreach (Light sun in _sunLights)
        {
            sun.color = Color.Lerp(_sunColor, NebulaColor, 0.5f);
            sun.shadowStrength = 0.2f;
        }
    }
}
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasViewController : Singleton<CanvasViewController>
{
    private static Vector3 TACTICAL_CAMERA_ANGLE = new Vector3(45, 135, 0);

    public GameObject Hud;
    public GameObject gameOverSummaryCanvas;
    public RectTransform FlightCanvas, TacticalCanvas;
    public GameObject TargetMarkers;
    public float AnimationTime = 1f;
    public AnimationCurve TacticalAnimationCurve;
    public AnimationCurve CameraHeightCurve;

    public static bool IsMapActive = false;
    public bool hudActive = true;
    private bool _universeMapActive = false;
    private CameraController _thirdPersonCamera;
    private MapCameraController _mapCamera;

    // If the map is open for selection mode, it only expects a single 
    // object/position to be selected.
    [HideInInspector]
    public bool IsMapOpenForSelection = false;
    // The item position that was selected on the map, if open for selection
    private Transform mapSelectedItem;
    private Vector3 mapSelectedPos;

    private void Start()
    {
        _thirdPersonCamera = Camera.main.GetComponent<CameraController>();
        _mapCamera = Camera.main.GetComponent<MapCameraController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)/* && PlayArcadeIntegration.Instance.devBuild*/)
        {
            ToggleHud();
        }
        else if (Input.GetKeyDown(KeyCode.M) && PlayArcadeIntegration.Instance.devBuild)
        {
            ToggleMap();
        }
        // No map in lobby level
        else if (Input.GetKeyDown(KeyCode.U))
        {
            if(SceneManager.GetActiveScene().name != "StartScenario"){
                CanvasController.Instance.CloseAllMenus();
                if (!_universeMapActive)
                {
                    CanvasController.Instance.CloseAllMenus();
                    CanvasController.Instance.OpenMenu(UIElements.Instance.UniverseMap);
                    _universeMapActive = true;
                }
                else
                {
                    CanvasController.Instance.CloseAllMenus();
                    _universeMapActive = false;
                }
                Ship.IsShipInputDisabled = _universeMapActive;
                Cursor.visible = _universeMapActive;
                Ship.PlayerShip.UsingMouseInput = !_universeMapActive;
            }
            else
            {
                TextFlash.ShowYellowText("Universe mapping not available in the lobby level.");
                return;
            }
        }
    }

    public void SetHUDActive(bool active)
    {
        Hud.SetActive(active);
    }

    public void ToggleHud()
    {
        Hud.SetActive(hudActive);
        hudActive = !hudActive;
    }
    /// <summary>
    /// If the map is open for selection, the player must select an item on the map to give a ship an order. The map
    /// must close once an object/position/ship is selected.
    /// </summary>
    /// <param name="isOpenForSelection"></param>
    public void ToggleMap()
    {
        IsMapActive = !IsMapActive;
        CanvasController.Instance.CloseAllMenus();
        ObjectUIMarkers.Instance.OnViewModeChanged(IsMapActive);

        if (IsMapActive)
        {
            Cursor.visible = true;
            StartCoroutine(AnimateCameraToMap());
        }
        else
        {
            StartCoroutine(AnimateCameraToShip());
        }
    }

    private IEnumerator ChangeCameraHeight(float speed)
    {
        float t = 0;
        Vector3 startPosition = Camera.main.transform.position;
        Vector3 endPosition = Camera.main.transform.position + Vector3.up * speed * 100f;

        while (t < AnimationTime)
        {
            t += Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(startPosition, endPosition, CameraHeightCurve.Evaluate(t / AnimationTime));
            Camera.main.transform.rotation = Quaternion.Euler(TACTICAL_CAMERA_ANGLE);
            yield return null;
        }
    }

    private IEnumerator AnimateCameraToShip()
    {
        OnCameraAnimationStart();

        float t = 0;
        Vector3 startPosition = Camera.main.transform.position;
        Quaternion startRotation = Camera.main.transform.rotation;

        while (t < AnimationTime)
        {
            t += Time.deltaTime;
            Camera.main.transform.position = Vector3.Lerp(startPosition, _thirdPersonCamera.GetTargetCameraPosition(), TacticalAnimationCurve.Evaluate(t / AnimationTime));
            Camera.main.transform.rotation = Quaternion.Lerp(startRotation, Ship.PlayerShip.transform.rotation, TacticalAnimationCurve.Evaluate(t / AnimationTime));
            yield return null;
        }

        // Activate UI elements
        TargetMarkers.SetActive(true);
        FlightCanvas.gameObject.SetActive(true);
        SetHUDActive(true);

        // Enable direct ship control
        Ship.IsShipInputDisabled = false;
        // Swap camera scripts
        _thirdPersonCamera.enabled = !IsMapActive;
        _mapCamera.enabled = IsMapActive;
        Camera.main.fieldOfView = 60f;
        //TextFlash.ShowYellowText("Press M to show sector map");
    }

    private IEnumerator AnimateCameraToMap()
    {
        OnCameraAnimationStart();
        Camera cam = Camera.main;
        Transform trans = cam.transform;
        
        float t = 0;
        trans.position = Ship.PlayerShip.transform.position;
        Vector3 startPosition = trans.position;
        Quaternion startRotation = trans.rotation;
        Vector3 endPosition = trans.position + _mapCamera.tacticalCameraOffset;
        Quaternion endRotation = Quaternion.Euler(TACTICAL_CAMERA_ANGLE);

        while (t < AnimationTime)
        {
            t += Time.deltaTime;
            cam.transform.position = Vector3.Lerp(startPosition, endPosition, TacticalAnimationCurve.Evaluate(t / AnimationTime));
            cam.transform.rotation = Quaternion.Lerp(startRotation, endRotation, TacticalAnimationCurve.Evaluate(t / AnimationTime));
            yield return null;
        }

        cam.transform.rotation = endRotation;

        // Activate UI elements
        TargetMarkers.SetActive(true);
        TacticalCanvas.gameObject.SetActive(true);

        // Disable direct ship control
                    Ship.IsShipInputDisabled = true;
        // Swap camera scripts
        _thirdPersonCamera.enabled = !IsMapActive;
        _mapCamera.enabled = IsMapActive;
        //Camera.main.fieldOfView = 100f;
        //TextFlash.ShowYellowText("Press M to close sector map");
    }

    private void OnCameraAnimationStart()
    {
        TargetMarkers.SetActive(false);

        // Deactivate active UI elements
        FlightCanvas.gameObject.SetActive(false);
        SetHUDActive(false);
        TacticalCanvas.gameObject.SetActive(false);

        // Terminate direct ship control
                    Ship.IsShipInputDisabled = true;
    }

    #region map selection for order issuing
    /// <summary>
    /// Invoked by ShipAI when an order is given requiring a 
    /// selection of a map object.
    /// </summary>
    /// <returns></returns>
    public Transform GetMapSelectedObject()
    {
        if (mapSelectedItem != null)
        {
            IsMapOpenForSelection = false;
            return mapSelectedItem;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Invoked by ShipAI when an order is given requiring a 
    /// selection of a map location.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetMapSelectedPosition()
    {
        if (mapSelectedPos != Vector3.zero)
        {
            Vector3 copy = mapSelectedPos;
            mapSelectedPos = Vector3.zero;
            IsMapOpenForSelection = false;
            return copy;
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Invoked by the map when a user has confirmed a target
    /// </summary>
    /// <param name="position"></param>
    public void SetMapSelectedObject(Transform position)
    {
        mapSelectedItem = position;
        ToggleMap();
    }

    /// <summary>
    /// Invoked by the map when a user has confirmed a position
    /// </summary>
    /// <param name="position"></param>
    public void SetMapSelectedPosition(Vector3 position)
    {
        mapSelectedPos = position;
        ToggleMap();
    }
    #endregion map selection for order issuing
}

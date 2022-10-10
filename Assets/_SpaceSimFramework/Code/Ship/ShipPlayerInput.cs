using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using SpaceSimFramework.Code.UI.HUD;
using UnityEngine.UIElements;

/// <summary>
/// Class specifically to deal with input.
/// </summary>
public class ShipPlayerInput : MonoBehaviour
{
    [Tooltip("When using Keyboard/Joystick input, should roll be added to horizontal stick movement. This is a common trick in traditional space sims to help ships roll into turns and gives a more plane-like feeling of flight.")]
    public bool addRoll = true;
    [Tooltip("When true, the mouse and mousewheel are used for ship input and A/D can be used for strafing like in many arcade space sims.\n\nOtherwise, WASD/Arrows/Joystick + R/T are used for flying, representing a more traditional style space sim.")]
    public bool useMouseInput = false;

    [Range(-1, 1)]
    public float pitch;
    [Range(-1, 1)]
    public float yaw;
    [Range(-1, 1)]
    public float roll;
    [Range(-1, 1)]
    public float strafe;
    [Range(-0.1f, 3)]
    public float throttle;
    
    // How quickly the throttle reacts to input.
    public float THROTTLE_SPEED = 0.5f;

    // Keep a reference to the ship this is attached to just in case.
    [HideInInspector]
    public Ship ship;
    private CameraController _cam;
    private ShipEquipment _shipEquipment;
    private Light[] _engineTorches;
    private TrailRenderer[] _engineTrails;

    private void Awake()
    {
        ship = GetComponent<Ship>();
        if (Camera.main is not null) _cam = Camera.main.GetComponent<CameraController>();
        _engineTorches = GetComponentsInChildren<Light>();
        _engineTrails = GetComponentsInChildren<TrailRenderer>();
        _shipEquipment = GetComponent<ShipEquipment>();
    }

    private void Update()
    {
        if (!ship.isPlayerControlled)
            return;

        if (Ship.IsShipInputDisabled)
            return;

        yaw = Input.GetAxis("Yaw");
        pitch = Input.GetAxis("Pitch");
        //strafe = Input.GetAxis("Strafe");
        roll = Input.GetAxis("Roll");
        if (useMouseInput)
        {
            SetStickCommandsUsingMouse();
            UpdateMouseWheelThrottle();
        }
        UpdateKeyboardThrottle(KeyCode.W, KeyCode.S);

        if (ship.InSupercruise)
            _shipEquipment.SupercruiseDrain();
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            var closestEnemies = SectorNavigation.GetClosestVisibleTarget(transform, ship.shipModelInfo.ScannerRange);
            closestEnemies = closestEnemies.OrderBy(
                x => Vector2.Distance(this.transform.position,x.transform.position)
            ).ToList();
            InputHandler.Instance.SelectedObject = closestEnemies.Count > 0 ? closestEnemies[0] : null;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ComputerViewController.Instance.OnComputersClicked();
            //  This will show the AI/AutoPilot commands
            //     if (CanvasController.Instance.GetNumberOfOpenMenus() <= 0)
            //     {
            //         GameObject menu = CanvasController.Instance.OpenMenu(UIElements.Instance.SimpleCommandMenu);
            //         SimpleCommandMenu scm = menu.GetComponent<SimpleCommandMenu>();
            //         scm.PopulateMenu();
            //     }
        }
        
    }

    public void ToggleSupercruise(bool forcedOn = false)
    {
        ship.Physics.IsEngineOn = true;
        if (forcedOn)
            ship.inSupercruise = true;
        else
            ship.InSupercruise = !ship.InSupercruise;

        if (ship.InSupercruise)
        {
            StartCoroutine(EngageSupercruise());
        }
        else if (!ship.InSupercruise)
        {
            throttle = 1f;
        }
    }

    private void ManualSupercruise()
    {
        StartCoroutine(EngageSupercruise());
    }
    
    public void ToggleEngines()
    {
        for (int i = 0; i < _engineTorches.Length; i++)
        {
            if (ship.Physics.IsEngineOn)
            {
                _engineTorches[i].intensity = 1.0f;
                _engineTrails[i].gameObject.SetActive(true);
            }
            else
            {
                _engineTorches[i].intensity = 0f;
                _engineTrails[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Accelerate ship to 3x max throttle
    /// </summary>
    /// <returns></returns>
    private IEnumerator EngageSupercruise()
    {
        while(throttle < 3.0f && ship.inSupercruise)
        {
            if (!ship.InSupercruise)
                yield break;

            throttle = Mathf.MoveTowards(throttle, 3.0f, Time.deltaTime * THROTTLE_SPEED);

            yield return null;
        }
        yield return null;
    }


    /// <summary>
    /// Freelancer style mouse controls. This uses the mouse to simulate a virtual joystick.
    /// When the mouse is in the center of the screen, this is the same as a centered stick.
    /// </summary>
    private void SetStickCommandsUsingMouse()
    {
        Vector3 mousePos = Input.mousePosition;

        // Figure out most position relative to center of screen.
        // (0, 0) is center, (-1, -1) is bottom left, (1, 1) is top right.      
        pitch = (mousePos.y - (Screen.height * 0.5f)) / (Screen.height * 0.5f);
        yaw = (mousePos.x - (Screen.width * 0.5f)) / (Screen.width * 0.5f);

        // Make sure the values don't exceed limits.
        pitch = -Mathf.Clamp(pitch, -1.0f, 1.0f);
        yaw = Mathf.Clamp(yaw, -1.0f, 1.0f);
    }

    /// <summary>
    /// Uses R and F to raise and lower the throttle.
    /// </summary>
    private void UpdateKeyboardThrottle(KeyCode increaseKey, KeyCode decreaseKey)
    {
        ship.InSupercruise = throttle > 1;
        if(ship.InSupercruise && Input.GetKey(decreaseKey))
        {
            throttle = 1.0f;
            ship.InSupercruise = false;
            return;
        }
        
        float target = throttle;

        if (Input.GetKey(increaseKey))
        {
            target = Input.GetKey(KeyCode.LeftShift) ? 3.0f : 1.0f;
        }
        else if (Input.GetKey(decreaseKey))
        {
            target = -0.5f;
        }
        else
        {
            // bleed it off
            if (throttle > 0)
                target = throttle - 0.005f;
            else
                target = throttle + 0.005f;
            target = Mathf.Min(target, 0f);
        }
        
        if (ship.isSpeedLimited)
            target = Mathf.Max(target, 0.05f);

        throttle = Mathf.MoveTowards(throttle, target, Time.deltaTime * THROTTLE_SPEED);
    }

    /// <summary>
    /// Uses the mouse wheel to control the throttle.
    /// </summary>
    private void UpdateMouseWheelThrottle()
    {
        // no longer does anything
        //throttle = Mathf.Clamp(throttle, -0.6f, 3.0f);
    }
}
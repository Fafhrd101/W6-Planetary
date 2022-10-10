using SpaceSimFramework.Code.UI.HUD;
using UnityEngine;

/// <summary>
/// Applies linear and angular forces to a ship.
/// This is based on the ship physics from https://github.com/brihernandez/UnityCommon/blob/master/Assets/ShipPhysics/ShipPhysics.cs
/// </summary>
public class ShipPhysics : MonoBehaviour
{
    [Tooltip("X: Lateral thrust\nY: Vertical thrust\nZ: Longitudinal Thrust")]
    //[HideInInspector]
    public Vector3 linearForce = new Vector3(100.0f, 100.0f, 100.0f);
    [Tooltip("X: Pitch\nY: Yaw\nZ: Roll")]
    //[HideInInspector]
    public Vector3 angularForce = new Vector3(100.0f, 100.0f, 100.0f);
    [Tooltip("Multiplier for all forces. Can be used to keep force numbers smaller and more readable.")]
    public float forceMultiplier = 100.0f;
    public Rigidbody Rigidbody { get; private set; }
    private Vector3 _appliedLinearForce = Vector3.zero;
    private Vector3 _appliedAngularForce = Vector3.zero;
    private Vector3 _maxAngularForce;
    // Engine kill controls
    private float _rBodyDrag;
    public bool IsEngineOn { get; set; } = true;
    // Keep a reference to the ship this is attached to just in case.
    private Ship _ship;

    // Use this for initialization
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        if (Rigidbody == null)
        {
            Debug.LogWarning(name + ": ShipPhysics has no rigidbody.");
        }

        _ship = GetComponent<Ship>();
        linearForce = _ship.shipModelInfo.LinearForce;
        angularForce = _ship.shipModelInfo.AngularForce;
    }

    private void Start()
    {
        _rBodyDrag = Rigidbody.drag;
        _maxAngularForce = angularForce * forceMultiplier;
    }

    public void FixedUpdate()
    {
        if (Rigidbody != null)
        {
            if(IsEngineOn)
                Rigidbody.AddRelativeForce(_appliedLinearForce, ForceMode.Force);

            Rigidbody.AddRelativeTorque(
                ClampVector3(_appliedAngularForce, -1 * _maxAngularForce, _maxAngularForce),
                ForceMode.Force);
        }
    }

    private void Update()
    {
        Vector3 linearInput;

        if (_ship.isPlayerControlled)
        {
            linearInput = new Vector3(_ship.PlayerInput.strafe, 0, _ship.PlayerInput.throttle);
            var angularInput = new Vector3(_ship.PlayerInput.pitch, _ship.PlayerInput.yaw, _ship.PlayerInput.roll);
            SetPhysicsInput(linearInput, angularInput);
        }
        else
        {
            linearInput = new Vector3(0, 0, _ship.AIInput.throttle);
            _appliedLinearForce = MultiplyByComponent(linearInput, linearForce) * forceMultiplier;
            _appliedAngularForce = _ship.AIInput.angularTorque;
            _appliedAngularForce.z = 0;
        }
    }

    /// <summary>
    /// Sets the input for how much of linearForce and angularForce are applied
    /// to the ship. Each component of the input vectors is assumed to be scaled
    /// from -1 to 1, but is not clamped.
    /// </summary>
    private void SetPhysicsInput(Vector3 linearInput, Vector3 angularInput)
    {
        _appliedLinearForce = MultiplyByComponent(linearInput, linearForce) * forceMultiplier;
        _appliedAngularForce = MultiplyByComponent(angularInput, angularForce) * forceMultiplier;
    }

    /// <summary>
    /// Turns the main engine intertial dampening off or on, by disabling the linear drag on the ship.
    /// </summary>
    public void ToggleEngines(bool force = false)
    {
        // if (!isEngineOn)
        // {
            Rigidbody.drag = 0;
            // ConsoleOutput.PostMessage("Engines on. ", Color.yellow);
            // //ship.enginesOn = true;
            IsEngineOn = true;
        // }
        // else
        // {
        //     rbody.drag = rBodyDrag;
        //     ConsoleOutput.PostMessage("Engines off. ", Color.yellow);
        //     //ship.enginesOn = false;
        //     IsEngineOn = false;
        // }
        // if (force)
        //     rbody.drag = rBodyDrag;
    }

    #region helper methods
    /// <summary>
    /// Returns a Vector3 where each component of Vector A is multiplied by the equivalent component of Vector B.
    /// </summary>
    private static Vector3 MultiplyByComponent(Vector3 a, Vector3 b)
    {
        Vector3 ret;

        ret.x = a.x * b.x;
        ret.y = a.y * b.y;
        ret.z = a.z * b.z;

        return ret;
    }

    /// <summary>
    /// Clamps vector components to a value between the minimum and maximum values given in min and max vectors.
    /// </summary>
    /// <param name="vector">Vector to be clamped</param>
    /// <param name="min">Minimum vector components allowed</param>
    /// <param name="max">Maximum vector components allowed</param>
    /// <returns></returns>
    public static Vector3 ClampVector3(Vector3 vector, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(vector.x, min.x, max.x),
            Mathf.Clamp(vector.y, min.y, max.y),
            Mathf.Clamp(vector.z, min.z, max.z)
            );
    }
    #endregion helper methods
}
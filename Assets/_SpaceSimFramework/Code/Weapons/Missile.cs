using System;
using UnityEngine;

// Missile guidance system
public class Missile : MonoBehaviour
{
    private static float ARM_TIME = 1f;

    private PIDController _pidAngle, _pidVelocity;
    private float pid_P = 2, pid_I = 0.8f, pid_D = 0.8f;

    // Properties
    private float _missileSpeed;
    private int _damage;
    private Color _trailColor;
    private float _turnRate;
    private bool _isGuided;
    private float _range;

    // Local members
    private Transform _target;
    private Rigidbody _rBody;
    private Vector3 _angularTorque;
    private bool _isPlayerShot;
    public Ship owner;
    private float _timer;
    private Vector3 _lastPos;
    private float _distanceTravelled;
    private ParticlePool _particlePool;

    private void Awake()
    {
        _particlePool = GameObject.Find("ParticlePool").GetComponent<ParticlePool>();
    }
    public void FireProjectile(MissileWeaponData missileWeaponData, Transform target, Ship owner, bool isPlayerShot)
    {
        _missileSpeed = missileWeaponData.MissileSpeed;
        _damage = missileWeaponData.Damage;
        _trailColor = missileWeaponData.TrailColor;
        _turnRate = missileWeaponData.TurnRate;
        _isGuided = missileWeaponData.IsGuided;
        _range = missileWeaponData.Range;
        this._target = target;
        this._isPlayerShot = isPlayerShot;
        this.owner = owner;
        _rBody = gameObject.GetComponent<Rigidbody>();
        _pidAngle = new PIDController(pid_P, pid_I, pid_D);
        _pidVelocity = new PIDController(pid_P, pid_I, pid_D);
        _lastPos = transform.position;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        _distanceTravelled += Vector3.Distance(_lastPos, transform.position);
        if (_distanceTravelled > _range)
            GameObject.Destroy(gameObject);
        _lastPos = transform.position;

        if (_target == null)
            return;

        // Turn missile towards target
        SteerTowardsDestination(_isGuided
            ? Targeting.PredictTargetLead3D(gameObject, _target.gameObject, _missileSpeed)
            : _target.position);
    }

    private void FixedUpdate()
    {
        if (_rBody == null) return;
        _rBody.AddRelativeForce(new Vector3(0, 0, _missileSpeed), ForceMode.Force);
        _rBody.AddRelativeTorque(ShipPhysics.ClampVector3(_angularTorque, -Vector3.one * _turnRate, Vector3.one*_turnRate), ForceMode.Force);
    }

    private void SteerTowardsDestination(Vector3 destination)
    {
        var distance = Vector3.Distance(destination, transform.position);

        if (distance > 10)
        {
            Vector3 angularVelocityError = _rBody.angularVelocity * -1;
            Vector3 angularVelocityCorrection = _pidVelocity.Update(angularVelocityError, Time.deltaTime);

            Transform transform1;
            Vector3 lavc = (transform1 = transform).InverseTransformVector(angularVelocityCorrection);

            Vector3 desiredHeading = destination - transform1.position;
            Vector3 currentHeading = transform1.forward;
            Vector3 headingError = Vector3.Cross(currentHeading, desiredHeading);
            Vector3 headingCorrection = _pidAngle.Update(headingError, Time.deltaTime);

            // Convert angular heading correction to local space to apply relative angular torque
            Vector3 lhc = transform.InverseTransformVector(headingCorrection * 200f);

            _angularTorque = lavc + lhc;
        }
        else
        {
            _angularTorque = Vector3.zero;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (_timer > ARM_TIME)
        {
            _particlePool.CreateParticleEffectAtPos(transform.position);

            if (other.gameObject.CompareTag("Ship")||other.gameObject.CompareTag("ForceField"))
            {
                //if (!other.transform.GetComponent<ShipEquipment>().shieldActive)
                    other.gameObject.GetComponent<Ship>().TakeDamage(_damage, _isPlayerShot);
            }
            else if (other.gameObject.CompareTag("Asteroid"))
                other.gameObject.GetComponent<Asteroid>().TakeDamage(_damage);
            else if (other.gameObject.CompareTag("StationParts")||other.gameObject.CompareTag("Station"))
                other.gameObject.GetComponentInParent<Station>().TakeDamage(_damage, owner, _isPlayerShot);
            
            GameObject.Destroy(gameObject);
        }
    }

    private void OnDisable()
    {
        var particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        particleSystem.transform.parent = null;
        Destroy(particleSystem.gameObject, 5.0f);
    }
}

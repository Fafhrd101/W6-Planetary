using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(SphereCollider))]

public class PrTurret : MonoBehaviour
{

    public enum turretState
    {
        Idle,
        ChasingTarget,
        Shooting,
        Dead
    }
    

    [Header("Basic AI Settings")]
    private bool _isActive = true;
    public Faction owningFaction;
    //[HideInInspector]
    public turretState actualState = turretState.Idle;

    [Header("Turret Settings")]
    public float awarenessDistance = 10.0f;
    public float rotationSpeed = 1.0f;
    [Range(0f, 360f)]
    public float rotationLimit = 60.0f;
    public float timeLimit = 0.0f;
    private float _actualTimeLimit = 0.0f;
    private Quaternion _initialRot; 

    [Header("Turret Weapon Settings")]
    public bool attackPlayers = false;
    public bool attackEnemies = true;
    public Transform weaponSlot;
    public GameObject weapon;
    public Transform Target;
    private PrWeapon _weaponComp;
    private GameObject _weaponGO;
    public bool overrideWeaponStats = true;
    public int bulletDamage = 1;
    public float bulletSpeed = 1.0f;
    public float bulletAccel = 0.0f;
    public float fireRate = 0.5f;
    public int clipSize = 10;
    public float reloadTime = 1.0f;

    [Header("Turret Animation Settings")]
    public bool useAnimation = true;
    private bool _animate = false;
    public AnimationCurve shootingMovement;
    public float movementFactor = 1.0f;
    public float animationSpeed = 1.0f;
    private float _animationTimer = 0.0f;

    private float _shootTimer = 1.0f;
    private float _actualTimer = 0.0f;

    [Header("Turret Visuals")]
    public bool showArea = true;
    public Renderer areaMesh;
    public bool applyColors = true;
    public Color attackEverythingColor = Color.white;
    public Color attackEnemiesColor = Color.white;
    public Color attackPlayerColor = Color.white;

    //Targeting variables
    private List<GameObject> _playerGO;
    //private List<GameObject> enemiesGO;
    private bool _targetsInRange = false;
    private Vector3 _actualTargetPos = Vector3.zero;
    [HideInInspector]
    public bool enemyDead = false;

    [Header("Debug")]
    //public bool doNotAttackPlayer = false;
    public bool DebugOn = false;
    //public TextMesh DebugText;
    public Mesh AreaMesh;

    private static readonly int Angle = Shader.PropertyToID("_Angle");

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    //public Mesh TargetArrow;

    // Use this for initialization
    void Start()
    {
        _playerGO = new List<GameObject>();
        //enemiesGO = new List<GameObject>();

        if (weaponSlot && weapon)
        {
            _weaponGO = Instantiate(weapon, weaponSlot.position, weaponSlot.rotation) as GameObject;
            _weaponGO.transform.parent = weaponSlot.transform;
            _weaponComp = _weaponGO.GetComponent<PrWeapon>() as PrWeapon;
            _weaponComp.attacker = this.gameObject;
            _weaponComp.ainpcTarget = Target;
            _weaponComp.aiWeapon = true;
            _weaponComp.turretWeapon = true;
            // if (attackPlayers && !attackEnemies)
            //     weaponComp.team = 1;
            // else if (attackEnemies && !attackPlayers)
            //     weaponComp.team = 0;
            // else if (attackPlayers && attackEnemies)
            //     weaponComp.team = 3;
            _initialRot = weaponSlot.rotation;
            GetComponent<SphereCollider>().radius = awarenessDistance;
            if (overrideWeaponStats)
                OverrideWeaponParameters();
            _actualTimer = _weaponComp.FireRate;
            _shootTimer = _weaponComp.FireRate;
        }

        if (showArea && areaMesh)
        {
            SetAreaVisuals(showArea);
        }
        else if (areaMesh)
        {
            SetAreaVisuals(showArea);
        }
    }

    void SetAreaVisuals(bool active)
    {
        areaMesh.GetComponent<Transform>().localScale = new Vector3(awarenessDistance, awarenessDistance, awarenessDistance) * 2;
        areaMesh.material.SetFloat(Angle, rotationLimit * 0.5f);
        areaMesh.enabled = active;

        if (applyColors)
        {
            if (attackPlayers && !attackEnemies)
                areaMesh.material.SetColor(BaseColor, attackPlayerColor);
            else if (attackEnemies && !attackPlayers)
                areaMesh.material.SetColor(BaseColor, attackEnemiesColor);
            else if (attackEnemies && attackPlayers)
                areaMesh.material.SetColor(BaseColor, attackEverythingColor);
        }

    }

    void OverrideWeaponParameters()
    {
        _weaponComp.FireRate = fireRate;
        _weaponComp.BulletDamage = bulletDamage;
        _weaponComp.BulletSpeed = bulletSpeed;
        _weaponComp.BulletAccel= bulletAccel;
        _weaponComp.Bullets = clipSize;
        _weaponComp.ReloadTime = reloadTime;
    }

    void ChangeState()
    {
        
        if (actualState == turretState.ChasingTarget)
        {
            actualState = turretState.Idle;
        }
    }

    void LateUpdate()
    {
        Vector3 slotPos = _weaponGO.transform.localPosition;

        if (_animate && useAnimation)
        {
            slotPos.z = shootingMovement.Evaluate(_animationTimer) * movementFactor;
            _weaponGO.transform.localPosition = slotPos;

            // Increase the timer by the time since last frame
            _animationTimer += Time.deltaTime * animationSpeed;
            if (_animationTimer > 2.0f)
                _animate = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
        if (timeLimit > 0.0f)
        {
            _actualTimeLimit += Time.deltaTime;
            if (_actualTimeLimit >= timeLimit)
            {
                _isActive = false;
            }
        }
        if (weaponSlot && _isActive)
        {
            if (_targetsInRange)
            {
                float closestDistance = awarenessDistance;

                foreach (GameObject p in _playerGO)
                {
                    if (p == null) continue;
                    if (p.GetComponent<PrTopDownCharInventory>())
                        enemyDead = p.GetComponent<PrTopDownCharInventory>().isDead;
                    else if (p.GetComponent<PrNPCAI>())
                        enemyDead = p.GetComponent<PrNPCAI>().dead;

                    if (!enemyDead)
                    {
                        Vector3 targetDir = (p.transform.position + new Vector3(0f, 1.6f, 0f)) - (weaponSlot.position);

                        float angle = Vector3.Angle(targetDir, transform.forward);
                        if (angle < rotationLimit * 0.54f)
                        {
                            float enemyDistance = Vector3.Distance(weaponSlot.position, p.transform.position + new Vector3(0.0f, 1.6f, 0.0f));

                            if (closestDistance > enemyDistance)
                            {
                                //Check if the enemy is visible to add it to the list of possible enemies and look for the closest target
                                RaycastHit hit;

                                if (Physics.Raycast(weaponSlot.position, targetDir, out hit))
                                {
                                    if (hit.collider.CompareTag("Player") && attackPlayers)
                                    {
                                        closestDistance = enemyDistance;
                                        _actualTargetPos = p.transform.position + new Vector3(0f, 1.6f, 0f);
                                        Target.position = p.transform.position;
                                        actualState = turretState.ChasingTarget;
                                    }
                                    else if (hit.collider.CompareTag("NPC") && attackEnemies)
                                    {
                                        closestDistance = enemyDistance;
                                        _actualTargetPos = p.transform.position + new Vector3(0f, 1.6f, 0f);
                                        Target.position = p.transform.position;
                                        actualState = turretState.ChasingTarget;
                                    }
                                    else
                                    {
                                        if (actualState == turretState.ChasingTarget)
                                            ChangeState();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (actualState == turretState.ChasingTarget)
                                ChangeState();
                        }
                    }
                    else
                    {
                        _playerGO.Remove(p);
                        ChangeState();
                        break;
                    }
                }
            }
            

            switch (actualState)
            {
               
                case turretState.Idle:
                    weaponSlot.rotation = Quaternion.Lerp(weaponSlot.rotation, _initialRot, 4 * Time.deltaTime);

                    // Debug.Log("Aiming Player");
                    break;
                case turretState.ChasingTarget:
 
                        Quaternion actualRot = weaponSlot.rotation;
                        weaponSlot.LookAt(_actualTargetPos);
                        Quaternion newRot = weaponSlot.rotation;
                        weaponSlot.rotation = Quaternion.Lerp(actualRot, newRot, rotationSpeed * Time.deltaTime);

                        if (_actualTimer >= 0.0f)
                            _actualTimer -= Time.deltaTime;
                        else
                        {
                            _actualTimer = _shootTimer;
                            _weaponComp.Shoot();
                            // print("Turret shooting");
                            if (!_weaponComp.reloading)
                            {
                                _animate = true;
                                _animationTimer = 0.0f;
                            }
                            else
                            {
                                _animate = false;
                            }                           

                        }
                        weaponSlot.transform.eulerAngles = new Vector3(0.0f, weaponSlot.transform.eulerAngles.y, 0.0f);
                  //  }
                    
                    // Debug.Log("Attacking");
                    break;
                /*case turretState.Shooting:
                    if (actualTimer >= 0.0f)
                        actualTimer -= Time.deltaTime;
                    else
                    {
                        actualTimer = shootTimer;
                        weaponComp.Shoot();
                        animate = true;
                        animationTimer = 0.0f;

                    }
                    // Debug.Log("Attacking");
                    break;*/
                case turretState.Dead:
                    //StopMovement();
                    //Debug.Log("Dead");
                    break;
                default:
                    // Debug.Log("NOTHING");
                    break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive) return;
        if (other.gameObject.CompareTag("Player") && attackPlayers)
        {
            if (owningFaction.RelationWith(Player.Instance.playerFaction) < 0.25)
            {
                _playerGO.Add(other.gameObject);
                _targetsInRange = true;
            }

        }
        if (other.gameObject.CompareTag("NPC") && attackEnemies)
        {
            if (owningFaction.RelationWith(other.gameObject.GetComponent<PrNPCAI>().faction) < 0.25)
            {
                _playerGO.Add(other.gameObject);
                _targetsInRange = true;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (!_isActive) return;
        if (other.gameObject.CompareTag("Player") && attackPlayers)
        {
            _playerGO.Remove(other.gameObject);
        }
        if (other.gameObject.CompareTag("NPC") && attackEnemies)
        {
            _playerGO.Remove(other.gameObject);
        }

        if (_playerGO.Count <= 0 /*&& enemiesGO.Count <= 0*/)
        {
            _targetsInRange = false;
            ChangeState();
        }
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.white;

        /*if (player && playerIsVisible)
        {
            if (eyesAndEarTransform)
                Gizmos.DrawLine(playerTransform.position + new Vector3(0, eyesAndEarTransform.position.y, 0), eyesAndEarTransform.position);
            else
                Gizmos.DrawLine(playerTransform.position + new Vector3(0f, 1.6f, 0f), transform.position + new Vector3(0f, 1.6f, 0f));
        }*/

        Quaternion lRayRot = Quaternion.AngleAxis(-rotationLimit * 0.5f, Vector3.up);
        Quaternion rRayRot = Quaternion.AngleAxis(rotationLimit * 0.5f, Vector3.up);
        Vector3 lRayDir = lRayRot * transform.forward;
        Vector3 rRayDir = rRayRot * transform.forward;
        if (weaponSlot)
        {
            Gizmos.DrawRay(weaponSlot.position, lRayDir * awarenessDistance);
            Gizmos.DrawRay(weaponSlot.position, rRayDir * awarenessDistance);
        }
        else
        {
            Gizmos.DrawRay(transform.position + new Vector3(0f, 1.6f, 0f), lRayDir * awarenessDistance);
            Gizmos.DrawRay(transform.position + new Vector3(0f, 1.6f, 0f), rRayDir * awarenessDistance);
        }

        Gizmos.DrawMesh(AreaMesh, transform.position, Quaternion.identity, Vector3.one * awarenessDistance);
    }
}

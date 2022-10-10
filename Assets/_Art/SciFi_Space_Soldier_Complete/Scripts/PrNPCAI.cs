using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DunGen.DungeonCrawler;
using Npc_AI;
using PixelCrushers.DialogueSystem;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PrNPCAI : MonoBehaviour
{

    public enum NpcType
    {
        Merchant,       // Merchants buy/sell stuff. Most stay near shop.
        Worker,         // Uses subTypes, otherwise a Citizen in the eyes of the law
        Soldier,        // Typically patrols region. May be static. Can be Raider or Police/Military
        Citizen,        // Wanders area, interacts with others.
        Visitor         // Wanders area, may interact.
    }

    public enum NpcSubType
    {   
           Unused,         // default for everyone except workers
           Farmer,         // Just a job, handles animations
           FisherMan,      // Just a job, handles animations
           Lumberjack,     // Just a job, handles animations
           Cook,           // Just a job, handles animations
           Gatherer,       // Just a job, handles animations
           Miner,          // Just a job, handles animations
           Builder,        // Just a job, handles animations    
           Librarian,      // Could be barkeeper, computer terminal, etc. Primary job is provide info/quests  
    }
    
    public enum AIState
    {
        Patrol,         // Uses POIs, randomly seeking out new ones. Should be changed.
        Wander,         // Uses POIs, randomly seeking out new ones
        Static,         // Don't move from start position
        ChasingTarget,
        AimingTarget,
        Attacking,
        CheckingSound,
        FriendlyFollow, // Pet, teamMates, etc.
        Idle,           // Ponder next state.
        GoingToWork,
        Working,        // Play job animations, look for interaction requests
        GoingHome,
        InteractingWithPlayer, // May not use
        Talking,        // With another NPC, plays animations
        Scared,         // Moving away from attacker
        Dead
    }

    [Header("Health and Stats")]
    public int health = 100;
    public NpcType type;
    [Tooltip("Workers can be sub types. Others ignore.")]
    public NpcSubType subType;
    [Tooltip("Sets a job animation for workers")]
    public int specialization;
    public bool dead;  
      
    [Header("Basic AI Settings")]
    public AIState currentState;
    public Transform targetTransform; 
    [HideInInspector] // master list
    public List<GameObject> npcList;
    public List<GameObject> targetList;
    public List<GameObject> protectList;
    public List<GameObject> currentVisible;
    private GameObject _lastAttacker;
    
    [Header("Speech")] 
    public string conversation;
    [HideInInspector]
    public BarkOnIdle bark;
    private float _barkLimiter = 3;
    private float _barkTimer = 3; // Allows first one to fire, timed from there
    [HideInInspector]
    public DialogueActor actor;
    private DialogueSystemController _dialogueSystem;
    
    [Header("Timed Actions")]
    public Transform home;
    public Transform work;
    public int distBetweenWorkAndHome;
    public POI interactingWithPoi;
    
    private DayAndNightControl _dayAndNightControl;
    private POI _nextPoi;

    [Header("Faction Relations")]
    public Faction faction;
    public float factionPenalty = 0.15f, dampeningFactor = 4f;
    public float relationWithPlayer;
    private GameObject _closestFriend;
    private float _closestFriendDistance = 99999.0f;
    private readonly Dictionary<Faction, float> _factionRelationsBackup = null;
    
    [Header("Weapon Settings")]
    public bool useArmIK = true;
    public Transform WeaponGrip;
    [Tooltip("Matches WeaponGrip")]
    public Transform rightHand;
    public Transform leftHand;
    public GameObject[] weaponChoices;
    public GameObject assignedWeapon;
    public float fireRate = 1.0f;
    public float attackAngle = 5f;
    public int meleeAttacksOptions = 1;
    public bool chooseRandomMeleeAttack = true;
    
    private int _actualMeleeAttack;    
    private PrWeapon _weapon;
    private float _lastFireTimer;

    [Header("Speeds")]
    public float chasingSpeed = 1.0f;
    public float normalSpeed = 0.75f;
    public float aimingSpeed = 0.3f;
    public bool stopIfGetHit = true;
    public float rotationSpeed = 0.15f;
    public bool useRootmotion = true;
    public float distToTarget;
    
    private bool _standInPlace;
    private bool _lockRotation;
    private readonly Vector3 _lockedRotDir = Vector3.zero;
    private bool _canAttack = true;

    [Header("AI Sensor Settings")] 
    public LayerMask visibleLayers;
    public float awarenessDistance = 20;
    public float aimingDistance = 15;
    public float attackDistance = 8;
    private float _targetDistance = 99999;
    [Range(10f, 360f)]
    public float lookAngle = 90f;
    public float hearingDistance = 20;
    public Transform eyesAndEarTransform;
    private Transform _actualSensorTrans;
    private Vector3 _actualSensorPos;
    [HideInInspector]
    public bool lookForPlayer;
    private Animator _animator;
    public bool targetIsVisible;
    private float _targetLastTimeSeen;
    //[SerializeField] private float forgetPlayerTimer = 5.0f;
    private Vector3 _lastNoisePos;
    private const float AlarmedTimer = 10.0f;
    private float _actualAlarmedTimer;
    private float _newtAlarm;
    
    [Header("Waypoints Settings")]
    public GameObject[] waypointRoute;
    private Waypoint _prevNode;
    private Waypoint _currentNode;
    private Waypoint _destNode;
    public bool waiting;
    [HideInInspector]
    public Transform[] waypoints;
    public float timeToWait = 3.0f;
    private float _actualTimeToWait;
    private float _waitTimer;
    public Vector3 finalGoal = Vector3.zero;

    [Header("Temperature Settings")]
    public bool useTemperature = true;
    public float temperature = 1.0f;
    public int temperatureDamage = 5;
    public float onFireSpeedFactor = 0.5f;
    private float _tempTimer;
    
    [Header("VFX")]
    public Renderer[] MeshRenderers;
    public int hitAnimsMaxTypes = 1;
    public GameObject spawnFX;
    public GameObject damageVFX;
    public GameObject explosionFX;
    public bool destroyOnDead;
    private Vector3 _lastHitPos = Vector3.zero;
    private bool _damaged;
    private float _damagedTimer;
    public bool destroyDeadBody;
    public float destroyDeadBodyTimer = 5.0f;
    public Transform BurnAndFrozenVFXParent;
    public GameObject frozenVFX;
    private GameObject _actualFrozenVFX;
    public GameObject burningVFX;
    private GameObject _actualBurningVFX;
    public GameObject damageSplatVFX;
    private PrBloodSplatter _actualSplatVFX;
    public GameObject deathVFX;
    public float deathVFXHeightOffset = -0.1f;
    private GameObject _actualDeathVFX;
    public bool useExplosiveDeath = true;
    private bool _explosiveDeath;
    public int damageThreshold = 50;
    public GameObject explosiveDeathVFX;
    private GameObject _actualExplosiveDeathVFX;
    
    [Header("Ragdoll setup")]
    public bool useRagdollDeath;
    public float ragdollForceFactor = 1.0f;
    private Vector3 _ragdollForce;

    [Header("Sound FX")]
    //
    // public float FootStepsRate = 0.4f;
    // public float generalFootStepsVolume = 1.0f;
    // public AudioClip[] Footsteps;
    // private float LastFootStepTime = 0.0f;
    private AudioSource _audio;

    [HideInInspector]
    public NavMeshAgent agent;

    [Header("Debug")]
    public bool doNotAttackTarget;

    private PrCharacterIK _characterIKController;
    private Transform _armIKTarget;
    private NavMeshPath _path;
    private bool _stopPlaying;
    private PlanetaryBaseSetup _setup;
    
    // Animations we can call
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int MeleeType = Animator.StringToHash("MeleeType");
    private static readonly int MeleeAttack = Animator.StringToHash("MeleeAttack");
    private static readonly int FrozenMix = Shader.PropertyToID("_FrozenMix");
    private static readonly int BurningMix = Shader.PropertyToID("_BurningMix");
    private static readonly int Aiming = Animator.StringToHash("Aiming");
    private static readonly int FX = Shader.PropertyToID("_DamageFX");
    private static readonly int Temperature = Animator.StringToHash("Temperature");
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Alert = Animator.StringToHash("Alert");
    private static readonly int CancelAlert = Animator.StringToHash("CancelAlert");
    private static readonly int Dead = Animator.StringToHash("Dead");
    private static readonly int Type = Animator.StringToHash("Type");
    private static readonly int Hit = Animator.StringToHash("Hit");

    // Use this for initialization
    public virtual void Start()
    {
        bark = GetComponent<BarkOnIdle>();
        actor = GetComponent<DialogueActor>();
        _dialogueSystem = GameObject.FindObjectOfType<DialogueSystemController>();
        _setup = GameObject.FindObjectOfType<PlanetaryBaseSetup>();
        DayAndNightCycle_Initialize();
        agent = GetComponent<NavMeshAgent>();
        _path = new NavMeshPath();

        if (spawnFX)
        {
            var fx = Instantiate(spawnFX, transform.position, Quaternion.identity);
            fx.transform.SetParent(ParticlePool.Instance.transform);
        }
        
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        _actualSensorTrans = eyesAndEarTransform ? eyesAndEarTransform : this.transform;
        _actualSensorPos = _actualSensorTrans.position;
        SetTimeToWait();
        _audio = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        if (GetComponent<PrCharacterIK>() == null)
            gameObject.AddComponent<PrCharacterIK>();
        _characterIKController = GetComponent<PrCharacterIK>();

        InstantiateWeapon();
        BuildMobLists();
        CheckTargetVisibility(360f);
        if (useRootmotion)
            _animator.applyRootMotion = false;
        gameObject.AddComponent<PrCharacterRagdoll>();
        
#region FX-Related
        var trans = ParticlePool.Instance.transform;
        var gOactualSplatVFX = Instantiate(damageSplatVFX, trans.position, trans.rotation, trans);
        
        if (useExplosiveDeath && explosiveDeathVFX)
        {
            _actualExplosiveDeathVFX = Instantiate(explosiveDeathVFX, transform.position, transform.rotation);
            _actualExplosiveDeathVFX.SetActive(false);
            _actualExplosiveDeathVFX.transform.parent = trans;
        }
        if (damageSplatVFX)
        {
            _actualSplatVFX = gOactualSplatVFX.GetComponent<PrBloodSplatter>();
        }
        if (frozenVFX)
        {
            _actualFrozenVFX = Instantiate(frozenVFX, trans.position, trans.rotation);
            _actualFrozenVFX.transform.parent = trans;
        }
        if (burningVFX)
        {
            _actualBurningVFX = Instantiate(burningVFX, trans.position, trans.rotation);
            _actualBurningVFX.transform.parent = trans;
        }
        if (deathVFX)
        {
            _actualDeathVFX = Instantiate(deathVFX, trans.position, trans.rotation);
            _actualDeathVFX.SetActive(false);
            _actualDeathVFX.transform.parent = trans;
        }
#endregion

        agent.enabled = true;
        // if (currentState == AIState.Static) return;
        // waypointRoute = _setup.waypoints;
        // _currentNode = FindClosestWaypoint(transform.position);
        // _prevNode = _currentNode;
        // SetDestination(finalGoal);
    }

    private void DayAndNightCycle_Initialize()
    {
        //_dayAndNightControl = FindObjectOfType<DayAndNightControl>();
        _dayAndNightControl = DayAndNightControl.Instance;
        
        if (_dayAndNightControl != null)
        {
            _dayAndNightControl.OnMorningHandler += GoToWork;
            _dayAndNightControl.OnEveningHandler += GoHome;
        }
        else
        {
            Debug.Log("Add in dayAndNight control to scene for use of npc's life cycle");
        }
    }

    private void GoToWork()
    {
        if (currentState == AIState.Working) return;
        ChangeState(AIState.GoingToWork);
    }

    private void GoHome()
    {
        ChangeState(AIState.GoingHome);
    }

    public void StopAllActivities()
    {
        //Debug.Log("Stop Moving");
        _stopPlaying = true;
        _animator.SetFloat(Speed, 0.0f);
        _animator.SetBool(Aiming, false);
    }

    public void BuildMobLists()
    {
        targetList = new List<GameObject>();//GameObject.FindGameObjectsWithTag("Player")
        protectList = new List<GameObject>();
        npcList = new List<GameObject>(GameObject.FindGameObjectsWithTag("NPC"));
        
        // Only soldiers have default targets and default protectees.
        if (type == NpcType.Soldier && faction.RelationWith(Player.Instance.playerFaction) < 0.25f)
            targetList.Add(GameObject.FindGameObjectWithTag("Player"));
        foreach (var f in npcList)
        {
            if (f == null || f == gameObject) continue;
            if (type == NpcType.Soldier && faction.RelationWith(f.GetComponent<PrNPCAI>().faction) < 0.25f &&
                f.GetComponent<PrNPCAI>().type != NpcType.Merchant && f.GetComponent<PrNPCAI>().type != NpcType.Citizen &&
                f.GetComponent<PrNPCAI>().type != NpcType.Visitor) 
                targetList.Add(f);
            if (type == NpcType.Soldier && faction.RelationWith(f.GetComponent<PrNPCAI>().faction) > 0.25f) 
                protectList.Add(f);
            
            // Everyone is willing to assist their own faction
            if (faction == f.GetComponent<PrNPCAI>().faction)
            {
                protectList.Add(f);
                // just in case it got added
                if (targetList.Contains(f))
                    targetList.Remove(f);
            }
        }
        GetClosestFriends();
        GetClosestTarget();
    }

    private void GetClosestFriends()
    {
        if (targetList.Count == 0) return;
        foreach (var f in protectList)
        {
            if (f == null) continue;
            if (!(_closestFriendDistance > Vector3.Distance(transform.position, f.transform.position))) continue;
            _closestFriend = f;
            _closestFriendDistance = Vector3.Distance(transform.position, f.transform.position);
        }
    }
    
    private void GetClosestTarget()
    {
        if (targetList.Count != 0)
        {
            // Prioritize our targets somehow
            foreach (var p in targetList)
            {
                if (p == null) continue;
                if (!(_targetDistance >
                      Vector3.Distance(_actualSensorPos, p.transform.position + new Vector3(0.0f, 1.6f, 0.0f))))
                    continue;
                //if (IsVisible(p))
                //{
                    targetTransform = p.transform;
                    _targetDistance = Vector3.Distance(_actualSensorPos,
                        p.transform.position + new Vector3(0.0f, 1.6f, 0.0f));
                    if (_targetDistance > 30) targetTransform = null;
                //}
                // else
                //     print("Choosing target "+targetTransform);
            }
        }
        else 
        {
            _targetDistance = 9999.0f;
            targetTransform = null;
        }
    }

    private bool IsVisible(GameObject mob)
    {
        foreach (var p in currentVisible)
        {
            if (mob == p)
                return true;
        }

        return false;
    }
    private void OnAnimatorMove()
    {
        if (agent != null && useRootmotion)
            agent.velocity = _animator.deltaPosition / Time.deltaTime;
    }
    // SendMessage() calls this
    private void BulletPos(Vector3 bulletPosition)
    {
        _lastHitPos = bulletPosition;
        _lastHitPos.y = 0;
    }
    // SendMessage() calls this
    private void SetAttacker(GameObject attacker)
    {
        //print("Assigning attacker");
        _lastAttacker = attacker;
    }
    // Deed events call  this
    public void OnReact()
    {
        print("reacting to a crime");
        switch (type)
        {
            case NpcType.Soldier:
                print("Direct response");
                break;
            default:
                print("Finding some help");
                break;
        }
    }
    
    private void ApplyDamagePassive(int damage)
    {
        if (currentState == AIState.Dead) return;
        health -= damage;

        if (health > 0) return;
        if (damage >= damageThreshold)
            _explosiveDeath = true;
        if (_actualSplatVFX)
            _actualSplatVFX.transform.parent = null;
                
        Die(true);
    }
    
    public void CanAttack() {}
    
   public void ApplyDamage(int damage)
   {
       if (currentState == AIState.Dead) return;
       EnableArmIK(false);

       if (_weapon.type == PrWeapon.Wt.Melee)
           SetCanAttack(false);
            
       //Get Damage Direction
       var hitDir = new Vector3(_lastHitPos.x,0, _lastHitPos.z) - transform.position;
       var front = transform.forward;
       
       //print("I got hit, looking where from");
       transform.LookAt(hitDir); 
       CheckTargetVisibility(lookAngle);
       //if (playerTransform == null) print("Can't see player");
       if (type != NpcType.Worker)
       {
           _animator.SetTrigger(Hit);
           _animator.SetInteger(Type, Random.Range(0, hitAnimsMaxTypes));
       }
            
       _damaged = true;
       _damagedTimer = 1.0f;

       if (stopIfGetHit)
       {
           agent.velocity = Vector3.zero;
           currentState = AIState.ChasingTarget;
           // This should change over to love/hate version?
           if (_lastAttacker)
           {
               // if (_lastAttacker.GetComponent<PrNPCAI>())
               //     _lastAttacker.GetComponent<PrNPCAI>().AddFactionPenalty(faction);
               // else Player.Instance.AddFactionPenalty(faction);
           }
           // else print("Hit with no attacker");
       }
       
       if (targetTransform != null)
       {
           agent.ResetPath();
           CheckPlayerNoise(targetTransform.position);
           currentState = AIState.ChasingTarget;
       }
       health -= damage;

       if (_actualSplatVFX)
       {
           _actualSplatVFX.transform.LookAt(_lastHitPos);
           _actualSplatVFX.Splat();
       }

       if (health > 0) return;
       if (damage >= damageThreshold)
           _explosiveDeath = true;
       if (_actualSplatVFX)
           _actualSplatVFX.transform.parent = null;
       
       if (faction == ObjectFactory.Instance.GetFactionFromName("Raider"))
       {
           if (!_lastAttacker || !_lastAttacker.GetComponent<PrTopDownCharInventory>())
               Player.Instance.raidersKilledByOthers++;
           else if (_lastAttacker && _lastAttacker.GetComponent<PrTopDownCharInventory>())
               Player.Instance.raidersKilledByPlayer++;
           else
               print("Undocumented Raider death!");
       }
           
       Die(false);
   }

    private void ApplyDamageNoVFX(int damage)
    {
        if (currentState == AIState.Dead) return;
        health -= damage;
        if (health > 0) return;
        if (damage >= damageThreshold)
            _explosiveDeath = true;
        if (_actualSplatVFX)
            _actualSplatVFX.transform.parent = null;
        Die(true);
    }

    private void NpcDestruction(bool temperatureDeath)
    {
        //print("attempting destruction");
        currentState = AIState.Dead;
        if (type != NpcType.Worker)
            _animator.SetBool(Dead, true);
        GetComponent<CharacterController>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
        Destroy(GetComponent<NavMeshAgent>());
        if (useRagdollDeath)
        {
            GetComponent<PrCharacterRagdoll>().ActivateRagdoll();
            _ragdollForce = ((transform.position + new Vector3(0, 1.5f, 0)) - (_lastHitPos + new Vector3(0f, 1.6f, 0f))) * (ragdollForceFactor * Random.Range(0.8f, 1.2f));
            
            if (!temperatureDeath)
                GetComponent<PrCharacterRagdoll>().SetForceToRagdoll(_lastHitPos + new Vector3(0f, 1.6f, 0f), _ragdollForce, BurnAndFrozenVFXParent);
        }

        if (_explosiveDeath && _actualExplosiveDeathVFX)
        {
            _actualExplosiveDeathVFX.transform.position = transform.position;
            _actualExplosiveDeathVFX.transform.rotation = transform.rotation;
            _actualExplosiveDeathVFX.SetActive(true);
            _actualExplosiveDeathVFX.SendMessage("SetExplosiveForce", _lastHitPos + new Vector3(0, 1.5f, 0), SendMessageOptions.DontRequireReceiver);
          
            Destroy(this.gameObject);
        }
        else
        {
            if (!deathVFX || !_actualDeathVFX) return;
            var particles = _actualDeathVFX.GetComponentsInChildren<ParticleSystem>();
            if (temperatureDeath)
            {
                //Freezing or Burning Death VFX...
            }
            else
            {
                _actualDeathVFX.transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
                _actualDeathVFX.transform.LookAt(_lastHitPos);
                _actualDeathVFX.transform.position = new Vector3(transform.position.x, deathVFXHeightOffset, transform.position.z);

                _actualDeathVFX.SetActive(true);

                if (particles.Length <= 0) return;
                foreach (var p in particles)
                    p.Play();
            }
        }
    }

    private void Die(bool temperatureDeath)
    {
        EnableArmIK(false);
        // Keep lists clean so we can respawn. Merchants won't.
        if (_setup.raiders.Contains(gameObject))
            _setup.raiders.Remove(gameObject);
        if (_setup.police.Contains(gameObject))
            _setup.police.Remove(gameObject);
        if (_setup.citizens.Contains(gameObject))
            _setup.citizens.Remove(gameObject);
        if (_setup.visitors.Contains(gameObject))
            _setup.visitors.Remove(gameObject);
        if (_setup.workers.Contains(gameObject))
            _setup.workers.Remove(gameObject);
        //print(name+" died");
        gameObject.tag = "Untagged";
        var hitDir = _lastHitPos - transform.position;
        NpcDestruction(temperatureDeath);
        dead = true;
    }

    private void EnableArmIK(bool active)
    {
        if (_characterIKController && useArmIK)
            _characterIKController.ikHandsActive = active;
    }
    
    private void EnableHeadIK(bool active)
    {
        if (_characterIKController)
            _characterIKController.ikHeadActive = active;
    }
    
    private void InstantiateWeapon()
    {
        // We may have assigned this upon loading
        if (assignedWeapon == null)
            assignedWeapon = weaponChoices[0];//weaponChoices[UnityEngine.Random.Range(0, weaponChoices.Length - 1)];
        var instWeapon = Instantiate(assignedWeapon, WeaponGrip.position, WeaponGrip.rotation);
        if (assignedWeapon == null || WeaponGrip == null) return;
        instWeapon.transform.parent = WeaponGrip;
        instWeapon.transform.localRotation = Quaternion.Euler(90, 0, 0);

        _weapon = instWeapon.GetComponent<PrWeapon>();
        _weapon.attacker = this.gameObject;
        _weapon.aiWeapon = true;
        _weapon.LaserSight.enabled = false;
        if (_weapon.type == PrWeapon.Wt.Melee)
        {
            _weapon.MeleeRadius = attackDistance;
            aimingDistance = attackDistance;
        }

        fireRate = _weapon.FireRate;

        if (targetTransform)
            _weapon.ainpcTarget = targetTransform;

        if (!useArmIK) return;
        if (_weapon.gameObject.transform.Find("ArmIK"))
        {
            _armIKTarget = _weapon.gameObject.transform.Find("ArmIK");

            if (!_characterIKController) return;
            _characterIKController.leftHandTarget = _armIKTarget;
            EnableArmIK(true);

        }
        else
        {
            if (_characterIKController != null)
                EnableArmIK(false);
        }
    }

    private void SetTimeToWait()
    {
        _actualTimeToWait = Random.Range(timeToWait * 0.75f, -timeToWait * 0.75f) + timeToWait;
    }

    public virtual void Update()
    {
        if (!_stopPlaying)
        {
            UpdateRenderers();
            TimeCheck();
            SetState();
            ApplyState();
            ApplyRotation();
            ApplyTemperature();
        }

        if (home == null)
            ChooseHome();
    }

    private void TimeCheck()
    {
        if (DayAndNightControl.Instance.currentTime is < 0.5f and > 0.3f && DayAndNightControl.Instance.dayCall)
        {
            GoToWork();
            DayAndNightControl.Instance.dayCall = false;
        }
        if (DayAndNightControl.Instance.currentTime > 0.7f && DayAndNightControl.Instance.nightCall)
        {
            GoHome();
            DayAndNightControl.Instance.nightCall = false;
        }
    }
    
    private void ChooseHome()
    {
       // Sort by distance to me, which is probably work
        _setup.totalHomes = _setup.totalHomes.OrderBy(
            x => Vector3.Distance(this.transform.position,x.transform.position)
        ).ToList();
        
        home = _setup.totalHomes[0];
        if (home.GetComponent<BuildingCreator>())
            home.GetComponent<BuildingCreator>().residents.Add(this.gameObject);

        // If we weren't assigned a work location, we are wandering/patrolling
        if (work == null)
            currentState = type is NpcType.Citizen or NpcType.Visitor ? AIState.Wander : AIState.Patrol;
        // Just a visual aid
        if (work && home)
            distBetweenWorkAndHome = (int)Vector3.Distance(work.position, home.position);
    }
    protected virtual void ApplyTemperature()
    {
        if (!useTemperature) return;
        if (temperature is > 1.0f or < 1.0f)
        {
            if (_tempTimer < 1.0f)
                _tempTimer += Time.deltaTime;
            else
            {
                _tempTimer = 0.0f;
                ApplyTemperatureDamage();
            }
        }

        foreach (var mesh in MeshRenderers)
        {
            if (mesh.material.HasProperty("_FrozenMix"))
                mesh.material.SetFloat(FrozenMix, Mathf.Clamp(1.0f - temperature, 0.0f, 1.0f));
            if (mesh.material.HasProperty("_BurningMix"))
                mesh.material.SetFloat(BurningMix, Mathf.Clamp(temperature - 1.0f, 0.0f, 1.0f));
        }

        if (_actualFrozenVFX)
            _actualFrozenVFX.SetActive(temperature < 1.0f);
        if (_actualBurningVFX)
            _actualBurningVFX.SetActive(temperature > 1.0f);
    }

    protected virtual void ApplyTemperatureDamage()
    {
        switch (temperature)
        {
            case < 1.0f:
            case > 1.0f:
                ApplyDamagePassive(temperatureDamage);
                break;
        }
    }

    protected virtual void UpdateRenderers()
    {
        if (!_damaged || MeshRenderers.Length <= 0) return;
        _damagedTimer = Mathf.Lerp(_damagedTimer, 0.0f, Time.deltaTime * 15);

        if (Mathf.Approximately(_damagedTimer, 0.0f))
        {
            _damagedTimer = 0.0f;
            _damaged = false;
        }
            
        foreach (var mesh in MeshRenderers)
        {
            if (mesh.material.HasProperty("_DamageFX"))
                mesh.material.SetFloat(FX, _damagedTimer);
        }

        foreach (SkinnedMeshRenderer SkinnedMesh in MeshRenderers)
        {
            if (SkinnedMesh.material.HasProperty("_DamageFX"))
                SkinnedMesh.material.SetFloat(FX, _damagedTimer);
        }
    }

    protected virtual void SetState()
    {
        if (health <= 0 || dead)
        {
            currentState = AIState.Dead;
            if (!destroyDeadBody) return;
            destroyDeadBodyTimer -= Time.deltaTime;
            if (!(destroyDeadBodyTimer <= 0.0f)) return;
            if (_setup.totalNpcs.Contains(gameObject))
                _setup.totalNpcs.Remove(gameObject);
            Destroy(gameObject);
        }
        else
        {
            //Set variables
            if (eyesAndEarTransform)
                _actualSensorPos = _actualSensorTrans.position;
            else
                _actualSensorPos = transform.position + new Vector3(0.0f, 1.6f, 0.0f);

            if (faction.RelationWith(Player.Instance.playerFaction) >= 0)
            {
                GetClosestTarget();
                GetClosestFriends();
                if (_closestFriend != null)
                    _closestFriendDistance = Vector3.Distance(transform.position, _closestFriend.transform.position);
                if (targetTransform == null) return;
                CheckTargetVisibility(lookAngle);
                if (targetIsVisible) return;
                targetTransform = null;
                _targetDistance = 999.0f;
            }
            else
            {
                if (_actualAlarmedTimer > 0.0)
                    _actualAlarmedTimer -= Time.deltaTime;

                if (targetList.Count == 0 || health <= 0 || doNotAttackTarget) return;
                GetClosestTarget();
                if (targetTransform != null)
                    _targetDistance = Vector3.Distance(_actualSensorPos, targetTransform.position + new Vector3(0.0f, 1.6f, 0.0f));

                if (_actualAlarmedTimer > 0.0)
                {
                    currentState = AIState.CheckingSound;
                }
                else if (_actualAlarmedTimer <= 0.0 && _targetDistance <= awarenessDistance)
                {
                    CheckTargetVisibility(lookAngle);
                    if (targetIsVisible) return;
                    targetTransform = null;
                    _targetDistance = 999.0f;
                }
            }
        }
    }

    protected virtual void ApplyState()
    {
        // if (interactingWithPOI)
        // {
        //     standInPlace = true;
        //     //ChangeState(AIState.Talking);
        //     // print("Doing stuff");
        //     // print(interactingWithPOI.action);
        //     // if (interactingWithPOI.action == "ChangeState")
        // }
        relationWithPlayer = faction.RelationWith((Player.Instance.playerFaction));
        switch (currentState)
        {
            // case AIState.Patrol:
            //     if (_standInPlace)
            //         StopMovement();
            //     else
            //     {
            //         if (agent.remainingDistance >= 1.0f && !waiting)
            //             MoveForward(agent.remainingDistance >= 2.0f ? normalSpeed : aimingSpeed, finalGoal);
            //         else if (!waiting && _waitTimer < _actualTimeToWait)
            //             waiting = true;
            //         else
            //             ChooseNextPoi();//ChooseNextWaypoint();
            //
            //         if (waiting)
            //         {
            //             StopMovement();
            //             if (_waitTimer < _actualTimeToWait)
            //                 _waitTimer += Time.deltaTime;
            //         }
            //     }
            //     //Debug.Log("patrolling");
            //     break;
            case AIState.Patrol:
            case AIState.Wander:
                if (_standInPlace)
                    StopMovement();
                else
                {
                    if (agent.remainingDistance >= 1.0f && !waiting)
                        MoveForward(agent.remainingDistance >= 2.0f ? normalSpeed : aimingSpeed, finalGoal);
                    else if (!waiting && _waitTimer < _actualTimeToWait)
                        waiting = true;
                    else
                    {
                        //print(name+" should have arrived at (unknown POI)");
                        ChooseNextPoi();
                    }
                    if (waiting)
                    {
                        StopMovement();
                        if (_waitTimer < _actualTimeToWait)
                            _waitTimer += Time.deltaTime;
                    }
                }
                break;
            case AIState.Static:
                _standInPlace = true;
                break;
            case AIState.ChasingTarget:
                if (targetTransform)
                {
                    if (_standInPlace)
                        StopMovement();
                    else
                        MoveForward(chasingSpeed, targetTransform.position);
                }
                break;
            case AIState.AimingTarget:
                if (targetTransform)
                {
                    if (_standInPlace)
                        StopMovement();
                    else
                        MoveForward(aimingSpeed, targetTransform.position);

                    LookToTarget(targetTransform.position);
                    AttackTarget();
                }
                break;
            case AIState.Attacking:
                if (targetTransform)
                {
                    LookToTarget(targetTransform.position);
                    AttackTarget();
                    StopMovement();
                }
                else
                {
                    _targetDistance = 999.0f;
                    currentState = AIState.ChasingTarget;
                }
                break;
            case AIState.Dead:
                GetComponent<Rigidbody>().isKinematic = true;
                break;
            case AIState.CheckingSound:
                if (_standInPlace)
                    StopMovement();
                else if (Vector3.Distance(transform.position, _lastNoisePos) >= 2.0f)
                    MoveForward(normalSpeed, _lastNoisePos);
                else
                    StopMovement();

                CheckTargetVisibility(lookAngle);
                break;
            case AIState.FriendlyFollow:
                if (_animator.GetBool(Aiming))
                    _animator.SetBool(Aiming, false);
                if (_standInPlace)
                    StopMovement();
                else if (_closestFriendDistance >= 4.0f && _closestFriend != null)
                    MoveForward(normalSpeed, _closestFriend.transform.position);
                else
                    StopMovement();
                
                CheckTargetVisibility(lookAngle);
                break;
            case AIState.Idle:
                StopMovement();
                OnIdle();
                break;
            case AIState.GoingToWork:
                if (_standInPlace)
                    StopMovement();
                else if (Vector3.Distance(transform.position, work.position) >= 2.0f)
                    MoveForward(normalSpeed, work.position);
                else
                {
                    StopMovement();
                    ChangeState(AIState.Working);
                    GetComponent<BasicInteractable>().enabled = true;
                }
                break;
            case AIState.GoingHome:
                GetComponent<BasicInteractable>().enabled = false;
                if (_standInPlace)
                    StopMovement();
                else if (Vector3.Distance(transform.position, home.position) >= 2.0f)
                    MoveForward(normalSpeed, home.position);
                else
                {
                    StopMovement();
                    ChangeState(AIState.Idle);
                }
                break;
            case AIState.Working:
                OnIdle();
                break;
            case AIState.InteractingWithPlayer:
                StopMovement();
                break;
            case AIState.Talking:
                StopMovement();
                break;
            case AIState.Scared:
                break;
        }
    }

    protected virtual void ApplyRotation()
    {
        // transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y , 0);
        // if (agent != null)
        // {
        //     if (useTemperature && temperature >= 0.05f)
        //         agent.updateRotation = true;
        //     else if (useTemperature && temperature < 0.05f)
        //         agent.updateRotation = false;
        //     else if (!useTemperature)
        //         agent.updateRotation = true;
        // }
    }

    private void AttackTarget()
    {
        var targetDir = (targetTransform.position + new Vector3(0f, 1.6f, 0f)) - (_actualSensorPos);

        var angle = Vector3.Angle(targetDir, transform.forward);
        if (!(angle < attackAngle)) return;
        _animator.ResetTrigger(Alert);
        _animator.SetTrigger(CancelAlert);

        if (!_weapon || doNotAttackTarget) return;
        if (!(Time.time >= (_lastFireTimer + fireRate))) return;
        _lastFireTimer = Time.time;
        //Attack Melee
        if (_weapon.type == PrWeapon.Wt.Melee)
            UseMeleeWeapon();
        //Attack Ranged 
        else
            ShootWeapon();
    }

    private void ShootWeapon()
    {
        if (!_canAttack) return;
        if (targetTransform)
            _weapon.ainpcTarget = targetTransform;

        _weapon.Shoot();
        if (_weapon.reloading) return;
        if (_weapon.actualBullets > 0)
            _animator.SetTrigger(Shoot);
        else
            _weapon.Reload();
    }

    private void UseMeleeWeapon()
    {
        if (!_canAttack) return;
        if (targetTransform)
            _weapon.ainpcTarget = targetTransform;

        _animator.SetTrigger(MeleeAttack);

        if (chooseRandomMeleeAttack)
            _animator.SetInteger(MeleeType, Random.Range(0, meleeAttacksOptions));
        else
        {
            _animator.SetInteger(MeleeType, _actualMeleeAttack);
            if (_actualMeleeAttack < meleeAttacksOptions - 1)
                _actualMeleeAttack += 1;
            else
                _actualMeleeAttack = 0;
        }
    }
    
    // Animation callback
    private void MeleeEvent()
    {
        _weapon.AIAttackMelee(targetTransform.position, targetTransform.gameObject);
    }

    private void SetCanAttack(bool set)
    {
        _canAttack = set;
    }

    private void CheckPlayerNoise(Vector3 noisePos)
    {
        if (currentState == AIState.Dead) return;
        var noisePath = agent.path;
        if (!doNotAttackTarget && !dead && faction.RelationWith(Player.Instance.playerFaction) < 0)
        {
            var currentGoal = agent.destination;
            SetDestination(noisePos);
            
            if (agent.remainingDistance != 0 && agent.CalculatePath(noisePos, noisePath))
            {
                if (_newtAlarm == 0.0f || Time.time >= _newtAlarm + 15f)
                {
                    if (currentState == AIState.Patrol)
                    {
                        if (_animator)
                            _animator.SetTrigger(Alert);
                        _lastNoisePos = noisePos;
                        _newtAlarm = Time.time;
                        _actualAlarmedTimer = AlarmedTimer;
                        agent.SetDestination(noisePos);
                        //Debug.Log(gameObject.name + " New Noise Position assigned. Position: " + lastNoisePos);
                    }
                }
                else
                {
                    _lastNoisePos = noisePos;
                    _newtAlarm = Time.time;
                    _actualAlarmedTimer = AlarmedTimer;
                    agent.SetDestination(noisePos);
                    //Debug.Log(gameObject.name + " New Noise Position assigned");
                }
            }
           
            else
            {
                agent.SetDestination(currentGoal);
                //Debug.Log(gameObject.name + " Can´t Reach Noise");
            }
        }
    }

    private void VisibilityRay(Vector3 targetDir)
    {
        if (!Physics.Raycast(_actualSensorPos, targetDir, out var hit, awarenessDistance, visibleLayers)) return;
        if (!hit.collider.CompareTag("Player") && !hit.collider.CompareTag("NPC"))
        {
            targetIsVisible = false;
            currentVisible = new List<GameObject>();
        }
        else if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("NPC"))
        {
            if(!currentVisible.Contains(hit.collider.gameObject)) currentVisible.Add(hit.collider.gameObject);
            //Debug.Log("Seeing Player " + player.transform.position);
            targetIsVisible = true;
            _actualAlarmedTimer = 0.0f;
            _newtAlarm = 0.0f;
        }
    }

    private void CheckTargetVisibility(float actualLookAngle)
    {
        if (targetTransform == null) return;
        // IsDead()
        if (targetTransform.GetComponent<PrNPCAI>())
            if (targetTransform.GetComponent<PrNPCAI>().dead)
                return;
        if (targetTransform.GetComponent<PrTopDownCharController>())
            if (targetTransform.GetComponent<PrTopDownCharController>().m_isDead)
                return;
        // IsDead()
        var targetDir = (targetTransform.position + new Vector3(0f, 1.6f, 0f)) - (_actualSensorPos);
        var angle = Vector3.Angle(targetDir, transform.forward);
        
        if (angle < actualLookAngle && !doNotAttackTarget && !dead)
        {
            if (Time.time >= _targetLastTimeSeen)
            {
                VisibilityRay(targetDir);
                _targetLastTimeSeen = Time.time + 0.1f;
            }

            if (!targetIsVisible) return;
            _targetDistance = Vector3.Distance(_actualSensorPos, targetTransform.position + new Vector3(0.0f, 1.6f, 0.0f));

            if (_targetDistance > aimingDistance)
            {
                if (faction.RelationWith(Player.Instance.playerFaction) < 0)
                {
                    currentState = AIState.ChasingTarget;
                    _animator.SetBool(Aiming, false);
                }
            }
            else if (_targetDistance <= aimingDistance)
            {
                if (_targetDistance <= attackDistance)
                {
                    if (faction.RelationWith(Player.Instance.playerFaction) < 0)
                    {
                        if (targetTransform && targetTransform.GetComponent<PrTopDownCharInventory>())
                        {
                            if (targetTransform.GetComponent<PrTopDownCharInventory>().isDead == false)
                                currentState = AIState.Attacking;
                            else
                                BuildMobLists();

                        }
                        else if (targetTransform && targetTransform.GetComponent<PrNPCAI>())
                        {
                            if (targetTransform.GetComponent<PrNPCAI>().dead == false)
                                currentState = AIState.Attacking;
                            else
                                BuildMobLists();
                        }
                    }
                    else
                    {
                        if (targetTransform)
                            currentState = AIState.Attacking;
                        else
                            BuildMobLists();
                    }
                }
                else
                {
                    if (faction.RelationWith(Player.Instance.playerFaction) < 0)
                    {
                        if (targetTransform && targetTransform.GetComponent<PrTopDownCharInventory>())
                        {
                            if (targetTransform.GetComponent<PrTopDownCharInventory>().isDead == false)
                                currentState = AIState.AimingTarget;
                            else
                                BuildMobLists();
                        }
                        else if (targetTransform && targetTransform.GetComponent<PrNPCAI>())
                        {
                            if (targetTransform.GetComponent<PrNPCAI>().dead == false)
                                currentState = AIState.AimingTarget;
                            else
                                BuildMobLists();
                        }
                    }
                    else
                    {
                        if (targetTransform && targetTransform.GetComponent<PrNPCAI>().dead == false)
                            currentState = AIState.AimingTarget;
                        else
                            BuildMobLists();
                    }
                }
                _animator.SetBool(Aiming, true);
            }
        }
        else if (_actualAlarmedTimer > 0.0f)
        {
            currentState = AIState.CheckingSound;
            _animator.SetBool(Aiming, false);
        }
        else
        {
            if (_animator)
                _animator.SetBool(Aiming, false);
        }
    }
    
    // void FootStep()
    // {
    //     if (Footsteps.Length > 0 && Time.time >= (LastFootStepTime + FootStepsRate))
    //     {
    //         int FootStepAudio = 0;
    //
    //         if (Footsteps.Length > 1)
    //         {
    //             FootStepAudio = Random.Range(0, Footsteps.Length);
    //         }
    //
    //         float FootStepVolume = animator.GetFloat("Speed") * generalFootStepsVolume;
    //         if (aiming)
    //             FootStepVolume *= 0.5f;
    //
    //         Audio.PlayOneShot(Footsteps[FootStepAudio], FootStepVolume);
    //
    //         LastFootStepTime = Time.time;
    //     }
    // }

    private void StopMovement()
    {
        if (agent == null) return;
        agent.velocity = Vector3.zero;
        agent.speed = 0;
        _animator.SetFloat(Speed, 0.0f);
    }

    public void FootStep()
    {}
    
    private void ChooseNextPoi()
    {
        var which = Random.Range(0, _setup.POIs.Count);
        var dest = _setup.POIs[which].transform.position;
        _nextPoi = _setup.POIs[which];
        waiting = false;
        _waitTimer = 0.0f;
        SetTimeToWait();
        finalGoal = dest;
        SetDestination(dest);
    }
    
    // private void ChooseNextWaypoint()
    // {
    //     waiting = false;
    //     _prevNode = _currentNode;
    //     _currentNode = FindClosestWaypoint(transform.position);
    //     if (_currentNode.neighbors.Count > 0)
    //         // check destnode isn't previous node, etc
    //         _destNode = _currentNode.neighbors[Random.Range(0, _currentNode.neighbors.Count)];
    //     else
    //     {
    //         // print("Waypoint without any neighbors!");
    //         // print(name + " is lost. killing them.");
    //         if (_setup.totalNpcs.Contains(gameObject))
    //             _setup.totalNpcs.Remove(gameObject);
    //         Destroy(gameObject);
    //         return;
    //     }
    //     
    //     _waitTimer = 0.0f;
    //     SetTimeToWait();
    //     finalGoal = _destNode.transform.position;
    //     SetDestination(_destNode.transform.position);
    // }

    private void SetDestination(Vector3 pos)
    {
        if (agent.CalculatePath(pos, _path))
            agent.path = _path;
        agent.SetDestination(pos);
    }

    private void MoveForward(float speed, Vector3 goal)
    {
        if (useRootmotion)
        {
            agent.destination = goal;
            agent.speed = speed;
            _animator.SetFloat(Speed, speed);
            if (!useTemperature) return;
            if (temperature <= 1.0f)
                _animator.SetFloat(Temperature, Mathf.Clamp(temperature, 0.0f, onFireSpeedFactor));
            print("Moving?");
        }
        else
        {
            //Debug.Log("Moving Forward");
            agent.destination = goal;
            if (useTemperature)
            {
                if (temperature <= 1.0f)
                {
                    agent.speed = (speed + 0.8f) * Mathf.Clamp(temperature, 0.0f, 1.0f);
                    _animator.SetFloat(Speed, speed * Mathf.Clamp(temperature, 0.5f, 1.0f));
                }
                _animator.SetFloat(Temperature, Mathf.Clamp(temperature, 0.0f, 1.0f));
            }
            else
            {
                agent.speed = (speed + 0.8f);
                _animator.SetFloat(Speed, speed, 0.25f, Time.deltaTime);
            }
        }
    }

    private void LookToTarget(Vector3 target)
    {
        //if (movement != Vector3.Zero) 
        var targetRot = Quaternion.LookRotation(target - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Mathf.Clamp(temperature, 0.0f,1.0f));
    }

    public void OnTriggerEnter(Collider other)
    {
        if (currentState == AIState.Dead) return;
        if (other.CompareTag("Noise"))
        {
            CheckPlayerNoise(other.transform.position);
        }
        else if ((other.CompareTag("NPC") && other.transform != this.transform) || other.CompareTag("Player"))
        {
            var head = other.GetComponent<PrTopDownCharInventory>() ? other.GetComponent<PrTopDownCharInventory>().headObj : 
                other.GetComponent<PrNPCAI>().eyesAndEarTransform;
            _characterIKController.ikHeadActive = true;
            _characterIKController.lookObj = head;
            // Do some speech bubble thing
            //_dialogueSystem.BarkString("Test speech", this.transform);
            if (_barkTimer > _barkLimiter)
            {
                if (currentState != AIState.Attacking && currentState != AIState.ChasingTarget && currentState != AIState.AimingTarget &&
                    type != NpcType.Worker && type != NpcType.Merchant && type != NpcType.Soldier)
                    _dialogueSystem.Bark("GeneralGreeting", this.transform);
                _barkTimer = 0;
            }
            else _barkTimer += Time.deltaTime;
        }
    }

    public void LateUpdate()
    {
        if (currentState == AIState.Dead) return;
        if (_lockRotation)
        {
            transform.rotation = Quaternion.Euler(_lockedRotDir);
        }
    }

    // Called by Animator
    private void EndMelee()
    {
    }

    // private Waypoint FindClosestPOI(Vector3 target)
    // {
    //     GameObject closest = null;
    //     var closestDist = Mathf.Infinity;
    //     foreach (var waypoint in waypointRoute)
    //     {
    //         var dist = (waypoint.transform.position - target).magnitude;
    //         if (!(dist < closestDist)) continue;
    //         closest = waypoint;
    //         closestDist = dist;
    //     }
    //
    //     return closest != null ? closest.GetComponent<Waypoint>() : null;
    // }

    public void ChangeState(AIState newState)
    {
        if (currentState == newState)
            return;

        var prevState = currentState;
        currentState = newState;
    }

    private void OnScared()
    {
        print("Scared mob called. Do something cool!");
        //gameObject.AddComponent(typeof(RunAway));
        //StartCoroutine(GetComponent<RunAway>().Run(Attacker));
    }

    private void OnIdle()
    {
        if (type is not (NpcType.Citizen or NpcType.Merchant)) return;
        
        var time = _dayAndNightControl.currentTime;
        if (time is > .3f and < .7f)
            GoToWork();
        else
            GoHome();
    }

    private float AddFactionPenalty(Faction otherfaction)
    {
        var rep = faction.RelationWith(otherfaction);
        rep = Mathf.Clamp(rep - factionPenalty, -1, 1);
        if (_factionRelationsBackup != null)
            _factionRelationsBackup[otherfaction] = Mathf.Clamp(_factionRelationsBackup[otherfaction] - factionPenalty, -1, 1);

        foreach (var currFac in ObjectFactory.Instance.Factions)
        {
            if (currFac == faction || currFac == otherfaction) continue;
            var relationWithOtherFaction = otherfaction.RelationWith(currFac);
            var repChange = (factionPenalty * -relationWithOtherFaction) / dampeningFactor;
            currFac.Cache[faction] = Mathf.Clamp(currFac.RelationWith(faction) + repChange, -1, 1);
            faction.Cache[currFac] = Mathf.Clamp(faction.RelationWith(currFac) + repChange, -1, 1);
            if(_factionRelationsBackup != null)
                _factionRelationsBackup[currFac] = Mathf.Clamp(_factionRelationsBackup[currFac] + repChange, -1, 1);
        }

        //DisplayFactionRelations();
        return rep;
    }
    public virtual void OnDrawGizmos()
    {
        // Gizmos.color = Color.white;
        // if (playerTransform && _playerIsVisible)
        // {
        //     if (eyesAndEarTransform)
        //         Gizmos.DrawLine(playerTransform.position + new Vector3(0, eyesAndEarTransform.position.y, 0), eyesAndEarTransform.position);
        //     else
        //         Gizmos.DrawLine(playerTransform.position + new Vector3(0f, 1.6f, 0f), transform.position + new Vector3(0f, 1.6f, 0f));
        // }
        //
        // Quaternion lRayRot = Quaternion.AngleAxis(-lookAngle * 0.5f, Vector3.up);
        // Quaternion rRayRot = Quaternion.AngleAxis(lookAngle * 0.5f, Vector3.up);
        // Vector3 lRayDir = lRayRot * transform.forward;
        // Vector3 rRayDir = rRayRot * transform.forward;
        // if (eyesAndEarTransform)
        // {
        //     Gizmos.DrawRay(eyesAndEarTransform.position, lRayDir * awarenessDistance);
        //     Gizmos.DrawRay(eyesAndEarTransform.position, rRayDir * awarenessDistance);
        // }
        // else
        // {
        //     Gizmos.DrawRay(transform.position + new Vector3(0f, 1.6f, 0f), lRayDir * awarenessDistance);
        //     Gizmos.DrawRay(transform.position + new Vector3(0f, 1.6f, 0f), rRayDir * awarenessDistance);
        // }
        //
        // Gizmos.DrawMesh(areaMesh, transform.position, Quaternion.identity, Vector3.one * awarenessDistance);
        //
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawMesh(areaMesh, transform.position, Quaternion.identity, Vector3.one * aimingDistance);
        // Gizmos.DrawSphere(_lastNoisePos, 1.0f);
        //
        // Gizmos.color = Color.red;
        // Gizmos.DrawMesh(areaMesh, transform.position, Quaternion.identity, Vector3.one * attackDistance);
        //
        // Gizmos.color = Color.blue;
        // Gizmos.DrawMesh(areaMesh, transform.position, Quaternion.identity, Vector3.one * hearingDistance);

        // if (useRagdollDeath && dead)
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawWireSphere(_lastHitPos + new Vector3(0f, 1.6f, 0f), 0.1f);
        //     Gizmos.DrawRay(_lastHitPos + new Vector3(0f, 1.6f, 0f),  _ragdollForce);
        // }
    }
}

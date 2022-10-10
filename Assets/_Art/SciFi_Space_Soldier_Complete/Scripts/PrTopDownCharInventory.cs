using System.Collections.Generic;
using UnityEngine;
using DunGen.DungeonCrawler;
using TMPro;
using UnityEngine.AI;

public class PrTopDownCharInventory : MonoBehaviour {

    [Header("Visuals")]

    public string characterName = "Soldier";

    [Header("Stats")]

    public int health = 100;
    public int actualHealth = 100;
    public bool oneShotHealth = false;
    public float stamina = 1.0f;
    public float staminaRecoverSpeed = 0.5f;
    [HideInInspector] public float actualStamina = 1.0f;
    public float staminaRecoverLimit = 0.5f;
    private float _actualStaminaRecover = 0.5f;

    [HideInInspector] public bool usingStamina = false;
    [HideInInspector] public bool isDead = false;
    public bool destroyOnDead = false;

    private bool _damaged = false;
    private float _damagedTimer = 0.0f;
    
    private SphereCollider _noiseTrigger;
    [HideInInspector]
    public float actualNoise = 0.0f;
    private readonly float _noiseDecaySpeed = 10.0f;

    [Header("Temperature Settings")]
    public bool useTemperature = true;
    [HideInInspector]
    public float temperature = 1.0f;
    public float temperatureThreshold = 2.0f;
    public int temperatureDamage = 5;
    public float onFireSpeedFactor = 0.5f;
    private float tempTimer = 0;

    [Header("Weapons")]
    public int playerWeaponLimit = 2;
    public PrWeapon[] InitialWeapons;
    private float _lastWeaponChange;
    [HideInInspector]
    public bool armed = true;
    //[HideInInspector]
    public GameObject[] carriedWeapons;
    //[HideInInspector]
    public int[] actualWeaponTypes;
    //[HideInInspector]
    public int activeWeapon = 0;
    private bool _canShoot = true;
    public PrWeaponList WeaponListObject;
    private GameObject[] _weaponList;
    public Transform WeaponR;
    public Transform WeaponL;
    public bool aimingIK = false;
    public bool useArmIK = true;
    public Transform headObj;

    //Grenade Vars
    [Header("Grenades Vars")]
    public float throwGrenadeMaxForce = 150f;
    public GameObject grenadesPrefab;
    public int maxGrenades = 10;
    public int grenadesCount = 5;
    private bool _isThrowing = false;

    [HideInInspector] public bool aiming;

    private float _fireRateTimer;
	private float _lastFireTimer;
    private Transform _aimTarget;

    [Header("VFX")]
    public Renderer[] MeshRenderers;
    public GameObject DamageFX;
    public Transform BurnAndFrozenVFXParent;
    public GameObject frozenVFX;
    public GameObject burningVFX;
    public GameObject damageSplatVFX;
    public GameObject explosiveDeathVFX;
    public GameObject deathVFX;
    public bool useExplosiveDeath = true;
    public int damageThreshold = 50;
    
    private PrBloodSplatter _actualSplatVFX;
    private GameObject _actualFrozenVFX;
    private GameObject _actualBurningVFX;
    private GameObject _actualDeathVFX;
    private Vector3 _lastHitPos = Vector3.zero;
    private bool _explosiveDeath = false;
    private GameObject _actualExplosiveDeathVFX;

    //Ragdoll Vars
    [Header("Ragdoll setup")]
    public bool useRagdollDeath = false;
    public float ragdollForceFactor = 1.0f; 

    [Header("Sound FX")]
    
    public float footStepsRate = 0.4f;
    public float generalFootStepsVolume = 1.0f;
    public AudioClip[] footsteps;
    private float _lastFootStepTime = 0.0f;
    private AudioSource _audio;

    //Use Vars
    [Header("Use Vars")]
    public float useAngle = 75.0f;
    [HideInInspector] public GameObject usableObject;
    [HideInInspector] public bool usingObject = false;
    
    //Pickup Vars
    private GameObject _pickupObj;

    //HuD Vars
    [Header("HUD")]
    public GameObject Compass;
    public TextMesh CompassDistance;
    private bool _compassActive = false;
    private Transform _compassTarget;
    public GameObject HUDUseHelper;
    private AvatarMenuController _menuController;
    [HideInInspector]
    public PrTopDownCharController charController;
    [HideInInspector]
    public Animator charAnimator;
    private GameObject[] _canvases;
    
    //ArmIK
    private Transform _armIKTarget;
    private PrCharacterIK _characterIKController;
    
    private static readonly int G = Animator.StringToHash("ThrowG");
    private static readonly int AttackMelee = Animator.StringToHash("AttackMelee");
    private static readonly int MeleeType = Animator.StringToHash("MeleeType");
    private static readonly int Shoot = Animator.StringToHash("Shoot");
    private static readonly int RunStop = Animator.StringToHash("RunStop");
    private static readonly int Aiming = Animator.StringToHash("Aiming");
    private static readonly int Armed1 = Animator.StringToHash("Armed");
    private static readonly int Turning = Animator.StringToHash("Turning");
    private static readonly int Pickup = Animator.StringToHash("Pickup");
    private static readonly int Use = Animator.StringToHash("StopUse");
    private static readonly int FrozenMix = Shader.PropertyToID("_FrozenMix");
    private static readonly int BurningMix = Shader.PropertyToID("_BurningMix");
    private static readonly int Dead = Animator.StringToHash("Dead");
    private static readonly int FX = Shader.PropertyToID("_DamageFX");
    private static readonly int Reloading = Animator.StringToHash("Reloading");
    private static readonly int Speed = Animator.StringToHash("Speed");

    public void Start() {
        //Creates weapon array
        carriedWeapons = new GameObject[playerWeaponLimit];
        actualWeaponTypes = new int[playerWeaponLimit];

        //Load Weapon List from Scriptable Object
        _weaponList = WeaponListObject.weapons;

        actualHealth = health;
        actualStamina = stamina;
        _actualStaminaRecover = staminaRecoverLimit;

        _audio = GetComponent<AudioSource>() as AudioSource;
        charAnimator = GetComponent<Animator>();
        _menuController = AvatarMenuController.Instance;
        if (_menuController == null)
            Debug.LogError("Missing controller");
        
        DeactivateCompass();
       
        charController = GetComponent<PrTopDownCharController>();
        _aimTarget = charController.AimFinalPos;
        _menuController.HUDHealthBar.GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);

        CreateNoiseTrigger();

        // //Start PlayerInfo to Load and Save player Info across levels
        // if (GameObject.Find("playerInfo_" + charController.playerNmb) && charController.playerSettings.TypeSelected == PrPlayerSettings.GameMode.SinglePlayer)
        // {
        //     Debug.Log("Player Info already Found");
        //     charController.LoadPlayerInfo();
            SetHealth(actualHealth);
        // }

        //Weapon Instantiate and initialization
        if (InitialWeapons.Length > 0)
        {
            InstantiateWeapons();
            
            armed = true;
            ChangeToWeapon(0);
        }
        else
        {
            armed = false;
        }

        _canvases = GameObject.FindGameObjectsWithTag("Canvas");
        if (_canvases.Length > 0)
        {
            foreach (var c in _canvases)
                UnparentTransforms(c.transform);
        }

        InitializeHUD();

        //ragdoll Initialization
        gameObject.AddComponent<PrCharacterRagdoll>();
        
        if (useExplosiveDeath && explosiveDeathVFX)
        {
            _actualExplosiveDeathVFX = Instantiate(explosiveDeathVFX, transform.position, transform.rotation);
            _actualExplosiveDeathVFX.SetActive(false);

            if (GameObject.Find("VFXBloodParent"))
                _actualExplosiveDeathVFX.transform.parent = GameObject.Find("VFXBloodParent").transform;
            else
            {
                var vfxParent = new GameObject("VFXBloodParent");
                _actualExplosiveDeathVFX.transform.parent = vfxParent.transform;
            }
        }

        if (deathVFX)
        {
            _actualDeathVFX = Instantiate(deathVFX, transform.position, transform.rotation);
            _actualDeathVFX.SetActive(false);

            if (GameObject.Find("VFXBloodParent"))
                _actualDeathVFX.transform.parent = GameObject.Find("VFXBloodParent").transform;
            else
            {
                var vfxParent = new GameObject("VFXBloodParent");
                vfxParent.transform.SetParent(ParticlePool.Instance.transform);
                _actualDeathVFX.transform.parent = vfxParent.transform;
            }
        }

        if (damageSplatVFX)
        {
            var gOactualSplatVFX = Instantiate(damageSplatVFX, transform.position, transform.rotation);
            gOactualSplatVFX.transform.position = transform.position;
            gOactualSplatVFX.transform.parent = ParticlePool.Instance.transform;
            _actualSplatVFX = gOactualSplatVFX.GetComponent<PrBloodSplatter>();
        }
        if (frozenVFX)
        {
            _actualFrozenVFX = Instantiate(frozenVFX, transform.position, transform.rotation);
            _actualFrozenVFX.transform.position = transform.position;
            _actualFrozenVFX.transform.parent = ParticlePool.Instance.transform;
            if (BurnAndFrozenVFXParent)
                _actualFrozenVFX.transform.parent = BurnAndFrozenVFXParent;
        }
        if (burningVFX)
        {
            _actualBurningVFX = Instantiate(burningVFX, transform.position, transform.rotation);
            _actualBurningVFX.transform.position = transform.position;
            _actualBurningVFX.transform.parent = ParticlePool.Instance.transform;
            if (BurnAndFrozenVFXParent)
                _actualBurningVFX.transform.parent = BurnAndFrozenVFXParent;
        }
        if (_menuController.useQuickReload && _menuController.quickReloadPanel && 
            _menuController.quickReloadMarker && _menuController.quickReloadZone)
        {
            QuickReloadActive(false);
        }

        //Update grenades HUD
        if (_menuController.HUDGrenadesCount)
            _menuController.HUDGrenadesCount.GetComponent<TMP_Text>().text = grenadesCount.ToString();
    }

    private static void UnparentTransforms(Transform target)
    {
        target.SetParent(null);
    }

    private void CreateNoiseTrigger()
    {
        var noiseGO = new GameObject
        {
            name = "Player Noise Trigger"
        };
        noiseGO.AddComponent<SphereCollider>();
        _noiseTrigger = noiseGO.GetComponent<SphereCollider>();
        _noiseTrigger.GetComponent<SphereCollider>().isTrigger = true;
        _noiseTrigger.transform.parent = this.transform;
        _noiseTrigger.transform.position = transform.position + new Vector3(0,1,0);
        _noiseTrigger.gameObject.tag = "Noise";
    }

    private void InstantiateWeapons()
    {
        var weapType = 0;
        foreach (PrWeapon weapon in InitialWeapons)
        {
            var weapInt = 0;
            //Debug.Log("Weapon to instance = " + Weap);
            foreach (var weap in _weaponList)
            {
                if (weapon.gameObject.name == weap.name)
                {
                    //Debug.Log("Weapon to pickup = " + weap + " " + weapInt);
                    actualWeaponTypes[weapType] = weapInt;
                    PickupWeapon(weapInt);
                }
                else
                    weapInt += 1;
            }
            weapType += 1;
        }
    }

    // Used by some anims?
    public void FootStep()
    {
        if (footsteps.Length <= 0 || !(Time.time >= (_lastFootStepTime + footStepsRate))) return;
        var footStepAudio = 0;
        if (footsteps.Length > 1)
        {
            footStepAudio = Random.Range(0, footsteps.Length);
        }
        var footStepVolume = charAnimator.GetFloat(Speed) * generalFootStepsVolume;
        if (aiming)
            footStepVolume *= 0.5f;
        _audio.PlayOneShot(footsteps[footStepAudio], footStepVolume);
        MakeNoise(footStepVolume * 10f);
        _lastFootStepTime = Time.time;
    }
    

    public void ActivateCompass(GameObject target)
    {
        Debug.Log("Compass activated " + target.name);

        Compass.SetActive(true);
        _compassActive = true;
        _compassTarget = target.transform;
    }

    private void DeactivateCompass()
    {
        _compassActive = false;
        Compass.SetActive(false);
    }

    private void MakeNoise(float volume)
    {
        actualNoise = volume;
    }

    public void LoadGrenades(int quantity)
    {
        grenadesCount += quantity;
        if (grenadesCount > maxGrenades)
        {
            grenadesCount = maxGrenades;
        }
        if (_menuController.HUDGrenadesCount)
            _menuController.HUDGrenadesCount.GetComponent<TMP_Text>().text = grenadesCount.ToString();
    }
    //Called by anim event
    public void ThrowG()
    {
        var grenade = Instantiate(grenadesPrefab, WeaponL.position, Quaternion.LookRotation(transform.forward)) as GameObject;
        //Grenade.GetComponent<PrBullet>().team = team;
        var grenadeForce = this.transform.forward * grenade.GetComponent<PrBullet>().bulletSpeed * 25 + Vector3.up * 2000;
        var targetDistance = Vector3.Distance(_aimTarget.transform.position, transform.position);
        var finalGrenadeForce = grenadeForce * (targetDistance / 20.0f);
        var maxForce = throwGrenadeMaxForce * 17;
        finalGrenadeForce.x = Mathf.Clamp(finalGrenadeForce.x, -maxForce, maxForce);
        finalGrenadeForce.y = Mathf.Clamp(finalGrenadeForce.y, -maxForce, maxForce);
        finalGrenadeForce.z = Mathf.Clamp(finalGrenadeForce.z, -maxForce, maxForce);
        //Debug.Log(finalGrenadeForce);
        grenade.GetComponent<Rigidbody>().AddForce(finalGrenadeForce);
        grenade.GetComponent<Rigidbody>().AddRelativeTorque(grenade.transform.forward * 50f, ForceMode.Impulse);

        grenadesCount -= 1;
        if (_menuController.HUDGrenadesCount)
            _menuController.HUDGrenadesCount.GetComponent<TMP_Text>().text = grenadesCount.ToString();
    }
    //Called by anim event
    public void EndThrow()
    {
        _isThrowing = false;
        EnableArmIK(true);
    }
    private void EnableArmIK(bool active)
    {
        if (!_characterIKController || !useArmIK) return;
        _characterIKController.ikHandsActive = carriedWeapons[activeWeapon].GetComponent<PrWeapon>().useIK && active;
    }

    private void QuickReloadActive(bool state)
    {
        //Debug.Log("QuickReloading " + state );
        _menuController.quickReloadPanel.SetActive(state);
    }

    // Called from anim
    public void EndMelee()
    {
        charController.useRootMotion = false;
    }
    // Called from anim
    public void MeleeEvent()
    {
        carriedWeapons[activeWeapon].GetComponent<PrWeapon>().AttackMelee();
       
    }

    public void Update () {

        if (Player.Instance.inputDisabled)
            return;
        
        if (_damaged && MeshRenderers.Length > 0)
        {
            _damagedTimer = Mathf.Lerp(_damagedTimer, 0.0f, Time.deltaTime * 10);

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

            if (_menuController.HUDDamageFullScreen)
               _menuController.HUDDamageFullScreen.GetComponent<UnityEngine.UI.Image>().color = new Vector4(1, 1, 1, _damagedTimer * 0.5f);
        }

        if (isDead) return;
        //Calculates Temperature and damage
        ApplyTemperature();
        
        // Change Weapon, mouse wheel
        if(Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            ChangeWeapon();
        }

        // Throw grenades
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (grenadesCount > 0 && charController.Sprinting == false && armed)
            {
                carriedWeapons[activeWeapon].GetComponent<PrWeapon>().CancelReload();
                charAnimator.SetTrigger(G);
                carriedWeapons[activeWeapon].GetComponent<PrWeapon>().LaserSight.enabled = false;
                EnableArmIK(false);
            }
        }
            
        // Shoot Weapons
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            if (charController.Sprinting == false && _isThrowing == false)
            {
                if (_canShoot && carriedWeapons[activeWeapon] != null && Time.time >= (_lastFireTimer + _fireRateTimer))
                {
                    //Melee Weapon
                    if (carriedWeapons[activeWeapon].GetComponent<PrWeapon>().type == PrWeapon.Wt.Melee) 
                    {
                        _lastFireTimer = Time.time;
                        charAnimator.SetTrigger(AttackMelee);
                        if (carriedWeapons[activeWeapon].GetComponent<PrWeapon>().name == "Player_Melee") // Sword
                            charAnimator.SetInteger(MeleeType, Random.Range(10, 14));
                        else if (carriedWeapons[activeWeapon].GetComponent<PrWeapon>().name == "Player_None") // Punch
                            charAnimator.SetInteger(MeleeType, Random.Range(0, 2));
                        charController.useRootMotion = true;
                        //charController.CantRotate();
                        //print("Attack "+carriedWeapons[ActiveWeapon].GetComponent<PrWeapon>().name);
                    }
                    //Ranged Weapon
                    else 
                    {
                        if (aiming)
                        {
                            _lastFireTimer = Time.time;
                            carriedWeapons[activeWeapon].GetComponent<PrWeapon>().Shoot();
                            if (carriedWeapons[activeWeapon].GetComponent<PrWeapon>().reloading == false)
                                charAnimator.SetTrigger(Shoot);
                        }
                    }
                }
            }
        }
        // Aim
        if (Input.GetMouseButtonDown(1))
        {
            charController.AimTargetVisual.SetActive(true);
            if (charController.Sprinting == false && !usingObject && armed && _isThrowing == false)
            {
                if (carriedWeapons[activeWeapon].GetComponent<PrWeapon>().type != PrWeapon.Wt.Melee)
                {
                    aiming = true;
                    if (carriedWeapons[activeWeapon].GetComponent<PrWeapon>().reloading == true || _isThrowing == true)
                        carriedWeapons[activeWeapon].GetComponent<PrWeapon>().LaserSight.enabled = false;
                    else
                        carriedWeapons[activeWeapon].GetComponent<PrWeapon>().LaserSight.enabled = true;
                    charAnimator.SetBool(RunStop, false);
                    charAnimator.SetBool(Aiming, true);
                }
                else
                {
                    aiming = true;
                    charAnimator.SetBool(RunStop, false);
                    charAnimator.SetBool(Aiming, true);
                }
            }
                
        }
        //Stop Aiming
        else if (!Input.GetMouseButton(1))
        {
            charController.AimTargetVisual.SetActive(false);
            aiming = false;
            charAnimator.SetBool(Aiming, false);
            carriedWeapons[activeWeapon].GetComponent<PrWeapon>().LaserSight.enabled = false;
        }
            
        if (HUDUseHelper && usableObject)
        {
            HUDUseHelper.transform.rotation = Quaternion.identity;
        }
            
        if (_actualStaminaRecover >= staminaRecoverLimit)
        {
            if (usingStamina)
            {
                if (actualStamina > 0.05f)
                    actualStamina -= Time.deltaTime;
                else if (actualStamina > 0.0f)
                {
                    actualStamina = 0.0f;
                    _actualStaminaRecover = 0.0f;
                }
            }
            else if (!usingStamina)
            {
                if (actualStamina < stamina)
                    actualStamina += Time.deltaTime * staminaRecoverSpeed;
                else
                    actualStamina = stamina;
            }
        }
        else if (_actualStaminaRecover < staminaRecoverLimit)
        {
            _actualStaminaRecover += Time.deltaTime;
        }
            
        _menuController.HUDStaminaBar.GetComponent<RectTransform>().localScale = new Vector3((1.0f / stamina) *  actualStamina, 1.0f, 1.0f);
            
        if (usingObject && usableObject)
        {
            Quaternion endRotation = Quaternion.LookRotation(usableObject.transform.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, endRotation, Time.deltaTime * 5);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        }

        //Noise Manager
        if (actualNoise > 0.0f)
        {
            actualNoise -= Time.deltaTime * _noiseDecaySpeed;
            _noiseTrigger.radius = actualNoise;
        }
    }

    void LateUpdate()
    {
        if (_compassActive && !isDead)
        {
            if (_compassTarget)
            {
                Compass.transform.LookAt(_compassTarget.position);
                CompassDistance.text = "" + (Mathf.RoundToInt(Vector3.Distance(_compassTarget.position, transform.position))) + " Mts";

                if (Vector3.Distance(_compassTarget.position, transform.position) <= 2.0f)
                {
                    _compassTarget = null;
                    DeactivateCompass();
                }
            }
            else
            {
                DeactivateCompass();
            }

        }

        if (!aimingIK || isDead) return;
        if (aiming && !carriedWeapons[activeWeapon].GetComponent<PrWeapon>().reloading && !_isThrowing && !charController.Sprinting)
        {
            WeaponR.parent.transform.LookAt(_aimTarget.position, Vector3.up);
            charAnimator.SetTrigger(Turning); 
        }
    }
    // Called by PrPickupObject
    public void TargetPickedUp()
    {
        charAnimator.SetTrigger(Pickup); 
    }

    public void StartUsingGeneric(string type)
    {
        aiming = false;
        usingObject = true;

        charController.m_CanMove = false;
        charAnimator.SetTrigger(type);

        carriedWeapons[activeWeapon].GetComponent<PrWeapon>().LaserSight.enabled = false;

        EnableArmIK(false);
    }

    public void PickupItem()
    {
        transform.rotation = Quaternion.LookRotation(_pickupObj.transform.position - transform.position);
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
            
        _pickupObj.SendMessage("PickupObjectNow", activeWeapon);
    }

    public void SpawnTeleportFX()
    {
        _damaged = true;
        _damagedTimer = 1.0f;
    }

    public void PickupWeapon(int weaponType)
    {
        //print("Picking up weapon "+WeaponType );
        var newWeapon = Instantiate(_weaponList[weaponType], WeaponR.position, WeaponR.rotation);
        newWeapon.transform.parent = WeaponR.transform;
        newWeapon.transform.localRotation = Quaternion.Euler(90, 0, 0);
        newWeapon.name = "Player_" + newWeapon.GetComponent<PrWeapon>().weaponName;
        actualWeaponTypes[activeWeapon] = weaponType;

        //New multi weapon system
        var replaceWeapon = true;

        for (int i = 0; i < playerWeaponLimit ; i++)
        {
            if (carriedWeapons[i] == null)
            {
                //Debug.Log(i + " " + newWeapon.name);

                carriedWeapons[i] = newWeapon;
                replaceWeapon = false;
                
                if (activeWeapon != i)
                    ChangeToWeapon(i);
                break;
            }

        }
        if (replaceWeapon)
        {
            //Debug.Log("Replacing weapon" + carriedWeapons[ActiveWeapon].name + " using " + NewWeapon.name);
            DestroyImmediate(carriedWeapons[activeWeapon]);
            carriedWeapons[activeWeapon] = newWeapon;
        }
        InitializeWeapons();
    }

    private void InitializeWeapons()
    {
        var actualW = carriedWeapons[activeWeapon].GetComponent<PrWeapon>();
        carriedWeapons[activeWeapon].SetActive(true);
        _menuController.HUDWeaponPicture.GetComponent<UnityEngine.UI.Image>().sprite = actualW.WeaponPicture;

        actualW.ShootTarget = _aimTarget;
        actualW.attacker = this.gameObject;
        _fireRateTimer = actualW.FireRate;

        actualW.HUDWeaponBullets = _menuController.HUDWeaponBullets;
        actualW.HUDWeaponBulletsBar = _menuController.HUDWeaponBulletsBar;
        actualW.HUDWeaponClips = _menuController.HUDWeaponClips;

        _menuController.useQuickReload = actualW.useQuickReload;

        actualW.HUDquickReloadMarker = _menuController.quickReloadMarker;
        actualW.HUDquickReloadZone = _menuController.quickReloadZone;
        actualW.SetupQuickReload();
        
        //ArmIK
        if (useArmIK)
        {
            if (actualW.gameObject.transform.Find("ArmIK"))
            {
                _armIKTarget = actualW.gameObject.transform.Find("ArmIK");
                if (GetComponent<PrCharacterIK>() == null)
                {
                    gameObject.AddComponent<PrCharacterIK>();
                    _characterIKController = GetComponent<PrCharacterIK>();
                }
                else if (GetComponent<PrCharacterIK>())
                {
                    _characterIKController = GetComponent<PrCharacterIK>();
                }

                if (_characterIKController)
                {
                    _characterIKController.leftHandTarget = _armIKTarget;
                    _characterIKController.ikHandsActive = true;
                }
            }
            else
            {
                if (_characterIKController != null)
                    _characterIKController.ikHandsActive = false;
            }
        }
        
        actualW.Audio = WeaponR.GetComponent<AudioSource>();

        if (actualW.type == PrWeapon.Wt.Pistol)
        {
            var pistolLayer = charAnimator.GetLayerIndex("PistolLyr");
            charAnimator.SetLayerWeight(pistolLayer, 1.0f);
            var pistolActLayer = charAnimator.GetLayerIndex("PistolActions");
            charAnimator.SetLayerWeight(pistolActLayer, 1.0f);
            charAnimator.SetBool(Armed1, true);
        }
        else if (actualW.type == PrWeapon.Wt.Rifle)
        {
            var pistolLayer = charAnimator.GetLayerIndex("PistolLyr");
            charAnimator.SetLayerWeight(pistolLayer, 0.0f);
            var pistolActLayer = charAnimator.GetLayerIndex("PistolActions");
            charAnimator.SetLayerWeight(pistolActLayer, 0.0f);
            charAnimator.SetBool(Armed1, true);
        }
        else if (actualW.type == PrWeapon.Wt.Melee)
        {
            var pistolLayer = charAnimator.GetLayerIndex("PistolLyr");
            charAnimator.SetLayerWeight(pistolLayer, 0.0f);
            var pistolActLayer = charAnimator.GetLayerIndex("PistolActions");
            charAnimator.SetLayerWeight(pistolActLayer, 0.0f);
            charAnimator.SetBool(Armed1, false);
        }
        else if (actualW.type == PrWeapon.Wt.Laser)
        {
            var pistolLayer = charAnimator.GetLayerIndex("PistolLyr");
            charAnimator.SetLayerWeight(pistolLayer, 0.0f);
            var pistolActLayer = charAnimator.GetLayerIndex("PistolActions");
            charAnimator.SetLayerWeight(pistolActLayer, 0.0f);
            charAnimator.SetBool(Armed1, true);
        }

        carriedWeapons[activeWeapon].GetComponent<PrWeapon>().UpdateWeaponGUI(_menuController.HUDWeaponPicture);
    }

    private void InitializeHUD()
    {
        if (_menuController.HUDDamageFullScreen)
            _menuController.HUDDamageFullScreen.GetComponent<UnityEngine.UI.Image>().color = new Vector4(1, 1, 1, 0);
        var yPos = _menuController.HUDWeaponBulletsBar.transform.parent.GetComponent<RectTransform>().localPosition.y;
        var xPos = _menuController.HUDWeaponBulletsBar.transform.parent.GetComponent<RectTransform>().localPosition.x;
        _menuController.HUDWeaponBulletsBar.transform.parent.GetComponent<RectTransform>().localPosition = new Vector3(xPos, yPos, 0);
    }

    private void ChangeToWeapon(int weaponInt)
    {
        _lastWeaponChange = Time.time;

        if (carriedWeapons[weaponInt] == null) return;
        foreach (var i in carriedWeapons)
        {
            if (i == null) continue;
            i.GetComponent<PrWeapon>().LaserSight.enabled = false;
            i.SetActive(false);
        }
        activeWeapon = weaponInt;
        carriedWeapons[activeWeapon].SetActive(true);
        InitializeWeapons();
        carriedWeapons[activeWeapon].GetComponent<PrWeapon>().UpdateWeaponGUI(_menuController.HUDWeaponPicture);
    }

    private void ChangeWeapon()
    {
        _lastWeaponChange = Time.time;
        var nextWeapon = activeWeapon;
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            nextWeapon = activeWeapon + 1;
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
            nextWeapon = activeWeapon - 1;
        if (nextWeapon >= playerWeaponLimit || nextWeapon < 0)
            nextWeapon = 0;

        //New Multiple weapon system
        if (carriedWeapons[nextWeapon] != null)
        {
            //Debug.Log(ActiveWeapon + " " + nextWeapon);
            foreach (GameObject i in carriedWeapons)
            {
                if (i != null)
                {
                    i.GetComponent<PrWeapon>().LaserSight.enabled = false;
                    i.SetActive(false);
                }
                //Debug.Log("Deactivating Weapon " + Weapon[ActiveWeapon]);
            }
                        
            activeWeapon = nextWeapon;

            carriedWeapons[activeWeapon].SetActive(true);

            InitializeWeapons();
            carriedWeapons[activeWeapon].GetComponent<PrWeapon>().UpdateWeaponGUI(_menuController.HUDWeaponPicture);
        }
        else
        {
            for (int i = nextWeapon; i < playerWeaponLimit; i++)
            {
                if (carriedWeapons[i] != null)
                {
                    activeWeapon = i - 1;
                    ChangeWeapon();
                    break;
                }
            }
            
            activeWeapon = playerWeaponLimit - 1;
            //Debug.Log(playerWeaponLimit);
            ChangeWeapon();
        }
    }

	public void StopUse()
	{
		charController.m_CanMove = true;
		charAnimator.SetTrigger(Use);
        usingObject = false;

        EnableArmIK(true);
       
    }
    public void EndPickup()
    {
        charController.m_CanMove = true;
        usingObject = false;

        EnableArmIK(true);
    }

    public void BulletPos(Vector3 bulletPosition)
    {
        _lastHitPos = bulletPosition;
        _lastHitPos.y = 0;
    }

    public void SetNewSpeed(float speedFactor)
    {
        charController.m_MoveSpeedSpecialModifier = speedFactor;
    }

    public void SetHealth(int healthInt)
    {
        actualHealth = healthInt;
        _menuController.HUDHealthBar.GetComponent<RectTransform>().localScale = actualHealth > 1 ? 
            new Vector3(Mathf.Clamp((1.0f / health) * actualHealth,0.1f,1.0f) , 1.0f, 1.0f) : 
            new Vector3(0.0f, 1.0f, 1.0f);
    }

    private void ApplyTemperature()
    {
        if (temperature is > 1.0f or < 1.0f)
        {
            if (tempTimer < 1.0f)
                tempTimer += Time.deltaTime;
            else
            {
                tempTimer = 0.0f;
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
        {
            _actualFrozenVFX.SetActive(temperature < 1.0f);
        }
        if (_actualBurningVFX)
        {
            _actualBurningVFX.SetActive(temperature > 1.0f);
        }
    }

    private void ApplyTemperatureDamage()
    {
        switch (temperature)
        {
            case < 1.0f:
            case > 1.0f:
                ApplyDamagePassive(temperatureDamage);
                break;
        }
    }

    private void ApplyDamagePassive(int damage)
    {
        if (isDead) return;
        SetHealth(actualHealth - damage);

        if (actualHealth > 0) return;
        if (_actualSplatVFX)
            _actualSplatVFX.transform.parent = null;

        Die(true);
    }

    public void ApplyDamage(int damage)
    {
        if (actualHealth <= 0) return;
        //Here you can put some Damage Behaviour if you want
        SetHealth(actualHealth - damage);

        _damaged = true;
        _damagedTimer = 1.0f;

        if (_actualSplatVFX)
        {
            _actualSplatVFX.transform.LookAt(_lastHitPos);
            _actualSplatVFX.Splat();
        }

        if (actualHealth > 0) return;
        if (_actualSplatVFX)
            _actualSplatVFX.transform.parent = null;
        if (damage >= damageThreshold)
            _explosiveDeath = true;
        Die(false);
    }

    public void ApplyDamageNoVFX(int damage)
    {
        if (actualHealth > 0)
        {
            //Here you can put some Damage Behaviour if you want
            SetHealth(actualHealth - damage);

            _damaged = true;
            _damagedTimer = 1.0f;

            if (actualHealth <= 0)
            {
                if (_actualSplatVFX)
                    _actualSplatVFX.transform.parent = null;
                if (damage >= damageThreshold)
                    _explosiveDeath = true;
                Die(true);
            }
        }
    }

    private void Die(bool temperatureDeath)
	{
        charController.AimTargetVisual.SetActive(false);
        DeactivateCompass();
        EnableArmIK(false);
		isDead = true;
		charAnimator.SetBool(Dead, true);

        charController.m_isDead = true;
        carriedWeapons[activeWeapon].GetComponent<PrWeapon>().TurnOffLaser();
        
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().enabled = false;
        GetComponent<ClickToMove>().enabled = false;
        GetComponent<ClickableObjectHandler>().enabled = false;
        GetComponent<PlayerInput>().enabled = false;
        GetComponent<NavMeshAgent>().enabled = false;
        this.tag = "Untagged";

        DestroyHUD();
        AvatarMenuController.Instance.gameOverScreen.SetActive(true);
        if (useRagdollDeath)
        {
            Vector3 ragdollDirection = transform.position - _lastHitPos;
            ragdollDirection = ragdollDirection.normalized;
            if (!temperatureDeath)
                GetComponent<PrCharacterRagdoll>().SetForceToRagdoll(_lastHitPos + new Vector3(0,1.5f,0), ragdollDirection * (ragdollForceFactor * Random.Range(0.8f,1.2f)), BurnAndFrozenVFXParent);
        }
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject i in enemies)
        {
            i.SendMessage("FindPlayers", SendMessageOptions.DontRequireReceiver);
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
            if (deathVFX && _actualDeathVFX)
            {
                if (temperatureDeath)
                {
                    //Freezing of Burning Death VFX...
                }
                else
                {
                    _actualDeathVFX.transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
                    _actualDeathVFX.transform.LookAt(_lastHitPos);
                    _actualDeathVFX.transform.position = new Vector3(transform.position.x, 0.05f, transform.position.z);

                    _actualDeathVFX.SetActive(true);

                    var particles = _actualDeathVFX.GetComponentsInChildren<ParticleSystem>();

                    if (particles.Length > 0)
                    {
                        foreach (ParticleSystem p in particles)
                        {
                            p.Play();
                        }
                    }
                }
            }
               
        }

        Destroy(charController);
        Destroy(GetComponent<Collider>());
    }

    private void DestroyHUD()
    {
        if (carriedWeapons[activeWeapon] != null)
        {
            //Destroy GUI
            carriedWeapons[activeWeapon].GetComponent<PrWeapon>().updateHUD = false;
        }
       
        if (_menuController.HUDDamageFullScreen != null)
        {
            if (_menuController.HUDDamageFullScreen.transform.parent.gameObject != null)
                Destroy(_menuController.HUDDamageFullScreen.transform.parent.gameObject);
        }
        if (_menuController.HUDWeaponPicture != null)
        {
            if (_menuController.HUDWeaponPicture.transform.parent.gameObject != null)
                Destroy(_menuController.HUDWeaponPicture.transform.parent.gameObject);
        }
    }

	public void EndReload()
	{
		_canShoot = true;
        charAnimator.SetBool(Reloading, false);
        if (_menuController.useQuickReload && _menuController.quickReloadPanel && 
            _menuController.quickReloadMarker && _menuController.quickReloadZone)
        {
            QuickReloadActive(false);
        }
        EnableArmIK(true);
    }

	void OnTriggerStay(Collider other) {
        
        if (other.CompareTag("Usable") && usableObject == null)
        {
                if (other.GetComponent<UsableDevice>().IsEnabled)
                    usableObject = other.gameObject;
        }
        else if (other.CompareTag("Pickup") && _pickupObj == null )
        {
            _pickupObj = other.gameObject;
        }
    }

	void OnTriggerExit(Collider other)
	{
        
        if (other.CompareTag("Usable") && usableObject != null)
        {
            usableObject = null;
            HUDUseHelper.SetActive(false);
        }
        if (other.CompareTag("Pickup") && _pickupObj != null)
        {
           
            _pickupObj = null;
               
        }
        
	}
        
}

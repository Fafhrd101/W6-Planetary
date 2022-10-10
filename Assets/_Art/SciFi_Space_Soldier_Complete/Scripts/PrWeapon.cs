using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PrWeapon : MonoBehaviour 
{
    public struct WeapDam
    {
        public int Damage;
        public GameObject Attacker;
    }
    public string weaponName = "Rifle";
    [Tooltip("Who's using the weapon")]
    public GameObject attacker;
    public enum Wt { Pistol = 0, Rifle = 1, Minigun = 2, RocketLauncher = 3, Melee = 4, Laser = 5 }
    public Wt type = Wt.Rifle;
    public bool useIK = true;

    [Header("Melee Weapon")]
    public float MeleeRadius = 1.0f;
    public int meleeDamage = 1;
    private List<GameObject> _meleeFinalTarget;

    [Header("Stats")]
    public int BulletsPerShoot = 1;
    public int BulletDamage = 20;
    public float tempModFactor = 0.0f;
    public float BulletSize = 1.0f;
    public float BulletSpeed = 1.0f;
    public float BulletAccel = 0.0f;
    public int Bullets = 10;
    [HideInInspector]
    public int actualBullets = 0;
    public int Clips = 3;
    [HideInInspector]
    public int actualClips = 0;
    public float ReloadTime = 1.0f;
    public bool playReloadAnim = true;
    private float _actualReloadTime = 0.0f;
    public float bulletTimeToLive = 3.0f;
    [HideInInspector]
    public bool reloading = false;
    public float FireRate = 0.1f;
    public float AccDiv;
    public float radialAngleDirection = 0.0f;
    public float shootingNoise = 25f;

    [Header("Quick Reload")]
    public bool useQuickReload = true;
    public Vector2 HUDquickReloadTimes = new Vector2(0.5f, 0.7f);
    private bool _quickReloadActive = false;

    [Header("References & VFX")]
    public float shootShakeFactor = 2.0f;
    public Transform ShootFXPos;
    public GameObject BulletPrefab;
    public GameObject ShootFXFLash;
    public Light ShootFXLight;
    public Renderer LaserSight;
    private PrTopDownCamera _playerCamera;

    [Header("Laser Weapon Settings")]
    public GameObject laserBeamPrefab;
    private GameObject[] _actualBeams;
    public float laserWidthFactor = 1.0f;
    public float laserLiveTime = 1.0f;
    public float warmingTime = 0.2f;
    public bool generatesBloodDamage = true;
    public GameObject warmingVFX;
    private GameObject _actualWarmingVFX;
    public GameObject laserHitVFX;
    private GameObject[] _actualLaserHits;

    [HideInInspector]
    public Transform ShootTarget;

    [Header("Sound FX")]
    public AudioClip[] ShootSFX;
    public AudioClip ReloadSFX;
    public AudioClip ShootEmptySFX;
    //[HideInInspector]
    public AudioSource Audio;

    [Header("Autoaim")]
    public float AutoAimAngle = 7.5f;
    public float AutoAimDistance = 10.0f;

    private Vector3 NPCTargetAuto = Vector3.zero;
    private Vector3 FinalTarget = Vector3.zero;

    //HUD
    [Header("HUD")]
    [HideInInspector]
    public bool updateHUD = true;

    public Sprite WeaponPicture;
    [HideInInspector]
    public GameObject HUDWeaponPicture;
    [HideInInspector]
    //public GameObject HUDWeaponName;
    //[HideInInspector]
    public GameObject HUDWeaponBullets;
    [HideInInspector]
    public GameObject HUDWeaponBulletsBar;
    [HideInInspector]
    public GameObject HUDWeaponBulletsBarBack;
    [HideInInspector]
    public GameObject HUDWeaponClips;
    [HideInInspector]
    public GameObject HUDquickReloadMarker;
    [HideInInspector]
    public GameObject HUDquickReloadZone;


    //Object Pooling Manager
    public bool usePooling = true;
    private GameObject[] _gameBullets;
    private GameObject _bulletsParent;
    private int _actualGameBullet = 0;
    private GameObject _muzzle;
    [HideInInspector]
    public bool aiWeapon = false;
    [HideInInspector]
    public Transform ainpcTarget;
    [HideInInspector]
    public bool turretWeapon = false;
    private static readonly int Reloading = Animator.StringToHash("Reloading");

    private void Awake()
    {
        actualBullets = Bullets;
        actualClips = Clips;
    }

    // Use this for initialization
    private void Start()
    {
        Audio = transform.parent.GetComponent<AudioSource>();
        if (!aiWeapon)
        {
            HUDWeaponBullets.GetComponent<TMP_Text>().text = (actualBullets / BulletsPerShoot).ToString();
            HUDWeaponClips.GetComponent<TMP_Text>().text = actualClips.ToString();
            HUDWeaponBulletsBar.GetComponent<Image>().fillAmount = (1.0f / Bullets) * actualBullets;
            HUDWeaponBulletsBar.GetComponent<RectTransform>().localScale = Vector3.one;
        }

        switch (type)
        {
            //Basic Object Pooling Initialization ONLY FOR RANGED WEAPONS
            case Wt.Rifle:
            case Wt.Pistol:
            case Wt.Minigun:
            case Wt.RocketLauncher:
            {
                if (usePooling)
                {
                    _gameBullets = new GameObject[Bullets * BulletsPerShoot];
                    _bulletsParent = new GameObject(weaponName + "_Bullets")
                    {
                        transform =
                        {
                            parent = ParticlePool.Instance.transform
                        }
                    };
                    for (var i = 0; i < (Bullets * BulletsPerShoot); i++)
                    {
                        _gameBullets[i] = Instantiate(BulletPrefab, ShootFXPos.position, ShootFXPos.rotation) as GameObject;
                        _gameBullets[i].SetActive(false);
                        _gameBullets[i].name = weaponName + "_Bullet_" + i.ToString();
                        _gameBullets[i].transform.parent = _bulletsParent.transform;

                        _gameBullets[i].GetComponent<PrBullet>().attacker = attacker;
                        _gameBullets[i].GetComponent<PrBullet>().usePooling = true;
                        _gameBullets[i].GetComponent<PrBullet>().InitializePooling();
                    }
                }

                break;
            }
            case Wt.Laser:
            {
                _actualBeams = new GameObject[BulletsPerShoot];
                _actualLaserHits = new GameObject[BulletsPerShoot];
                var bulletsParent = new GameObject(weaponName + "_Beams")
                {
                    transform =
                    {
                        parent = ParticlePool.Instance.transform
                    }
                };
                //Laser Weapon Initialization
                for (var i = 0; i < BulletsPerShoot; i++)
                {
                    _actualBeams[i] = Instantiate(laserBeamPrefab, ShootFXPos.position, ShootFXPos.rotation);
                    _actualBeams[i].SetActive(false);
                    _actualBeams[i].name = weaponName + "_Beam_" + i.ToString();
                    _actualBeams[i].transform.parent = bulletsParent.transform;
                    _actualBeams[i].GetComponent<PrWeaponLaserBeam>().InitializeLine(laserWidthFactor, ShootFXPos);

                    _actualLaserHits[i] = Instantiate(laserHitVFX, ShootFXPos.position, ShootFXPos.rotation);
                    _actualLaserHits[i].SetActive(false);
                    _actualLaserHits[i].name = weaponName + "_Beam_Hit_" + i.ToString();
                    _actualLaserHits[i].transform.parent = bulletsParent.transform;
                }

                if (turretWeapon)
                {
                    ShootTarget = new GameObject("ShootTarget").transform;
                    ShootTarget.SetParent(transform);
                }

                break;
            }
            case Wt.Melee:
                //Melee Weapon Initialization
                /*
            HUDWeaponBullets.GetComponent<TMP_Text>().text = "";
            HUDWeaponClips.GetComponent<TMP_Text>().text = "";
            HUDWeaponBulletsBar.GetComponent<RectTransform>().localScale = Vector3.zero;*/
                break;
        }

        if (ShootFXFLash)
        {
            _muzzle = Instantiate(ShootFXFLash, ShootFXPos.position, ShootFXPos.rotation);
            _muzzle.transform.parent = ShootFXPos.transform;
            _muzzle.SetActive(false);
        }

        if (GameObject.Find("PlayerCamera") != null)
        {
            _playerCamera = GameObject.Find("PlayerCamera").GetComponent<PrTopDownCamera>();

        }

        if (!useQuickReload) return;
        HUDquickReloadTimes[0] = HUDquickReloadTimes[0] switch
        {
            < 0.0f => 0.0f,
            >= 1.0f => 0.98f,
            _ => HUDquickReloadTimes[0]
        };

        HUDquickReloadTimes[1] = HUDquickReloadTimes[1] switch
        {
            < 0.0f => 0.1f,
            >= 1.0f => 0.99f,
            _ => HUDquickReloadTimes[1]
        };
    }

    // Update is called once per frame
    public void Update()
    {
        if (!reloading) return;
        _actualReloadTime += Time.deltaTime;

        if (!aiWeapon && !turretWeapon && useQuickReload)
        {
            if (HUDquickReloadMarker != null)
                HUDquickReloadMarker.GetComponent<RectTransform>().localPosition = new Vector3(_actualReloadTime * (46.0f / ReloadTime), 0, 0);

            if (_actualReloadTime >= (HUDquickReloadTimes[0] * ReloadTime) && _actualReloadTime <= (HUDquickReloadTimes[1] * ReloadTime))
                _quickReloadActive = true;
            else
                _quickReloadActive = false;
        }

        if (_actualReloadTime >= ReloadTime)
        {
            PositiveReload();
        }
    }

    private void OnDestroy()
    {
        if (_bulletsParent)
            Destroy(_bulletsParent);
    }

    private void PositiveReload()
    {
        reloading = false;
        _actualReloadTime = 0.0f;
        SendMessageUpwards("EndReload", SendMessageOptions.DontRequireReceiver);

        WeaponEndReload();
    }

    public void SetupQuickReload()
    {
        HUDquickReloadZone.GetComponent<RectTransform>().localPosition = new Vector3(HUDquickReloadTimes[0] * 46.0f, 0, 0);
        HUDquickReloadZone.GetComponent<RectTransform>().localScale = new Vector3(HUDquickReloadTimes[1] - HUDquickReloadTimes[0], 1, 1);
    }

    public void TryQuickReload()
    {
        if (!_quickReloadActive) return;
        PositiveReload();
        _quickReloadActive = false;
    }

    public void TurnOffLaser()
    {
        LaserSight.enabled = false;
    }

    public void LateUpdate()
    {
        if (aiWeapon) return;
        LaserSight.transform.position = ShootFXPos.position;
        LaserSight.transform.LookAt(ShootTarget.position, Vector3.up);
    }

    private void WeaponEndReload()
    {
        actualBullets = Bullets;
        UpdateWeaponGUI();
    }

    private void UpdateWeaponGUI()
    {
        if (aiWeapon || type == Wt.Melee || !updateHUD) return;
        HUDWeaponBullets.GetComponent<TMP_Text>().text = (actualBullets / BulletsPerShoot).ToString();
        HUDWeaponClips.GetComponent<TMP_Text>().text = "X"+actualClips.ToString();
        HUDWeaponBulletsBar.GetComponent<Image>().fillAmount =(1.0f / Bullets) * actualBullets;
        //Debug.Log("Bullets = " + Bullets);
    }

    public void UpdateWeaponGUI(GameObject weapPic)
    {
        switch (aiWeapon)
        {
            case false when type != Wt.Melee:
            {
                HUDWeaponBullets.GetComponent<TMP_Text>().text = (actualBullets / BulletsPerShoot).ToString();
                HUDWeaponClips.GetComponent<TMP_Text>().text = "X "+actualClips.ToString();
                HUDWeaponBulletsBar.GetComponent<Image>().enabled = true;
                HUDWeaponBulletsBar.GetComponent<Image>().fillAmount = (1.0f / Bullets) * actualBullets;
                HUDWeaponPicture = weapPic;
                if (HUDWeaponPicture.GetComponentInChildren<TMP_Text>())
                    HUDWeaponPicture.GetComponentInChildren<TMP_Text>().text = weaponName;
                break;
            }
            case false when type == Wt.Melee:
            {
                HUDWeaponBullets.GetComponent<TMP_Text>().text = "";
                HUDWeaponClips.GetComponent<TMP_Text>().text = "";
                HUDWeaponBulletsBar.GetComponent<Image>().enabled = false;
                HUDWeaponPicture = weapPic;
                if (HUDWeaponPicture.GetComponentInChildren<TMP_Text>())
                    HUDWeaponPicture.GetComponentInChildren<TMP_Text>().text = weaponName;
                break;
            }
        }
    }

    public void CancelReload()
    {
        reloading = false;
        if (playReloadAnim)
            attacker.GetComponent<Animator>().SetBool(Reloading, false);
        SendMessageUpwards("EndReload", SendMessageOptions.DontRequireReceiver);
        _actualReloadTime = 0.0f;
    }

	public void Reload()
    {
        if (actualClips <= 0 && Clips != -1) return;
        if (!aiWeapon || !turretWeapon)
        {
            if (useQuickReload)
                SendMessageUpwards("QuickReloadActive", true, SendMessageOptions.DontRequireReceiver);
            actualClips -= 1;
        }
            
        if (playReloadAnim && !turretWeapon && !aiWeapon)
            attacker.GetComponent<Animator>().SetBool(Reloading, true);
        reloading = true;
        Audio.PlayOneShot(ReloadSFX);
        _actualReloadTime = 0.0f;
    }

    public void AIReload()
    {
        SendMessageUpwards("StartReload", SendMessageOptions.DontRequireReceiver);
        reloading = true;
        Audio.PlayOneShot(ReloadSFX);
        _actualReloadTime = 0.0f;
    }

    private void AutoAim()
    {
        var npcs = GameObject.FindGameObjectsWithTag("NPC");
        if (npcs != null)
        {
            var bestDistance = 100.0f;

            foreach (GameObject npc in npcs)
            {
                var npcPos = npc.transform.position;
                var npcDirection = npcPos - attacker.transform.position;
                var npcDistance = npcDirection.magnitude;

                if (!(Vector3.Angle(attacker.transform.forward, npcDirection) <= AutoAimAngle) ||
                    !(npcDistance < AutoAimDistance)) continue;
                if (npc.GetComponent<PrNPCAI>().currentState != PrNPCAI.AIState.Dead)
                {
                    if (npcDistance < bestDistance)
                    {
                        bestDistance = npcDistance;
                        NPCTargetAuto = npcPos + new Vector3(0, 1, 0);
                    }
                }
            }
        }

        if (NPCTargetAuto != Vector3.zero)
        {
            FinalTarget = NPCTargetAuto;
            ShootFXPos.transform.LookAt(FinalTarget);
        }
        else
        {
            ShootFXPos.transform.LookAt(ShootTarget.position);
            FinalTarget = ShootTarget.position;
        }
    }

    private void AIAutoAim()
    {
        var playerPos = ainpcTarget.position + new Vector3(0, 1.5f, 0);
        FinalTarget = playerPos;
    }

    private void PlayShootAudio()
    {
        if (ShootSFX.Length <= 0) return;
        int footStepAudio = 0;

        if (ShootSFX.Length > 1)
            footStepAudio = Random.Range(0, ShootSFX.Length);

        var randomVolume = Random.Range(0.1f, .2f);

        Audio.PlayOneShot(ShootSFX[footStepAudio], randomVolume);

        if (!aiWeapon)
            attacker.SendMessage("MakeNoise", shootingNoise);
    }

    public void Shoot()
	{
        if (aiWeapon || turretWeapon)
        {
            AIAutoAim();
        }
        else
        {
            AutoAim();
        }

        if (actualBullets > 0)
            PlayShootAudio();
        //else
        //    Audio.PlayOneShot(ShootEmptySFX);
        var angleStep = radialAngleDirection / BulletsPerShoot;
        var finalAngle = 0.0f; 

        for (var i = 0; i < BulletsPerShoot; i++)
		{
            
            var finalAccuracyModX = Random.Range(AccDiv, -AccDiv) * Vector3.Distance(attacker.transform.position, FinalTarget);
            finalAccuracyModX /= 100;

            var FinalAccuracyModY = Random.Range(AccDiv, -AccDiv) * Vector3.Distance(attacker.transform.position, FinalTarget);
            FinalAccuracyModY /= 100;

            var finalAccuracyModZ = Random.Range(AccDiv, -AccDiv) * Vector3.Distance(attacker.transform.position, FinalTarget);
            finalAccuracyModZ /= 100;
          
            var finalOrientation = FinalTarget + new Vector3(finalAccuracyModX, FinalAccuracyModY, finalAccuracyModZ);

			ShootFXPos.transform.LookAt(finalOrientation);

            if (BulletsPerShoot > 1 && radialAngleDirection > 0.0f)
            {
                Quaternion aimLocalRot = Quaternion.Euler(0, finalAngle - (radialAngleDirection / 2) + (angleStep * 0.5f), 0);
                ShootFXPos.transform.rotation = ShootFXPos.transform.rotation * aimLocalRot;

                finalAngle += angleStep;
            }

            if (type == Wt.Melee)
            {
                print("Fire Melee weapon");
            }
            else if (type != Wt.Laser && BulletPrefab && ShootFXPos && !reloading)
            {
                if (actualBullets > 0)
                {
                    GameObject bullet;
                    if (usePooling)
                    {
                        //Object Pooling Method 
                        bullet = _gameBullets[_actualGameBullet];
                        bullet.transform.position = ShootFXPos.position;
                        bullet.transform.rotation = ShootFXPos.rotation;
                        bullet.GetComponent<Rigidbody>().isKinematic = false;
                        bullet.GetComponent<Collider>().enabled = true;
                        bullet.GetComponent<PrBullet>().timeToLive = bulletTimeToLive;
                        bullet.GetComponent<PrBullet>().ResetPooling();
                        bullet.SetActive(true);
                        _actualGameBullet += 1;
                        if (_actualGameBullet >= _gameBullets.Length)
                            _actualGameBullet = 0;
                    }
                    else
                    {
                        bullet = Instantiate(BulletPrefab, ShootFXPos.position, ShootFXPos.rotation);
                        bullet.GetComponent<PrBullet>().usePooling = false;
                        bullet.SetActive(true);
                        bullet.GetComponent<Rigidbody>().isKinematic = false;
                        bullet.GetComponent<Collider>().enabled = true;
                        bullet.GetComponent<PrBullet>().timeToLive = bulletTimeToLive;
                    }
                    
                    //Object Pooling VFX
                    _muzzle.transform.rotation = transform.rotation;
                    EmitParticles(_muzzle);

                    //Generic 
                    bullet.GetComponent<PrBullet>().Damage = BulletDamage;
                    bullet.GetComponent<PrBullet>().temperatureMod = tempModFactor;
                    bullet.GetComponent<PrBullet>().bulletSpeed = BulletSpeed;
                    bullet.GetComponent<PrBullet>().bulletAccel = BulletAccel;
                    bullet.GetComponent<PrBullet>().attacker = attacker;
                    if (usePooling)
                        bullet.transform.localScale = bullet.GetComponent<PrBullet>().originalScale * BulletSize;

                    ShootFXLight.GetComponent<PrLightAnimator>().AnimateLight(true);
                    actualBullets -= 1;

                    // if (_playerCamera)
                    // {
                    //     if (!aiWeapon)
                    //         _playerCamera.Shake(shootShakeFactor, 0.2f);
                    //     else
                    //         _playerCamera.Shake(shootShakeFactor * 0.5f, 0.2f);
                    // }

                    if (actualBullets == 0)
                        Reload();

                }

            }
            // Laser Shoot
            else if (type == Wt.Laser && _actualBeams.Length != 0 && ShootFXPos && !reloading)
            {
                bool useDefaultImpactFX = true;
                
                Vector3 hitPos = ShootTarget.position + new Vector3(0, 1.2f, 0);

                Vector3 hitNormal = ShootTarget.forward;


                if (actualBullets > 0)
                {
                    
                    //Object Pooling Method 
                    GameObject beam = _actualBeams[_actualGameBullet];
                    beam.transform.position = ShootFXPos.position;
                    beam.transform.rotation = ShootFXPos.rotation;
                    beam.SetActive(true);
                    beam.GetComponent<PrWeaponLaserBeam>().Activate(laserLiveTime);
                    //Shoot Beam
                    if (Physics.Raycast(ShootFXPos.position, ShootFXPos.forward, out var hit))
                    {
                        var target = hit.collider.gameObject;
                         hitPos = hit.point;
                        hitNormal = hit.normal;
                        beam.GetComponent<PrWeaponLaserBeam>().SetPositions(ShootFXPos.position, hitPos);

                        if (hit.collider.CompareTag("Player"))
                        {
                            target.SendMessage("SetAttacker", attacker, SendMessageOptions.DontRequireReceiver);
                            target.SendMessage("BulletPos", hit.point, SendMessageOptions.DontRequireReceiver);
                            target.SendMessage("ApplyTempMod", tempModFactor, SendMessageOptions.DontRequireReceiver);
                            
                            if (generatesBloodDamage)
                            {
                                target.SendMessage("ApplyDamage", BulletDamage, SendMessageOptions.DontRequireReceiver);
                                if (target.GetComponent<PrTopDownCharInventory>().DamageFX != null)
                                {
                                    Instantiate(target.GetComponent<PrTopDownCharInventory>().DamageFX, hitPos, Quaternion.LookRotation(hitNormal));
                                    useDefaultImpactFX = false;
                                }
                            }
                            else
                            {
                                target.SendMessage("ApplyDamageNoVFX", BulletDamage, SendMessageOptions.DontRequireReceiver);
                            }

                        }
                        else if (hit.collider.CompareTag("NPC"))
                        {
                            target.SendMessage("SetAttacker", attacker, SendMessageOptions.DontRequireReceiver);
                            target.SendMessage("BulletPos", hit.point, SendMessageOptions.DontRequireReceiver);
                            target.SendMessage("ApplyTempMod", tempModFactor, SendMessageOptions.DontRequireReceiver);
                            
                            if (generatesBloodDamage)
                            {
                                target.SendMessage("ApplyDamage", BulletDamage, SendMessageOptions.DontRequireReceiver);
                                if (target.GetComponent<PrNPCAI>().damageVFX != null)
                                {
                                    Instantiate(target.GetComponent<PrNPCAI>().damageVFX, hitPos, Quaternion.LookRotation(hitNormal));
                                    useDefaultImpactFX = false;
                                }
                            }
                            else
                            {
                                target.SendMessage("ApplyDamageNoVFX", BulletDamage, SendMessageOptions.DontRequireReceiver);
                            }
                        }

                        else if (hit.collider.CompareTag("NPC"))
                        {
                            target.SendMessage("SetAttacker", attacker, SendMessageOptions.DontRequireReceiver);
                            target.SendMessage("BulletPos", hit.point, SendMessageOptions.DontRequireReceiver);
                            target.SendMessage("ApplyTempMod", tempModFactor, SendMessageOptions.DontRequireReceiver);
                            
                            if (generatesBloodDamage)
                            {
                                target.SendMessage("ApplyDamage", BulletDamage, SendMessageOptions.DontRequireReceiver);

                                if (target.GetComponent<PrNPCAI>().damageVFX != null)
                                {
                                    Instantiate(target.GetComponent<PrNPCAI>().damageVFX, hitPos, Quaternion.LookRotation(hitNormal));
                                    useDefaultImpactFX = false;
                                }
                            }
                            else
                            {
                                target.SendMessage("ApplyDamageNoVFX", BulletDamage, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                        else if (hit.collider.CompareTag("Destroyable"))
                        {
                            target.SendMessage("SetAttacker", attacker, SendMessageOptions.DontRequireReceiver);
                            target.SendMessage("BulletPos", hit.point, SendMessageOptions.DontRequireReceiver);
                            target.SendMessage("ApplyTempMod", tempModFactor, SendMessageOptions.DontRequireReceiver);
                            target.SendMessage("ApplyDamage", BulletDamage, SendMessageOptions.DontRequireReceiver);
                            if (target.GetComponent<Rigidbody>())
                            {
                                target.GetComponent<Rigidbody>().AddForceAtPosition(hitNormal * Random.Range(-200.0f,-400.0f), hitPos);
                            }
                        }
                    }

                    else
                    {
                        beam.GetComponent<PrWeaponLaserBeam>().SetPositions(ShootFXPos.position, ShootTarget.position + new Vector3(0,1.2f,0));
                    }

                    //default Hit VFX
                    if (useDefaultImpactFX)
                    {
                        _actualLaserHits[_actualGameBullet].SetActive(true);
                        _actualLaserHits[_actualGameBullet].transform.position = hitPos;
                        _actualLaserHits[_actualGameBullet].transform.rotation = Quaternion.LookRotation(hitNormal);
                        _actualLaserHits[_actualGameBullet].GetComponent<ParticleSystem>().Play();
                    }

                    _actualGameBullet += 1;
                    //Object Pooling VFX
                    _muzzle.transform.rotation = transform.rotation;
                    EmitParticles(_muzzle);

                    if (_actualGameBullet >= _actualBeams.Length)
                        _actualGameBullet = 0;

                    ShootFXLight.GetComponent<PrLightAnimator>().AnimateLight(true);
                    actualBullets -= 1;

                    // if (_playerCamera)
                    // {
                    //     if (!aiWeapon)
                    //         _playerCamera.Shake(shootShakeFactor, 0.2f);
                    //     else
                    //         _playerCamera.Shake(shootShakeFactor * 0.5f, 0.2f);
                    // }
                    if (actualBullets == 0)
                        Reload();
                }
            }

            UpdateWeaponGUI();

            NPCTargetAuto = Vector3.zero;

            
        }
	}

    private void EmitParticles(GameObject vfxEmitter)
    {
        vfxEmitter.SetActive(true);
        vfxEmitter.GetComponent<ParticleSystem>().Play();
    }


    public void AIAttackMelee(Vector3 playerPos, GameObject targetGO)
    {
        PlayShootAudio();

        //Object Pooling VFX
        if (_muzzle)
            EmitParticles(_muzzle);
        if (ShootFXLight)
            ShootFXLight.GetComponent<PrLightAnimator>().AnimateLight(true);

        if (!(Vector3.Distance(playerPos + Vector3.up, ShootFXPos.position) <= MeleeRadius)) return;
        //Debug.Log("Hit Player Successfully");
        targetGO.SendMessage("SetAttacker", attacker, SendMessageOptions.DontRequireReceiver);
        targetGO.SendMessage("BulletPos", ShootFXPos.position, SendMessageOptions.DontRequireReceiver);
        targetGO.SendMessage("ApplyDamage", meleeDamage, SendMessageOptions.DontRequireReceiver);
    }

    public void AttackMelee()
    {
        PlayShootAudio();
        //Object Pooling VFX
        if (_muzzle) EmitParticles(_muzzle);
        //Use Light
        if (ShootFXLight) ShootFXLight.GetComponent<PrLightAnimator>().AnimateLight(true);
        //Start Finding NPC Target
        _meleeFinalTarget = new List<GameObject>();

        var npcsTemp = GameObject.FindGameObjectsWithTag("NPC");
        // GameObject[] PlayersTemp = GameObject.FindGameObjectsWithTag("Player");
        // GameObject[] npcs = new GameObject[npcsTemp.Length + PlayersTemp.Length];
        var npcs = new GameObject[npcsTemp.Length];
        var t = 0;
        foreach (GameObject E in npcsTemp)
        {
            npcs[t] = E;
            t += 1;
        }
        
        var bestDistance = 100.0f;

        foreach (var npc in npcs)
        {
            var npcPos = npc.transform.position;
            var npcDirection = npcPos - attacker.transform.position;
            var npcDistance = npcDirection.magnitude;

            if (!(Vector3.Angle(attacker.transform.forward, npcDirection) <= 90) ||
                !(npcDistance < MeleeRadius)) continue;
            if (npc.GetComponent<PrNPCAI>())
            {
                if (npc.GetComponent<PrNPCAI>().currentState == PrNPCAI.AIState.Dead) continue;
                if (!(npcDistance < bestDistance)) continue;
                bestDistance = npcDistance;
                _meleeFinalTarget.Add(npc);
            }
            else if (npc.GetComponent<PrTopDownCharInventory>())
            {
                if (npc.GetComponent<PrTopDownCharInventory>().isDead == true) continue;
                if (!(npcDistance < bestDistance)) continue;
                bestDistance = npcDistance;
                _meleeFinalTarget.Add(npc);
            }
        }

        var destroyables = GameObject.FindGameObjectsWithTag("Destroyable");

        if (destroyables != null)
        {
            bestDistance = 100.0f;

            foreach (GameObject destroyable in destroyables)
            {
                var destroyablePos = destroyable.transform.position;
                var destroyDirection = destroyablePos - attacker.transform.position;
                var npcDistance = destroyDirection.magnitude;

                if (Vector3.Angle(attacker.transform.forward, destroyDirection) <= 90 && npcDistance < MeleeRadius)
                {
                    if (npcDistance < bestDistance)
                    {
                        bestDistance = npcDistance;
                        _meleeFinalTarget.Add(destroyable);
                    }
                }
            }
        }

        foreach (var meleeTarget in _meleeFinalTarget)
        {
            //Debug.Log("Hit NPC Successfully");
            meleeTarget.SendMessage("SetAttacker", attacker, SendMessageOptions.DontRequireReceiver);
            meleeTarget.SendMessage("BulletPos", ShootFXPos.position, SendMessageOptions.DontRequireReceiver);
            meleeTarget.SendMessage("ApplyDamage", meleeDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void LoadAmmo(int loadType)
    {
        actualBullets = Bullets;
        actualClips = Clips / loadType;
        WeaponEndReload();
    }

    void OnDrawGizmos()
    {
        /*
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(FinalTarget, 0.25f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ShootFXPos.position, 0.2f);*/
    }
}

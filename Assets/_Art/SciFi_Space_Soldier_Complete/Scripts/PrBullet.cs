using UnityEngine;

public class PrBullet : MonoBehaviour
{
    [Tooltip("Who created this bullet")]
    public GameObject attacker;
    public bool isGrenade = false;
    public float explodeTimer = 5.0f;
    public bool friendlyFire = true;
    
    [Header("General Stats")]
    public bool UsePhysicsToTranslate = false;
    public bool UsePhysicsCollisions = false;
    public int Damage = 0;
    [HideInInspector]
    public float hitForce = 100.0f;
    [HideInInspector]
    public float bulletSpeed = 1.0f;
    [HideInInspector]
    public float bulletAccel = 0.0f;
    [HideInInspector]
    public float temperatureMod = 0.0f;
    private Vector3 _fwd = Vector3.zero;

    [Header("Explosive Stats")]
    public bool RadialDamage = false;
    public float DamageRadius = 3.0f;
    public float RadialForce = 600.0f;
    public float cameraShakeFactor = 3.0f;
    public float cameraShakeDuration = 0.5f;

    [Header("VFX")]
    public bool generatesBloodDamage = true;
    public GameObject DefaultImpactFX;
	private bool _useDefaultImpactFX = true;
	public GameObject DefaultImpactDecal;
    public GameObject[] DetachOnDie;
    
    //Object Pooling
    [Header("Object Pooling")]
    [HideInInspector]
    public bool usePooling = true;
    public float timeToLive = 3.0f;
    private Vector3[] _detachablePositions;
    private Quaternion[] _detachableRotations;
    private Vector3[] _detachableScales;
    [HideInInspector] public Vector3 originalScale;
    private bool _alreadyDestroyed = false;
    private PrTopDownCamera _playerCamera;
    
    private void Start()
    {
        if (UsePhysicsToTranslate && !isGrenade)
        {
            GetComponent<Rigidbody>().AddForce(Vector3.forward * bulletSpeed * 10);
        }

        if (GameObject.Find("PlayerCamera"))
            _playerCamera = GameObject.Find("PlayerCamera").GetComponent<PrTopDownCamera>();

    }

    public void InitializePooling()
    {
        if (DetachOnDie.Length > 0 && !isGrenade)
        {
            _detachablePositions = new Vector3[DetachOnDie.Length];
            _detachableRotations = new Quaternion[DetachOnDie.Length];
            _detachableScales = new Vector3[DetachOnDie.Length];

            int i = 0;
            foreach (GameObject GO in DetachOnDie)
            {
                _detachablePositions[i] = GO.transform.localPosition;
                _detachableRotations[i] = GO.transform.localRotation;
                _detachableScales[i] = GO.transform.localScale;
                i = i + 1;

                //Object Pooling System
                GO.AddComponent<PrDestroyTimer>();
                GO.GetComponent<PrDestroyTimer>().UseObjectPooling = true;
                GO.GetComponent<PrDestroyTimer>().enabled = false;
            }
        }

        originalScale = this.transform.localScale;
    }


    public void ResetPooling()
    {
        _alreadyDestroyed = false;
        _useDefaultImpactFX = true;

        if (DetachOnDie.Length > 0 && !isGrenade)
        {
            int i = 0;
            foreach (GameObject GO in DetachOnDie)
            {
                GO.transform.parent = this.transform;
                GO.transform.localPosition = _detachablePositions[i];
                GO.transform.localRotation = _detachableRotations[i];
                GO.transform.localScale = _detachableScales[i];
                i = i + 1;
      
                GO.GetComponent<PrDestroyTimer>().enabled = false;
            }
        }
        if (GetComponentInChildren<TrailRenderer>())
        {
            GetComponentInChildren<TrailRenderer>().Clear();
        }
    }
    
    public void Update () {
        
        if (!UsePhysicsToTranslate)
        {
            if (bulletSpeed > 0.01f)
                bulletSpeed += bulletAccel * Time.deltaTime;

            transform.Translate(Vector3.forward * bulletSpeed);

            _fwd = transform.TransformDirection(Vector3.forward);
        }

        RaycastHit hit;

        if (Physics.Raycast(transform.position, _fwd, out hit, bulletSpeed * 2.0f)) {
            if (!_alreadyDestroyed && !hit.collider.CompareTag("Bullets") && !hit.collider.CompareTag("MainCamera"))
			    DestroyBullet(hit.normal, hit.point, hit.transform, hit.collider.gameObject, hit.collider.tag);
		}

        if (isGrenade)
        {
            if (explodeTimer > 0.0f)
                explodeTimer -= Time.deltaTime;
            else
                DestroyGrenade(Vector3.down, this.transform.position, this.transform);
        }
        else
        {
            timeToLive -= Time.deltaTime;
            if (timeToLive <= 0.0f)
            { 
                if (usePooling)
                    this.gameObject.SetActive(false);
                else
                    Destroy(this.gameObject);
            }
        }
    }


    private void DestroyGrenade(Vector3 hitNormal, Vector3 hitPos, Transform hitTransform)
    {
        DestroyImmediate(GetComponent<Rigidbody>());
        DestroyImmediate(GetComponent<Collider>());
        var explosivePos = transform.position;
        var colls = Physics.OverlapSphere(explosivePos, DamageRadius);
        foreach (var col in colls)
        {

            if (col.CompareTag("Destroyable"))
            {
                if (col.GetComponent<Rigidbody>())
                {
                    col.gameObject.GetComponent<Rigidbody>().AddExplosionForce(RadialForce, explosivePos, DamageRadius * 3);
                }

                col.SendMessage("BulletPos", explosivePos, SendMessageOptions.DontRequireReceiver);
                col.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                col.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
            }
            else if (col.CompareTag("Player"))
            {
                if (col.GetComponent<PrTopDownCharInventory>())
                {
                    if (!friendlyFire)
                    {
                        if (col.GetComponent<Rigidbody>())
                        {
                            col.gameObject.GetComponent<Rigidbody>().AddExplosionForce(RadialForce, explosivePos, DamageRadius * 3);
                        }
                        col.SendMessage("BulletPos", explosivePos, SendMessageOptions.DontRequireReceiver);
                        col.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                        col.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        if (col.GetComponent<Rigidbody>())
                        {
                            col.gameObject.GetComponent<Rigidbody>().AddExplosionForce(RadialForce, explosivePos, DamageRadius * 3);
                        }
                        col.SendMessage("BulletPos", explosivePos, SendMessageOptions.DontRequireReceiver);
                        col.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                        col.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
                    }
                    
                }
            }
            else if (col.CompareTag("NPC"))
            {
                if (col.GetComponent<Rigidbody>())
                {
                    col.gameObject.GetComponent<Rigidbody>().AddExplosionForce(RadialForce, explosivePos, DamageRadius * 3);
                }
                col.SendMessage("BulletPos", explosivePos, SendMessageOptions.DontRequireReceiver);
                col.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                col.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
            }


            else if (col.CompareTag("NPC"))
            {
                if (col.GetComponent<Rigidbody>())
                {
                    col.gameObject.GetComponent<Rigidbody>().AddExplosionForce(RadialForce, explosivePos, DamageRadius * 3);
                }
                col.SendMessage("BulletPos", explosivePos, SendMessageOptions.DontRequireReceiver);
                col.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                col.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        if (DefaultImpactFX && _useDefaultImpactFX)
        {
            var fx = Instantiate(DefaultImpactFX, hitPos + Vector3.up * 0.32f, Quaternion.identity);
            fx.transform.SetParent(ParticlePool.Instance.transform);
        }

        if (DefaultImpactDecal)
        {
            var bulletDecal = Instantiate(DefaultImpactDecal, hitPos, Quaternion.LookRotation(hitNormal)) as GameObject;
            bulletDecal.transform.localPosition += bulletDecal.transform.forward * 0.01f;
        }


        // if (_playerCamera)
        //     _playerCamera.ExplosionShake(cameraShakeFactor, cameraShakeDuration);

        if (DetachOnDie.Length > 0)
        {
            foreach (var GO in DetachOnDie)
            {
                GO.transform.parent = null;
                GO.AddComponent<PrDestroyTimer>();
                GO.GetComponent<PrDestroyTimer>().DestroyTime = 10f;
            }
        }

        Destroy(this.gameObject);
    }
    
    private void DestroyBullet(Vector3 hitNormal, Vector3 hitPos, Transform hitTransform, GameObject target, string hitTag)
	{
        _alreadyDestroyed = true;

        var explosivePos = transform.position;
        var colls = Physics.OverlapSphere(explosivePos, DamageRadius);
        target.SendMessage("SetAttacker", attacker, SendMessageOptions.DontRequireReceiver);
        if (!RadialDamage)
        {
            switch (hitTag)
            {
                case "Destroyable":
                {
                    target.SendMessage("BulletPos", transform.position, SendMessageOptions.DontRequireReceiver);
                    target.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                    if (generatesBloodDamage)
                    {
                        target.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
                        if (target.GetComponent<Rigidbody>())
                        {
                            target.GetComponent<Rigidbody>().AddForceAtPosition(hitNormal * -hitForce, hitPos);
                        }
                    }
                    
                    else
                        target.SendMessage("ApplyDamageNoVFX", Damage, SendMessageOptions.DontRequireReceiver);

                    break;
                }
                case "Player":
                {
                    target.SendMessage("BulletPos", transform.position, SendMessageOptions.DontRequireReceiver);
                    target.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                    if (generatesBloodDamage)
                    {
                        target.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
                        if (target.GetComponent<PrTopDownCharInventory>().DamageFX != null)
                        {
                            Instantiate(target.GetComponent<PrTopDownCharInventory>().DamageFX, hitPos, Quaternion.LookRotation(hitNormal));
                            _useDefaultImpactFX = false;
                        }
                    }
                    else
                        target.SendMessage("ApplyDamageNoVFX", Damage, SendMessageOptions.DontRequireReceiver);

                    break;
                }
                case "NPC":
                {
                    //Player.Instance.AddFactionPenalty();
                    target.SendMessage("BulletPos", transform.position, SendMessageOptions.DontRequireReceiver);
                    target.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                    if (generatesBloodDamage)
                    {
                        target.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
                        if (target.GetComponent<PrNPCAI>().damageVFX != null)
                        {
                            var fx = Instantiate(target.GetComponent<PrNPCAI>().damageVFX, hitPos, Quaternion.LookRotation(hitNormal));
                            fx.transform.SetParent(ParticlePool.Instance.transform);
                            _useDefaultImpactFX = false;
                        }
                    }
                    
                    else
                        target.SendMessage("ApplyDamageNoVFX", Damage, SendMessageOptions.DontRequireReceiver);

                    break;
                }
                default:
                {
                    if (hitTag == "NPC")
                    {
                        target.SendMessage("BulletPos", transform.position, SendMessageOptions.DontRequireReceiver);
                        target.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                        if (generatesBloodDamage)
                        {
                            target.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
                            if (target.GetComponent<PrNPCAI>().damageVFX != null)
                            {
                                var fx = Instantiate(target.GetComponent<PrNPCAI>().damageVFX, hitPos, Quaternion.LookRotation(hitNormal));
                                fx.transform.SetParent(ParticlePool.Instance.transform);
                                _useDefaultImpactFX = false;
                            }
                        }
                        else
                            target.SendMessage("ApplyDamageNoVFX", Damage, SendMessageOptions.DontRequireReceiver);
                    }
                    break;
                }
            }
        }
        else
        {
            if (target.GetComponent<Rigidbody>())
            {
                target.GetComponent<Rigidbody>().AddForceAtPosition(hitNormal * -hitForce, hitPos);
            }
 
            //Object Pooling Mode
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Collider>().enabled = false;

            foreach (var col in colls)
            {
                if (col.CompareTag("Destroyable"))
                {
                    if (col.GetComponent<Rigidbody>())
                    {
                        col.gameObject.GetComponent<Rigidbody>().AddExplosionForce(RadialForce, explosivePos, DamageRadius * 3);
                    }

                    col.SendMessage("BulletPos", explosivePos, SendMessageOptions.DontRequireReceiver);
                    col.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                    col.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);

                }
                else if (col.CompareTag("Player"))
                {
                    if (col.GetComponent<Rigidbody>())
                    {
                        col.gameObject.GetComponent<Rigidbody>().AddExplosionForce(RadialForce, explosivePos, DamageRadius * 3);
                    }
                    col.SendMessage("BulletPos", explosivePos, SendMessageOptions.DontRequireReceiver);
                    col.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                    col.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
                    
                }
                else if (col.CompareTag("NPC"))
                {
                    if (col.GetComponent<Rigidbody>())
                    {
                        col.gameObject.GetComponent<Rigidbody>().AddExplosionForce(RadialForce, explosivePos, DamageRadius * 3);
                    }
                    col.SendMessage("BulletPos", explosivePos, SendMessageOptions.DontRequireReceiver);
                    col.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                    col.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);
                }
                else if (col.CompareTag("NPC"))
                {
                    if (col.GetComponent<Rigidbody>())
                    {
                        col.gameObject.GetComponent<Rigidbody>().AddExplosionForce(RadialForce, explosivePos, DamageRadius * 3);
                    }
                    col.SendMessage("BulletPos", explosivePos, SendMessageOptions.DontRequireReceiver);
                    col.SendMessage("ApplyTempMod", temperatureMod, SendMessageOptions.DontRequireReceiver);
                    col.SendMessage("ApplyDamage", Damage, SendMessageOptions.DontRequireReceiver);

                }
            }
            //
            // if (_playerCamera)
            //     _playerCamera.ExplosionShake(cameraShakeFactor, cameraShakeDuration);
        }
        
		if (DefaultImpactFX && _useDefaultImpactFX)
        {
            var fx = Instantiate(DefaultImpactFX, hitPos, Quaternion.LookRotation(hitNormal));
            fx.transform.SetParent(ParticlePool.Instance.transform);
        }

		if (DefaultImpactDecal && hitTag != "NPC" && hitTag != "Player" && hitTag != "NPC")
		{
			var bulletDecal = Instantiate(DefaultImpactDecal, hitPos  , Quaternion.LookRotation( hitNormal) ) as GameObject;
			bulletDecal.transform.localPosition += bulletDecal.transform.forward * 0.01f;
			bulletDecal.transform.parent = target.transform;
		}

        if (DetachOnDie.Length > 0)
        {
            foreach (var GO in DetachOnDie)
            {
                GO.transform.parent = this.transform.parent;
                //Object Pooling System
                GO.GetComponent<PrDestroyTimer>().enabled = true;
                GO.GetComponent<PrDestroyTimer>().DestroyTime = 10f;
            }
        }

        if (usePooling)
            //Object Pooling Mode
            this.gameObject.SetActive(false);
        else
            Destroy(this.gameObject); 
    }
    
    public void OnCollisionEnter(Collision collision)
    {
        if (!UsePhysicsCollisions) return;
        if (!_alreadyDestroyed)
            DestroyBullet(collision.contacts[0].normal, collision.contacts[0].point, collision.transform, collision.gameObject, collision.transform.tag);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Fixed direction gun hardpoint
/// </summary>
public class GunHardpoint : MonoBehaviour {

    // Weapon mounted in this hardpoint
    public WeaponData mountedWeapon;
    protected float Range => mountedWeapon == null ? 0 : mountedWeapon.Range;

    public float ProjectileSpeed => mountedWeapon == null ? 0 : mountedWeapon.GetProjectileSpeed();

    public GameObject Barrel;
    public ParticleSystem muzzleEffect;
    // Projectile pool, for each gun
    [HideInInspector]
    public List<GameObject> projectilePool;
    [HideInInspector]
    public int projectilePoolSize = 20;
    // For beam weapons
    [HideInInspector]
    public Beam weaponBeam;
    [HideInInspector]
    public float timer;
    [HideInInspector]
    public Ship ship;
    [HideInInspector]
    public Station station;
    [HideInInspector]
    public AudioSource gunAudio;

    public void Awake()
    {
        gunAudio = GetComponent<AudioSource>();
        ship = GetComponentInParent<Ship>();
        station = GetComponentInParent<Station>();
        projectilePool = new List<GameObject>(projectilePoolSize);
        if (ship != null)
            RegisterHardpointWithShip();
        if (station != null)
            RegisterHardpointWithStation();
    }
    
    protected void Start()
    {
        timer = 0;

        if (mountedWeapon == null)
        {
            return; // Will be lazy-loaded later
        }
        mountedWeapon.InitWeapon(this);
    }

    protected void FixedUpdate () 
    {
        if (mountedWeapon)
            mountedWeapon.UpdateWeapon(this);
    }

    protected virtual void RegisterHardpointWithShip()
    {
        ship.Equipment.AddGun(this);
    }
    protected virtual void RegisterHardpointWithStation()
    {
        // Stations don't have guns, only turrets. So that code will handle this...
       // station.Equipment.AddGun(this);
    }
    public virtual void OnTriggerFireGun(bool isPrimary)
    {
        if (mountedWeapon == null)
            return;

        if (ship != null && ship == Ship.PlayerShip && Ship.PlayerShip.isPlayerControlled && Ship.IsShipInputDisabled)
            return;

        if (ship.doNotFireOnMe)
            return;
        // Projectile pool will not be initialized after jumping to the next system. 
        // Yes it will.
        if (isPrimary)
        {
            if ((mountedWeapon.Type == WeaponData.WeaponType.Projectile && projectilePool.Count <= 0) ||
                 (mountedWeapon.Type == WeaponData.WeaponType.Beam && weaponBeam == null))
            {
                SetWeapon(mountedWeapon);
            }
            if (mountedWeapon.Type == WeaponData.WeaponType.Beam || mountedWeapon.Type == WeaponData.WeaponType.Projectile)
            {
                mountedWeapon.OnTriggerFireWeapon(this, Barrel.transform.up);
            }
        }
        else
        {
            if (mountedWeapon.Type == WeaponData.WeaponType.Missile)
                mountedWeapon.OnTriggerFireWeapon(this, Barrel.transform.up);
        }
    }

    public void SetWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            // Reset all weapon data
            mountedWeapon = null;
            if(projectilePool != null && projectilePool.Count == projectilePoolSize) { 
                for (int i = 0; i < projectilePoolSize; i++)
                {
                    if (projectilePool[i])
                        GameObject.Destroy(projectilePool[i]);
                }
            }
            if (weaponBeam != null)
                GameObject.Destroy(weaponBeam.gameObject);
        }
        else
            weapon.InitWeapon(this);

        mountedWeapon = weapon;
    }

}

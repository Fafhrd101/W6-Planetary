using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/ProjectileWeaponData")]
public class ProjectileWeaponData : WeaponData{

    [Tooltip("Minimum time between shots")]
    public float ReloadTime;
    [Tooltip("Projectile speed in units per second")]
    public float ProjectileSpeed;
    [Tooltip("Damage per shot")]
    public int Damage;
    [Tooltip("Projectile prefab")]
    public GameObject Projectile;
    [Tooltip("Energy usage per shot")]
    public int EnergyDrain;

    private Ship owner;
    
    public override float GetProjectileSpeed()
    {
        return ProjectileSpeed;
    }
    public override float GetProjectileDamage()
    {
        return Damage;
    }
    public override string GetProjectileName()
    {
        return "";
    }
    public override void InitWeapon(GunHardpoint hardpoint)
    {
        owner = hardpoint.ship ? hardpoint.ship : null;
        // Use a monobehaviour to start a coroutine
        Player.Instance.StartCoroutine(ProjectileInstantiator(hardpoint));
    }

    /// <summary>
    /// This coroutine is used to spread the instantiation of projectiles through many frames.
    /// This will hopefully reduce lagspikes.
    /// </summary>
    private IEnumerator ProjectileInstantiator(GunHardpoint hardpoint)
    {
        if (hardpoint == null)
            yield return null;
        
        hardpoint.projectilePool = new List<GameObject>();

        for (int i = 0; i < hardpoint.projectilePoolSize; i++)
        {
            if (hardpoint == null)
                yield return null;
            GameObject proj = GameObject.Instantiate(Projectile, hardpoint.transform, true);
            // Mark if projectile belongs to player (to record kills and change rep)
            //proj.GetComponent<Projectile>().PlayerShot = hardpoint.ship.faction == Player.Instance.PlayerFaction;
            hardpoint.projectilePool.Add(proj);
            proj.SetActive(false);

            if (hardpoint.ship == null && hardpoint.station == null)
                break;

            if (hardpoint.ship)
                Physics.IgnoreCollision(hardpoint.ship.GetComponent<Collider>(), hardpoint.projectilePool[i].GetComponent<Collider>(), true);
            else if (hardpoint.station)
                Physics.IgnoreCollision(hardpoint.station.GetComponentInChildren<Collider>(), hardpoint.projectilePool[i].GetComponent<Collider>(), true);
            yield return null;
        }
        yield return null;
    }

    public override void OnTriggerFireWeapon(GunHardpoint hardpoint, Vector3 forwardVector)
    {
        if (hardpoint.timer <= 0)
        {
            Fire(hardpoint, forwardVector);
            hardpoint.timer = ReloadTime;
        }
    }

    public override void UpdateWeapon(GunHardpoint hardpoint)
    {
        if (hardpoint.timer > 0)
            hardpoint.timer -= Time.deltaTime;
    }

    public override void Fire(GunHardpoint hardpoint, Vector3 forwardVector)
    {
        // Check available energy
        if (hardpoint.ship && hardpoint.ship.Equipment.energyAvailable < this.EnergyDrain)
            return;

        forwardVector += new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f)*0.02f;
        if (hardpoint.ship && hardpoint.ship.faction != Player.Instance.playerFaction)
            forwardVector *= 2f;
        
        // Find first unused projectile
        for (var i = 0; i < hardpoint.projectilePoolSize; i++)
        {
            // if (hardpoint == null || hardpoint.projectilePool == null) break;
            // if (hardpoint.projectilePool[i].activeInHierarchy) continue;
            hardpoint.projectilePool[i].transform.position = hardpoint.Barrel.transform.position + forwardVector * 7;
            hardpoint.projectilePool[i].transform.rotation = hardpoint.Barrel.transform.rotation;
            hardpoint.projectilePool[i].SetActive(true); 
            
            var proj = hardpoint.projectilePool[i].GetComponent<Projectile>();
            proj.FireProjectile(forwardVector,ProjectileSpeed, Range, Damage, hardpoint.ship);
                
            // You can't hear other ships, and you can't see the muzzle
            if (hardpoint.ship == Ship.PlayerShip)
            {
                hardpoint.muzzleEffect.Play();
                hardpoint.gunAudio.Play();
            }

            if (hardpoint.ship)
                hardpoint.ship.Equipment.WeaponFired(EnergyDrain);

            return;
        }
    }
}

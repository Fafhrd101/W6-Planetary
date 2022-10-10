﻿using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/MissileWeaponData")]
public class MissileWeaponData : WeaponData
{
    [Tooltip("Minimum time between shots")]
    public float ReloadTime;
    [Tooltip("Projectile speed in units per second")]
    public float MissileSpeed;
    [Tooltip("Damage per missile")]
    public int Damage;
    [Tooltip("Projectile prefab")]
    public GameObject Projectile;
    [Tooltip("Color of the missile trail")]
    public Color TrailColor;
    [Tooltip("Ammunition ware name")]
    public string AmmoName;
    [Tooltip("Turn rate of projectile in degrees/second")]
    public float TurnRate;
    [Tooltip("Is the missile guided or dumbfire")]
    public bool IsGuided;

    public override float GetProjectileSpeed()
    {
        return MissileSpeed;
    }
    public override float GetProjectileDamage()
    {
        return Damage;
    }
    
    public override string GetProjectileName()
    {
        return AmmoName;
    }
    public override void InitWeapon(GunHardpoint hardpoint)
    {
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
        if (!AmmoAvailable(hardpoint))
            return;

        forwardVector += new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f) * 0.02f;
        if (hardpoint.ship.faction != Player.Instance.playerFaction)
            forwardVector *= 2f;

        // Create and launch projectile
        Missile missile = GameObject.Instantiate(Projectile).GetComponent<Missile>();
        var transform = missile.transform;
        transform.position = hardpoint.Barrel.transform.position + forwardVector * 7;
        transform.rotation = hardpoint.Barrel.transform.rotation;
        missile.transform.RotateAround(transform.position, hardpoint.Barrel.transform.right, -90);
        missile.FireProjectile(this, GetTarget(hardpoint), hardpoint.ship, hardpoint.ship.faction == Player.Instance.playerFaction);

        hardpoint.muzzleEffect.Play();
        hardpoint.gunAudio.Play();
        hardpoint.ship.Equipment.WeaponFired(1);
    }

    /// <summary>
    /// Get the current target of this ship
    /// </summary>
    private Transform GetTarget(GunHardpoint hardpoint)
    {
        if (hardpoint.ship.isPlayerControlled)
        {
            GameObject currentPlayerTarget = InputHandler.Instance.GetCurrentSelectedTarget();
            if (currentPlayerTarget != null)
                return currentPlayerTarget.transform;            
        }
        else
        {
            if (hardpoint.ship.AIInput.wayPointList.Count > 0)
                return hardpoint.ship.AIInput.wayPointList[0];
        }
        return null;
    }

    private bool AmmoAvailable(GunHardpoint hardpoint)
    {
        // Check available ammunition
        foreach (var holdItem in hardpoint.ship.ShipCargo.cargoContents)
        {
            if ( holdItem.itemName.Contains("Missile")/*holdItem.itemName == AmmoName*/ && holdItem.amount > 0)
            {
                holdItem.amount--;  // Will be fired
                return true;
            }
        }
        Debug.Log("No missiles in hold!");
        return false;
    }

}

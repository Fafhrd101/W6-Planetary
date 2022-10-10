using UnityEngine;
using System.Collections;

public class W6PickupWeapon : PrPickupObject {
   
    public enum Weapons
    {
        Melee = 0,
        Pistol = 1, 
        Rifle = 2, 
        Shotgun = 3,
        RocketLauncher = 4,
        Laser = 5,
        FireThrower = 6,
        IceThrower = 7
    }
    [Header("Type Weapon Settings")]
    public Weapons WeaponType;

    protected override void SetName()
    {
        //Debug.Log(weaponNames[(int)WeaponType]);
        if (weaponNames.Length > 0)
            itemName = weaponNames[(int)WeaponType];
    }
    protected override void PickupObjectNow(int ActiveWeapon)
    {

        if (player != null)
        {
            PrTopDownCharInventory PlayerInv = player.GetComponent<PrTopDownCharInventory>();
//print("weapon "+(int)WeaponType);
            PlayerInv.PickupWeapon((int)WeaponType);

        }

        base.PickupObjectNow(ActiveWeapon);
    }
   
   
}

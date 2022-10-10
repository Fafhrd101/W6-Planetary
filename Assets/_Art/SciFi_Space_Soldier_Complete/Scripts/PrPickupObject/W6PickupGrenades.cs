using UnityEngine;
using System.Collections;

public class W6PickupGrenades : PrPickupObject {

    /*public enum Ammo
    {
        Explosive = 1,
        //Incendiary = 2, 
        //Ice = 3, 
    }*/
    [Header("Type Ammo Settings")]
    //public Ammo LoadType = Ammo.Explosive;
    public int quantity = 1;

    protected override void SetName()
    {
        itemName = "Grenades x" + quantity;
    }

    protected override void PickupObjectNow(int activeWeapon)
    {

        if (player != null)
        {
            var playerInv = player.GetComponent<PrTopDownCharInventory>();
            playerInv.LoadGrenades(quantity);
        }

        base.PickupObjectNow(activeWeapon);
    }
}

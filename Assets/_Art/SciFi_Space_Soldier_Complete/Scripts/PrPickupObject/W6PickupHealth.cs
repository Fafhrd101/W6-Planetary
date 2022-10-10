public class W6PickupHealth : PrPickupObject {
    
    protected override void SetName()
    {
        itemName = "Health Pack";
    }

    protected override void PickupObjectNow(int activeWeapon)
    {

        if (player != null)
        {
            var playerInv = player.GetComponent<PrTopDownCharInventory>();

            playerInv.SetHealth(playerInv.health);
        }

        base.PickupObjectNow(activeWeapon);
    }
}

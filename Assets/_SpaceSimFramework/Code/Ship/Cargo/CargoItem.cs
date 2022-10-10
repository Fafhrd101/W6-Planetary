using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cargo container with a hold item, can be picked up by ships.
/// </summary>
public class CargoItem : MonoBehaviour {

    public HoldItem item;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Ship>() == Ship.PlayerShip)
        {
            if (other.gameObject.GetComponent<ShipCargo>() != null)
                other.gameObject.GetComponent<ShipCargo>().AddCargoItem(this);
            GameObject.Destroy(this.gameObject);
            TextFlash.ShowYellowText(item.amount+" "+item.itemName+" salvaged!");
            Progression.AddExperience(100);
            Player.Instance.salvageCollected++;
        }
    }

    public void InitCargoItem(HoldItem.CargoType type, int numOfItems, string itemName)
    {
        item = new HoldItem(type, itemName, numOfItems);

        var o = this.gameObject;
        o.name = itemName + " (" + numOfItems + ")";
        SectorNavigation.Cargo.Add(o);
    }

    private void OnDestroy()
    {
        SectorNavigation.Cargo.Remove(this.gameObject);
    }
}

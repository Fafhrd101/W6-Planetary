using System.Collections.Generic;
using System.Linq;
using SpaceSimFramework.Code.UI.HUD;
using UnityEngine;
using Random = UnityEngine.Random;


public class ShipCargo : MonoBehaviour {

    public static float CARGO_DROP_PROBABILITY = 0.1f, WEAPON_DROP_PROBABILITY = 0.07f;
    public GameObject CargoItemPrefab;
    public int cargoSize = 30, cargoOccupied = 0;
    public List<HoldItem> cargoContents;
    public List<KeyValuePair> visibleCargo = new List<KeyValuePair>();
 
    private Ship _ship;
    
	void Awake () {
        cargoContents = new List<HoldItem>();
        cargoSize = gameObject.GetComponent<Ship>().shipModelInfo.CargoSize;
        _ship = GetComponent<Ship>();
        
        // Generate random cargo items
        if (_ship.isPlayerControlled)
            return;

        while(true)
        {
            int item = Random.Range(0, Commodities.Instance.NumberOfWares);
            int amount = Random.Range(10, 30);

            if(cargoOccupied + amount <= cargoSize) { 
                AddWare(HoldItem.CargoType.Ware, Commodities.Instance.CommodityTypes[item].Name, amount);
            }
            else
            {
                break;
            }
        }

	}

    /// <summary>
    /// Invoked by the Ship script when owned ship has been destroyed. Drops loot.
    /// </summary>
    public void OnShipDestroyed()
    {
        // Drop random cargo items from the ones available
        foreach(HoldItem holdItem in cargoContents)
        {
            if(Random.Range(0f,1f) < CARGO_DROP_PROBABILITY)
            {
                Vector3 randomAddition = new Vector3(Random.Range(1, 5), Random.Range(1, 5), Random.Range(1, 5));
                // Eject item to a random location
                GameObject cargo = GameObject.Instantiate(
                    CargoItemPrefab,
                    transform.position+randomAddition,
                    Quaternion.identity);

                cargo.GetComponent<CargoItem>().InitCargoItem(holdItem.cargoType, holdItem.amount, holdItem.itemName);
            }
        }

        Ship ship = GetComponent<Ship>();
        foreach (var hardpoint in ship.Equipment.Guns)
        {
            if (hardpoint.mountedWeapon != null && Random.Range(0f, 1f) < WEAPON_DROP_PROBABILITY)
            {
                Vector3 randomAddition = new Vector3(Random.Range(1, 5), Random.Range(1, 5), Random.Range(1, 5));
                // Eject item to a random location
                GameObject cargo = GameObject.Instantiate(
                    CargoItemPrefab,
                    transform.position + randomAddition,
                    Quaternion.identity);

                cargo.GetComponent<CargoItem>().InitCargoItem(HoldItem.CargoType.Weapon, 1, hardpoint.mountedWeapon.name);
            }
        }
        foreach (var hardpoint in ship.Equipment.Turrets)
        {
            if (hardpoint.mountedWeapon != null && Random.Range(0f, 1f) < WEAPON_DROP_PROBABILITY)
            {
                Vector3 randomAddition = new Vector3(Random.Range(1, 5), Random.Range(1, 5), Random.Range(1, 5));
                // Eject item to a random location
                GameObject cargo = GameObject.Instantiate(
                    CargoItemPrefab,
                    transform.position + randomAddition,
                    Quaternion.identity);

                cargo.GetComponent<CargoItem>().InitCargoItem(HoldItem.CargoType.Weapon, 1, hardpoint.mountedWeapon.name);
            }
        }
    }
    
    /// Invoked when a ship picks up a cargo container.
    public void AddWare(HoldItem.CargoType type, string cargoName, int amount)
    {
        if (cargoOccupied < cargoSize)
        {
            if (cargoOccupied + amount <= cargoSize)
            {
                // Take all the cargo
                cargoOccupied += amount;
            }
            else
            {
                // Take as much as fits
                amount = cargoSize - cargoOccupied;
                cargoOccupied = cargoSize;
            }

            HoldItem cargoItem = GetCargo(cargoName);
            KeyValuePair cargoItemkvp = GetCargokvp(cargoName);
            if (cargoItem == null)
            {
                cargoContents.Add(new HoldItem(type, cargoName, amount));
                visibleCargo.Add(new KeyValuePair(cargoName, amount));
            }
            else
            {
                cargoItem.amount += amount;
                cargoItemkvp.val += amount;
            }

            if (Ship.PlayerShip != null && this.gameObject == Ship.PlayerShip.gameObject)
            {
                ConsoleOutput.PostMessage("Cargobay now contains " + cargoName + " (" + amount + ")");
                //TextFlash.ShowYellowText("Cargobay now contains " + name + " (" + amount + ")");
            }
        }
        else
        {
            if (this.gameObject == Ship.PlayerShip.gameObject)
            {
                ConsoleOutput.PostMessage("Cargobay full!", Color.yellow);
                TextFlash.ShowYellowText("Cargobay full!");
            }
        }
    }
    
    /// Invoked when a ship picks up a cargo container.
    public void AddCargoItem(CargoItem cargo)
    {
        AddWare(cargo.item.cargoType, cargo.item.itemName, cargo.item.amount);
    }
    
    /// Invoked to remove some of the cargo hold items.
    public void RemoveCargoItem(string itemName, int amount)
    {
        foreach (var cargoitem in cargoContents.Where(cargoitem => cargoitem.itemName == itemName))
        {
            if(cargoitem.amount <= amount || amount == 0)
            {
                // Remove all cargo of this type from hold
                cargoContents.Remove(cargoitem);
            }
            else
            {
                // Remove only some cargo of this type
                cargoitem.amount -= amount;
            }
            cargoOccupied -= amount;

            return;
        }
    }

    /// <summary>
    /// Invoked to remove all of the cargo hold items.
    /// </summary>
    public void RemoveCargo()
    {
        cargoContents = new List<HoldItem>();
        cargoOccupied = 0;
    }
    
    /// Whether the cargobay contains a specified type of cargo.
    private HoldItem GetCargo(string cargoType)
    {
        foreach (var cargoitem in cargoContents)
        {
            if (cargoitem.itemName == cargoType)
                return cargoitem;
        }
        return null;
    }

    private KeyValuePair GetCargokvp(string cargoType)
    {
        foreach (var cargoitem in visibleCargo)
        {
            if (cargoitem.key == cargoType)
                return cargoitem;
        }
        return null;
    }


}

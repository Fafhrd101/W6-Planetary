using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using StationWares = StationContentGenerator.StationWares;

[Serializable]
public class StationGoodsForSale
{
    public string itemName;
    public int itemPrice;
    public int amountOnHand;
    public int idealAmount; // affects current price
    public StationGoodsForSale(string name, int cost, int amount, int ideal){
        this.itemName = name;
        this.itemPrice = cost;
        this.amountOnHand = amount;
        this.idealAmount = ideal;
    }
}
[Serializable]
public class StationShipsForSale
{
    public GameObject ship;
    public int itemPrice;
    public StationShipsForSale(GameObject ship, int cost){
        this.ship = ship;
        this.itemPrice = cost;
    }
}
[Serializable]
public class StationEquipmentForSale
{
    public Equipment eq;
    public int itemPrice;
    public StationEquipmentForSale(Equipment eq, int cost){
        this.eq = eq;
        this.itemPrice = cost;
    }
}
[Serializable]
public class StationWeaponsForSale
{
    public WeaponData weapon;
    public int itemPrice;
    public StationWeaponsForSale(WeaponData weapon, int cost){
        this.weapon = weapon;
        this.itemPrice = cost;
    }
}
[Serializable]
public class KeyValuePair {
    public string key;
    public int val;
    public KeyValuePair(string key, int val){
        this.key = key;
        this.val = val;
    }
}
public class StationDealer : MonoBehaviour {
    public Dictionary<string, int> WarePrices;
    public List<KeyValuePair> VisibleWarePrices = new List<KeyValuePair>();
    public List<StationGoodsForSale> goodsForSale;
    public List<StationEquipmentForSale> equipmentForSale;
    public List<StationWeaponsForSale> weaponsForSale;
    public List<StationShipsForSale> shipsForSale;
    
    /// <summary>
    /// Generate cargo wares sold by the station
    /// </summary>
    /// <returns>A class containing all station data</returns>
    public StationWares GenerateStationData()
    {
        var stationWares = new StationWares
        {
            StationID = GetComponent<Station>().id
        };
        // Clear them as we probably are calling twice
        goodsForSale = new List<StationGoodsForSale>();
        VisibleWarePrices = new List<KeyValuePair>();
        
        // Three wares will be unavailable at each station - this is for the CargoDelivery mission (check the GetMissionData method)
        var unavailableWareIndices = new Vector3(Random.Range(0, Commodities.Instance.NumberOfWares), Random.Range(0, Commodities.Instance.NumberOfWares),Random.Range(0, Commodities.Instance.NumberOfWares));
        int price;
        stationWares.WaresForSale = new Dictionary<string, int>();
        // All stations need to generate wares, to keep the AI happy. We'll check for a dealer when displaying
        // Skip the last few; missiles, info, etc
        for (int i = 0; i < Commodities.Instance.CommodityTypes.Count-2; i++)
        {
            if ((int)unavailableWareIndices.x == i || (int)unavailableWareIndices.y == i || (int)unavailableWareIndices.z == i)
                continue;
            var amount = Random.Range(10, 100);
            var ideal = Random.Range(25, 150);
            price = Random.Range(Commodities.Instance.CommodityTypes[i].MinPrice,
                Commodities.Instance.CommodityTypes[i].MaxPrice);
            if (IsAbundant(Commodities.Instance.CommodityTypes[i].Name))
            {
                //print(Commodities.Instance.CommodityTypes[i].Name+" is too abundant");
                price = (int)(price * .75f);
            }
            else if (IsDesired(Commodities.Instance.CommodityTypes[i].Name))
            {
                //print(Commodities.Instance.CommodityTypes[i].Name+" is in short supply");
                price = (int)(price * 1.25f);
            }
            goodsForSale.Add(new StationGoodsForSale(Commodities.Instance.CommodityTypes[i].Name, price, amount,
                ideal));
            stationWares.WaresForSale.Add(Commodities.Instance.CommodityTypes[i].Name, price);
                
            // if (Commodities.Instance.CommodityTypes[i].Name == "Ore")
            //     print("Station will sell ore at "+price);
        }
        WarePrices = stationWares.WaresForSale;

        if (this.gameObject.GetComponent<Station>().loadout.hasWeaponDealer)
        {
            foreach (var weapon in ObjectFactory.Instance.Weapons)
            {
                // Remove 40% of the items
                if (Random.value < 0.4)
                    continue;
                price = (int) (weapon.Cost * Random.Range(.5f, 1.5f));
                weaponsForSale.Add(new StationWeaponsForSale(weapon, price));
            }
        }

        foreach (var eq in ObjectFactory.Instance.Equipment)
        {
            // Remove 40% of the items
            if (Random.value < 0.4)
                continue;

            price = (int)(eq.Cost * Random.Range(.5f, 1.5f));
            equipmentForSale.Add(new StationEquipmentForSale(eq, price));
        }

        if (this.gameObject.GetComponent<Station>().loadout.hasShipDealer)
        {
            foreach (var ship in ObjectFactory.Instance.Ships)
            {
                // Remove 40% of the items
                if (Random.value < 0.4)
                    continue;
                price = (int) (ship.GetComponent<Ship>().shipModelInfo.Cost * Random.Range(.5f, 1.5f));
                shipsForSale.Add(new StationShipsForSale(ship, price));
            }
        }
        
        foreach (var (key, val) in stationWares.WaresForSale.Select(x => (x.Key, x.Value)))
        {
            VisibleWarePrices.Add(new KeyValuePair(key, val));
            //Console.WriteLine($"{key} is {val} years old.");
        }
        
        return stationWares;
    }

    public void ClearStationData()
    {
        goodsForSale = new List<StationGoodsForSale>();
        weaponsForSale = new List<StationWeaponsForSale>();
        equipmentForSale = new List<StationEquipmentForSale>();
        shipsForSale = new List<StationShipsForSale>();
    }

    private bool IsAbundant(string wareName)
    {
        foreach (var var in SectorNavigation.Instance.abundantGoods)
            if (var == wareName)
                return true;
        return false;
    }
    private bool IsDesired(string wareName)
    {
        foreach (var var in SectorNavigation.Instance.desiredGoods)
            if (var == wareName)
                return true;
        return false;
    }
}

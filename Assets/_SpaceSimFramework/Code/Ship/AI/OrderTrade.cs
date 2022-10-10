using System.Collections;
using System.Collections.Generic;
using SpaceSimFramework.Code.UI.HUD;

using UnityEngine;

public class OrderTrade : Order
{
    private State state;
    private Station destinationStation;
    private string currentCargo = null;

    public bool InFinalApproach = false;
    private bool dockingRetry = false;
    private float timer; 

    private new enum State
    {
        None,           // Uninitialized
        AcquireCargo,   // Fly to station with good ware price and purchase it
        SellCargo       // Find a station where the ware will sell for a better price and sell it
    }

    public OrderTrade()
    {
        Name = "Trade";
        state = State.None;
    }

   
    public override void UpdateState(ShipAI controller)
    {
        if (dockingRetry)
        {
            timer += Time.deltaTime;
            if (timer > 3f) { 
                timer = 0;
                dockingRetry = false;
                Debug.Log("/// Retrying docking, queue was full");
                RequestDocking(controller);
            }
            return;
        }

        if(state == State.None)
        {
            if (controller.ship.ShipCargo.cargoOccupied > controller.ship.ShipCargo.cargoSize*0.8f)
                state = State.SellCargo;    // Ship is already loaded
            else
                state = State.AcquireCargo; // Ship is empty
        }
        if(state == State.AcquireCargo)
        {
            if (destinationStation == null)
            {  
                // No destination
                destinationStation = FindStationForCargoPurchase(controller);
                if (destinationStation != null) {
                    RequestDocking(controller);
                }
                else {
                    // ConsoleOutput.PostMessage(controller.name +
                    //    ": trade command end - no suitable traderun found");
                    // Debug.Log(controller.name +
                    //    ": trade command end - no suitable traderun found");
                    controller.FinishOrder();
                }
            }
            else { 
                FlyToStation(controller);
                
            }
        }
        if (state == State.SellCargo)
        {
            FlyToStation(controller);
        }

        ShipSteering.SteerTowardsTarget(controller);
    }

    private void SellCargoToStation(ShipAI controller)
    {
        int pricePerUnit = GetPricePerUnit();
        foreach (HoldItem cargoitem in controller.ship.ShipCargo.cargoContents)
        {
            if (cargoitem.itemName == currentCargo)
            {
                Debug.Log("/// Sold " + cargoitem.amount + " of " + currentCargo + " for "+ (cargoitem.amount * pricePerUnit));
                Player.Instance.credits += (cargoitem.amount * pricePerUnit);
                controller.ship.ShipCargo.RemoveCargoItem(currentCargo, cargoitem.amount);
                return;
            }
        }
    }

    private void BuyCargoFromStation(ShipAI controller)
    {
        int pricePerUnit = GetPricePerUnit();

        int freeCargo = controller.ship.ShipCargo.cargoSize - controller.ship.ShipCargo.cargoOccupied;
        if(freeCargo * pricePerUnit <= Player.Instance.credits)
        {
            controller.ship.ShipCargo.cargoContents.Add(new HoldItem(HoldItem.CargoType.Ware, currentCargo, freeCargo));
            Player.Instance.credits -= (freeCargo * pricePerUnit);
            Debug.Log("/// Loaded " + freeCargo + " of " + currentCargo + " for " + (freeCargo * pricePerUnit));
        }
        else
        {
            int amount = Player.Instance.credits / pricePerUnit;
            controller.ship.ShipCargo.cargoContents.Add(new HoldItem(HoldItem.CargoType.Ware, currentCargo, amount));
            Player.Instance.credits -= (amount * pricePerUnit);
            Debug.Log("/// Loaded "+amount+" of " + currentCargo + " for " + (amount * pricePerUnit));
        }
        
    }

    private int GetPricePerUnit()
    {
        Commodities.WareType cargo = Commodities.Instance.GetWareByName(currentCargo);
        StationDealer stationDealer = destinationStation.GetComponent<StationDealer>();

        int averageWarePrice = (cargo.MaxPrice + cargo.MinPrice) / 2;
        int pricePerUnit = stationDealer.WarePrices.ContainsKey(currentCargo)
            ? stationDealer.WarePrices[currentCargo]
            : averageWarePrice;

        return pricePerUnit;
    }

    private void RequestDocking(ShipAI controller)
    {
        GameObject[] dockWaypoints;
        try { 
            dockWaypoints = destinationStation.RequestDocking(controller.gameObject);
        }
        catch (DockingQueueException)
        {
            dockingRetry = true;
            return;
        }
        catch (DockingException) {
            controller.FinishOrder();
            return;
        }
        
        controller.wayPointList.Clear();

        foreach (var t in dockWaypoints)
            controller.wayPointList.Add(t.transform);

        controller.nextWayPoint = 0;
    }

    private IEnumerator DockingRetry()
    {
        yield return null;
    }

    private Station FindStationForCargoSale(ShipAI controller)
    {
        if (currentCargo == null)
            return null;

        Station station = null;
        float bestWareValue = 0;

        var knownStations = UniverseMap.Knowledge[SectorNavigation.CurrentSector].stations;

        foreach (var stationInfo in knownStations)
        {
            if (stationInfo.ID == destinationStation.id)    // Skip station at which ship's currently docked.
                continue;

            StationDealer stationDealer = SectorNavigation.GetStationByID(stationInfo.ID).GetComponent<StationDealer>();
            if(stationDealer.WarePrices[currentCargo] > bestWareValue)
            {
                bestWareValue = Commodities.Instance.GetWareSellRating(currentCargo, stationDealer.WarePrices[currentCargo]);
                station = stationDealer.GetComponent<Station>();
            }
        }

        Debug.Log("/// Found station for cargo dropoff: " + station.name + ", cargo: " + currentCargo + ", value:" + bestWareValue);
        return station;
    }

    private Station FindStationForCargoPurchase(ShipAI controller)
    {
        Station station = null;
        float bestWareValue = 0;

        var knownStations = UniverseMap.Knowledge[SectorNavigation.CurrentSector].stations;

        foreach (var stationInfo in knownStations)
        {
            
            if (SectorNavigation.GetStationByID(stationInfo.ID) == null)
            {
                Debug.LogWarning("Attempted to find "+stationInfo.ID+" but it's not in the DB!");
                return null;
            }

            var stationDealer = SectorNavigation.GetStationByID(stationInfo.ID).GetComponent<StationDealer>();
            if (stationDealer.WarePrices == null) continue;
            foreach(var ware in stationDealer.WarePrices)
            {
                float wareValue = Commodities.Instance.GetWareBuyRating(ware.Key, ware.Value);
                if (wareValue > bestWareValue && wareValue > 0.5f)
                {
                    bestWareValue = wareValue;
                    currentCargo = ware.Key;
                    station = stationDealer.GetComponent<Station>();
                }
            }
        }

        //Debug.Log("/// Found station for cargo purchase: " + station.name + ", cargo: " + currentCargo + ", value:" + bestWareValue);
        return station;
    }

    private void FlyToStation(ShipAI controller)
    {
        // Check angle to waypoint: first steer towards waypoint, then move to it
        if (OrderDock.FacingWaypoint(controller))
            OrderDock.MoveToWaypoint(controller);
        else
            controller.throttle = 0f;

        // Disable collision avoidance check in ShipAI
        if (controller.nextWayPoint > 0)
            InFinalApproach = true;
    }

    /// <summary>
    /// Station callback when ship has docked.
    /// </summary>
    public void PerformTransaction(ShipAI controller)
    {
        if(state == State.SellCargo)
        {
            SellCargoToStation(controller);
            state = State.AcquireCargo;
            destinationStation = null;  // Trade run complete
            return;
        }
        if (state == State.AcquireCargo)
        {
            BuyCargoFromStation(controller);
            state = State.SellCargo;

            destinationStation = FindStationForCargoSale(controller);
            if (destinationStation != null)
            {
                RequestDocking(controller);
            }
            else
            {
                ConsoleOutput.PostMessage(controller.name +
                   ": trade command error - no profitable sale location");
                Debug.Log(controller.name +
                   ": trade command end - no profitable sale location");
                controller.FinishOrder();
            }

            return;
        }
    }

    public override void Destroy()
    {
        //Debug.Log("/// Order Trade terminated");
    }
}

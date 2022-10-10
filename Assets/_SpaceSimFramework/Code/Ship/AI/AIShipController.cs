using System.Collections.Generic;
using UnityEngine;

public class AIShipController : MonoBehaviour {

    public float checkInterval = 3f;
    private float _timer = 0;

    void Update()
    {
        _timer += Time.deltaTime;

        if(_timer > checkInterval)
        {
            _timer = 0;

            GameObject[] aiShips = SectorNavigation.Ships.ToArray();

            // Check each ship to see if one has finished its order
            foreach (GameObject ship in aiShips)
            {
                if (ship == null)
                    continue;
                var currentShip = ship.GetComponent<Ship>();
                if (currentShip.faction == Player.Instance.playerFaction)
                    continue;

                if (currentShip.AIInput.CurrentOrder == null)     // give new order
                    IssueOrder(ship, "", null);
            }
        }
    }
 
    /// <summary>
    /// Issue an order to a ship. The spawn location (jumpgate or base) of the ship is given
    /// so that the given destination is not the same as the spawn location.
    /// </summary>
    /// <param name="ship">Ship to be given an order</param>
    /// <param name="debugText">Internal debug text for logging</param>
    /// <param name="spawnPosition">Jumpgate if ship has spawned at a gate, station if ship has undocked</param>
    /// <returns></returns>
    public static string IssueOrder(GameObject ship, string debugText, GameObject spawnPosition)
    {
        ShipAI shipAI = ship.gameObject.GetComponent<ShipAI>();

        var rand = Random.Range(0, 6);
        if (IsHostile(ship)) rand = 0; // raiders always attack
        if (spawnPosition) rand = 5;
        
        switch (rand)
        {
            case 0:
            case 1:// Attack all enemies
                shipAI.AttackAll();
                debugText += ", order: AttackAll";
                break;
            case 2: // Dock
                List<GameObject> dockables;
                // Don't leave sector until you've been here awhile
                dockables = shipAI.ageInMin > 2 ? SectorNavigation.GetDockableObjects() : SectorNavigation.GetTradeableObjects();
                
                if(spawnPosition)
                    dockables.Remove(spawnPosition);  // Dont head back to where you started
                for (int i = 0; i < dockables.Count; i++)
                {
                    // Remove station if it is of an enemy faction
                    if (dockables[i].CompareTag("Station"))
                        if (ship.GetComponent<Ship>().faction.RelationWith(dockables[i].GetComponent<Station>().faction) < 0)
                        {
                            dockables.RemoveAt(i);
                            i--;
                        }
                }
            
                if (dockables.Count > 0)
                {
                    var dockTarget = dockables[Random.Range(0, dockables.Count)];
                    //print("Choosing to dock at "+dockTarget);
                    shipAI.DockAt(dockTarget);
                    shipAI.target = dockTarget.transform;
                    debugText += ", order: DockAt " + dockTarget.name;
                }
                else
                {
                    shipAI.PatrolPath(SectorNavigation.GetPatrolWaypoints());
                    debugText += ", order: Patrol";
                }
            
                break;
            case 3: // Trade
                List<GameObject> tradeables = SectorNavigation.GetTradeableObjects();
                if(spawnPosition)
                    tradeables.Remove(spawnPosition);  // Dont head back to where you started
                for (int i = 0; i < tradeables.Count; i++)
                {
                    // Remove station if it is of an enemy faction
                    if (tradeables[i].CompareTag("Station"))
                        if (ship.GetComponent<Ship>().faction.RelationWith(tradeables[i].GetComponent<Station>().faction) < 0)
                        {
                            tradeables.RemoveAt(i);
                            i--;
                        }
                }

                if (tradeables.Count > 0)
                {
                    var tradeTarget = tradeables[Random.Range(0, tradeables.Count)];
                    //print("Choosing to trade at "+tradeTarget);
                    //shipAI.DockAt(tradeTarget);
                    shipAI.AutoTrade();
                    shipAI.target = tradeTarget.transform;
                    debugText += ", order: TradeAt " + tradeTarget.name;
                }
                else
                {
                    shipAI.PatrolPath(SectorNavigation.GetPatrolWaypoints());
                    debugText += ", order: Patrol";
                }

                break;
            case 4: // Idle
                debugText += ", order: Idle";
                shipAI.Idle();
                break;
             default: // Patrol
                shipAI.PatrolPath(SectorNavigation.GetPatrolWaypoints());
                debugText += ", order: Patrol";
                break;
        }

        return debugText;
    }

    private static bool IsHostile(GameObject ship)
    {
        if (ship.GetComponent<Ship>().faction == ObjectFactory.Instance.GetFactionFromName("Raider"))
            return true;
        return false;
    }
}

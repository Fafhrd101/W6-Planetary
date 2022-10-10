﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FactionBase
{
    Friendly,
    Neutral,
    Hostile,
    Player
}

[CreateAssetMenu(menuName = "Faction")]
public class Faction : ScriptableObject {

    public FactionBase baseFaction;
    public Faction[] friendly, hostile;
    public readonly Dictionary<Faction, float> Cache = new Dictionary<Faction, float>();
    public ShipLoadout[] loadouts;
    public StationLoadout[] stations;
    private float defaultDisposition = 0;
    public Gradient trailColoring;
    
    /// <summary>
    /// Returns relation of this faction with a given faction.
    /// </summary>
    /// <param name="otherFaction">The faction to compare with</param>
    /// <returns>1 if friendly, -1 if hostile, default disposition otherwise</returns>
    public float RelationWith(Faction otherFaction)
    {
        if (Cache.ContainsKey(otherFaction))
            return Cache[otherFaction];

        float output = defaultDisposition;

        foreach(Faction faction in friendly)
        {
            if (faction == otherFaction)
                return 1;
        }

        foreach (Faction faction in hostile)
        {
            if (faction == otherFaction)
                return -1;
        }

        Cache.Add(otherFaction, output);

        return output;
    }

    /// <summary>
    /// Gets the marker color of the target depending on the requestee's faction.
    /// </summary>
    /// <param name="target">Target gameobject</param>
    /// <returns>Red, green or white</returns>
    public Color GetTargetColor(GameObject target)
    {
        Faction targetFaction = null;

        if (target.CompareTag("Waypoint"))
            return Color.magenta;
        // Check if target is a mission target
        if(target.CompareTag("Ship") && MissionControl.CurrentJob != null)
        {
            if (MissionControl.CurrentJob.Type == Mission.JobType.Assassinate)
            {
                // If mission target, mark it
                if (target == ((Assassination)MissionControl.CurrentJob).Target)
                    return Color.magenta;
            }
            else if (MissionControl.CurrentJob.Type == Mission.JobType.Patrol)
            {
                // If target ship is enemy to employer faction, mark it
                if (MissionControl.CurrentJob.Employer.GetRelationColor(
                    target.GetComponent<Ship>().faction) == Color.red)
                    return Color.magenta;
            }
        }

        if (target.CompareTag("Ship"))
        {
            Ship ship = target.GetComponent<Ship>();
            if(Ship.PlayerShip == ship)
            {
                return Color.yellow;
            }
            targetFaction = ship.faction;
        }
        else if (target.CompareTag("Station"))
        {
            Station station = target.GetComponent<Station>();
            targetFaction = station.faction;
        }

        return GetRelationColor(targetFaction);
    }

    public Color GetRelationColor(Faction other)
    {
        float relation = 0;

        // Cyan for player owned ships
        if (other != null && other == Ship.PlayerShip.faction)
            return Color.cyan;

        relation = other == null ? 0 : RelationWith(other);

        if (relation > 0.5)
            return Color.green;
        else if (relation < 0)
            return Color.red;
        else
            return Color.cyan;
    }

    // Gets a random ship loadout
    // public ShipLoadout GetRandomLoadout(string modelName)
    // {
    //     var possibleLoadouts = Array.FindAll(Loadouts, loadout => loadout.ShipModel == modelName);
    //     return possibleLoadouts.Length == 0 ? null : possibleLoadouts[UnityEngine.Random.Range(0, possibleLoadouts.Length)];
    // }

}

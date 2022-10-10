using System.Collections.Generic;
using UnityEngine;

public class StationContentGenerator : MonoBehaviour
{
    public class StationWares
    {
        public string StationID;
        public Dictionary<string, int> WaresForSale;
    }
    /// <summary>
    /// Ensures station dealers are populated with appropriate wares,
    /// taking into account station faction
    /// </summary>
    /// <param name="currentSector"></param>
    /// <param name="stations">List of stations in the current sector</param>
    /// 
    /// Usually called twice. Once immediately upon loading the scene, and again when we're actually ready for it.
    public static void OnSectorChanged(Vector2 currentSector, List<GameObject> stations)
    {
        foreach (var t in stations)
        {
            t.GetComponent<StationDealer>().GenerateStationData();
        }
    }
}
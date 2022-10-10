using DunGen;
using DunGen.DungeonCrawler;
using Npc_AI;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProductionZone : MonoBehaviour
{
    private PlanetaryBaseSetup _setup;

    [TextArea] public string note =
        "This zone will transform itself into 1 of 6 types, then it will load workers for such";
    public GameObject[] resourceAreas;
    public int rssPerDayPerJob;
    public int localJobs;
    public int rssStored = 0;
    private DayAndNightControl _dayAndNightControl;
    private RuntimeDungeon _generator;
    public int active;
    //public int tile;
    private System.Random RandomStream { get; set; }
    
    private void Start()
    {
        _setup = FindObjectOfType<PlanetaryBaseSetup>();
        _dayAndNightControl = FindObjectOfType<DayAndNightControl>();
        _generator = _setup.GetComponent<RuntimeDungeon>();
        // _dayAndNightControl.OnDayPassedHandler += OnDayPassedListener;
        _dayAndNightControl.OnHourPassedHandler += OnHourPassedListener;
        
        foreach (var area in resourceAreas)
        {
            area.SetActive(false);
        }
        
        RandomStream = new System.Random(_generator.seedUsed);

        //active = Random.Range(0, 3);
        active = RandomStream.Next(0, 2); // no miners for now
        resourceAreas[active].SetActive(true);
        name = "PO" + resourceAreas[active].name;

        _setup.totalResourceZones.Add(this.transform);
    }

    // private void OnDayPassedListener()
    // {
    //     rssStored += rssPerDayPerJob * localJobs;
    //     // Temporarily convert tonnage to credits, then let NFTScribe know about it
    //     Player.Instance.credits += rssStored;
    // }
    
    private void OnHourPassedListener()
    {
        //print("Hour passed");
        rssStored += rssPerDayPerJob * localJobs / 24;
        // Temporarily convert tonnage to credits, then let NFTScribe know about it
        Player.Instance.credits += rssStored;
    }
}

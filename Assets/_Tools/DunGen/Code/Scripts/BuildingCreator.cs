using System.Collections.Generic;
using DunGen.DungeonCrawler;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuildingCreator : MonoBehaviour
{
    [TextArea] public string note = "We create the building; " +
                                    "first one for each merchant type, " +
                                    "then a home for each merchant," +
                                    "then make remainders empty." +
                                    "We tell the spawner if we need a merchant.";
    
    [Tooltip("We'll create one for each registered type")]
    public string storeType;
    public GameObject signHolder;
    private int _numSigns = 0;
    
    // remove this stupid shit
    [Tooltip("We're using only one sign, do not change store type.")]
    public bool lockType = false;
    
    public POI poi;
    public Transform interactPosition;
    private PlanetaryBaseSetup _setup;
    // [SerializeField]
    // [Tooltip("An image that's rendered to the minimap, if desired")]
    // private GameObject minimapIcon = null;
    public List<GameObject> itemListTemp;
    public GameObject[] signs;
    [Tooltip("Matches the home sign value")]
    public int HOMESIGN;
    public int cells;
    public List<GameObject> residents;
    
    private void Start()
    {
        _setup = GameObject.FindObjectOfType<PlanetaryBaseSetup>();

        foreach (var one in signs)
        {
            one.SetActive(false);
            _numSigns++;
        }
        // One celled buildings are always homes or empty
        if (cells == 1)
        {
            if (_setup.totalHomes.Count >= _setup.merchants.Count)
            {
                // Too many homes, turn off entire building
                //baseCreator.SetActive(false);
                _setup.totalEmptyBuildings.Add(interactPosition);
                return;
            }
            _setup.totalHomes.Add(interactPosition);
            foreach (var one in signs)
                one.SetActive(false);
            signHolder.transform.GetChild(HOMESIGN).gameObject.SetActive(true);
            return;
        }

        var active = _setup.merchants.Count+1;//Random.Range(1, _numSigns);
        if (active > _numSigns)
            return;
        if (lockType)
            active = 0;
        
        signHolder.transform.GetChild(active).gameObject.SetActive(true);
        var baseStr = signHolder.transform.GetChild(active).gameObject.name;
        storeType = baseStr;
        if (poi != null)
            poi.myName += storeType;
        
        _setup.GetComponent<NPC_Spawner>().SpawnMerchant(storeType, poi.transform, interactPosition);
    }
}

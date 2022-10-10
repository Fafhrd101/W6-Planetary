using DunGen.DungeonCrawler;
using UnityEngine;

public class POI : MonoBehaviour
{
    private PlanetaryBaseSetup _setup;
    public string myName;
    [Tooltip("If we're interacting here, what animation do we play?")]
    public string action;
    private void Start ()
    {
        _setup = FindObjectOfType<PlanetaryBaseSetup>();
        _setup.POIs.Add(this);
        if (name == "")
            myName = gameObject.name;
    }
 
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC") && other.GetComponent<PrNPCAI>())
        {
            other.GetComponent<PrNPCAI>().interactingWithPoi = this;
            //print(other.gameObject.name+" is interacting with a POI");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC") && other.GetComponent<PrNPCAI>())
            other.GetComponent<PrNPCAI>().interactingWithPoi = null;
    }
}

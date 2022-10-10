using DunGen.DungeonCrawler;
using UnityEngine;

public class NPCWorkLocation : MonoBehaviour
{
    public ProductionZone productionZone;
    [Tooltip("If we got a blue guy, he's the temp")]
    public GameObject tempToRemove;
    public PrNPCAI.NpcSubType subType;
    public int specialization;
    public GameObject leftToolRequired;
    public GameObject rightToolRequired;
    
    private PlanetaryBaseSetup _setup; 
    private Transform _leftHand;
    private Transform _rightHand;
    private Animator _animator;

    private void Start()
    {
        var npcSpawned = NPC_Spawner.Instance.SpawnWorker(transform);
        npcSpawned.transform.localScale = new Vector3(0.66f, 0.66f, 0.66f);
        npcSpawned.GetComponent<PrNPCAI>().subType = subType;
        npcSpawned.GetComponent<PrNPCAI>().specialization = specialization;
        npcSpawned.GetComponent<PrNPCAI>().home = transform;
        npcSpawned.GetComponent<PrNPCAI>().work = transform;
        npcSpawned.name = npcSpawned.name.Replace("Unused", npcSpawned.GetComponent<PrNPCAI>().subType.ToString());
        _rightHand = npcSpawned.GetComponent<PrNPCAI>().rightHand;
        _leftHand = npcSpawned.GetComponent<PrNPCAI>().leftHand;
        _animator = npcSpawned.GetComponent<Animator>();
        if (tempToRemove != null)
        {
            var anim2 = tempToRemove.GetComponent<Animator>().runtimeAnimatorController;
            _animator.runtimeAnimatorController = anim2;
            tempToRemove.SetActive(false);
        }
        if (leftToolRequired != null && _leftHand != null)
        {
            var instWeapon = Instantiate(leftToolRequired, _leftHand.position, _leftHand.rotation);
            instWeapon.transform.parent = _leftHand;
            instWeapon.transform.localRotation = Quaternion.Euler(0, -90, -90);
        }
        if (rightToolRequired != null && _rightHand != null)
        {
            var instWeapon = Instantiate(rightToolRequired, _rightHand.position, _rightHand.rotation);
            instWeapon.transform.parent = _rightHand;
            instWeapon.transform.localRotation = Quaternion.Euler(0, -90, -90);
        }
        
        npcSpawned.transform.position = this.transform.position;
        
        _setup = GameObject.FindObjectOfType<PlanetaryBaseSetup>();
        _setup.totalJobs.Add(this.transform);
        productionZone.localJobs++;
    }
}

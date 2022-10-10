using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrPlayerInfo : MonoBehaviour {

    public static PrPlayerInfo player1;

    [Header("Player Controller Variables")]
    public int playerNumber = 0;
    public Vector3 lastPlayerPosition = Vector3.zero;
    
    [Header("Player Inventory Variables")]
    public string playerName = "none";
    public int lives = 0;
    public int health = 100;
    public int actualHealth = 100;
    public int maxWeaponCount = 2;
    public int[] weapons;

    public GameObject[] grenadeType;
    public int grenades;

    [Header("Objectives Variables")]
    public bool loadPrevSettings = false;
    public int lastObjectiveActive = 0;

    void Awake()
    {
        if (player1 == null)
        {
            DontDestroyOnLoad(gameObject);
            player1 = this;
        }
        else if (player1 != this)
        {
            Destroy(gameObject);
        }
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

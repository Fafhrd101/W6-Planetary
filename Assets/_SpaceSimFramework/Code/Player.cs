using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceSimFramework.Code.UI.HUD;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Contains player data at runtime, including a list of in-sector (spawned) and 
/// out-of-sector (stored) ships. 
/// </summary>
public class Player : MonoBehaviour {

    public string serverName;
    public Vector2 currentSector = new Vector2(0,0);
    public Vector2 previousSector = new Vector2(0,0);
    public Vector3 previousPosition;
    public int credits;
    public int experience;
    public int level;
    public int shipsDestroyed;
    public int freightersDestroyed;
    public int stationsDestroyed;
    public int salvageCollected;
    public int raidersKilledByPlayer;
    public int raidersKilledByOthers;
    public int pilotIconNumber;
    public Sprite pilotIcon;
    
    [Header("Inventories")]
    public GameObject mainInv;
    public GameObject shipInv;
    public GameObject bankInv;
    
    // <Faction, (Ship Kills, Station Kills)>
    public Dictionary<Faction, Vector3> Kills;
    public List<GameObject> Ships;
    public Faction playerFaction;
    [HideInInspector]
    public bool dontDestroy = false;

    [HideInInspector]
    public bool inputDisabled = false;
    
    // OOS = Out Of Sector
    public List<ShipDescriptor> OOSShips
    {
        get { return _oosShips ??= new List<ShipDescriptor>(); }
    }
    private List<ShipDescriptor> _oosShips;

    // Keeps your original reputation 
    private Dictionary<Faction, float> _playerRelationsBackup = null;

    /// <summary>
    /// Stores data for player owned ships in other (not player) sectors.
    /// Out-of-sector ships are not simulated.
    /// </summary>
    public class ShipDescriptor
    {
        public string modelName;
        public Vector2 Sector;
        public string StationDocked;
        public Vector3 Position;
        public Quaternion Rotation;
        public float Armor;
        public WeaponData[] Guns;
        public WeaponData[] Turrets;
        public Equipment[] MountedEquipment;
        public HoldItem[] CargoItems;
    }
    // Current instance management
    public static Player Instance { get; private set; }

    private void Awake ()
    {
        if (Instance != null)
            DestroyImmediate(Instance.gameObject);

        Instance = this;
    }
 
    private void OnDestroy ()
    {
        Instance = null;
    }

    private void OnEnable()
    {
        if (SceneManager.GetActiveScene().name != "EmptyPlanet")
            Ship.PlayerShip.enabled = true;
    }

    private void Start ()
    {
        if (dontDestroy)
            DontDestroyOnLoad(this);
        //serverName = PlayArcadeIntegration.Instance.playerName;
        
        // Assign our data holders
        var ships = GameObject.FindGameObjectsWithTag("Ship");
        Ships = new List<GameObject>();
        foreach (GameObject ship in ships) 
            if (ship.GetComponent<Ship>().faction == playerFaction)
                Ships.Add(ship);
		if (Kills == null)
        {
            Kills = new Dictionary<Faction, Vector3>();
            foreach (Faction f in ObjectFactory.Instance.Factions)
                Kills.Add(f, Vector3.zero);
        }
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "EmptyPlanet")
        {
            return;
        }
        
        if (Ship.PlayerShip.isDestroyed)
            Invoke(nameof(GameOver), 2);
        else if (SceneManager.GetActiveScene().name != "MainMenu")
            Progression.totalEnemiesAlive = SectorNavigation.GetTotalNPCShips();
        else
        {
            Progression.Experience =
                Progression.Kills = Progression.Level = Progression.StationKills = 0;
            experience = shipsDestroyed = stationsDestroyed = freightersDestroyed = 0;
            credits = 5000;
        }
        
        // if (Input.GetKey(KeyCode.LeftShift))
        // {
        //     if (Input.GetKeyDown(KeyCode.S))
        //     {
        //         SaveGame.SaveAutosave(SectorNavigation.UnsetSector);
        //     }
        // if (Input.GetKeyDown(KeyCode.F))
        // {
        //     DisplayFactionRelations();
        // }
        // } 
    }
    public void GameOver()
    {
        CanvasViewController.Instance.FlightCanvas.gameObject.SetActive(false);
        CanvasViewController.Instance.Hud.gameObject.SetActive(false);
        CanvasViewController.Instance.gameOverSummaryCanvas.SetActive(true);
        Ship.PlayerShip.isDestroyed = true;
        Ship.IsShipInputDisabled = true;
    }
    #region faction relations

    public float factionPenalty = 0.2f, dampeningFactor = 4f;

    /// <summary>
    /// Decrease rep with faction.
    /// </summary>
    public float AddFactionPenalty(Faction otherfaction)
    {
        float rep = playerFaction.RelationWith(otherfaction);

        SetTemporaryFactionHostility(otherfaction);

        rep = Mathf.Clamp(rep - factionPenalty, -1, 1);
        if (_playerRelationsBackup != null)
        {
            _playerRelationsBackup[otherfaction] = Mathf.Clamp(_playerRelationsBackup[otherfaction] - factionPenalty, -1, 1);
        }

        foreach (var currFac in ObjectFactory.Instance.Factions)
        {
            if (currFac == playerFaction || currFac == otherfaction)
                continue;

            var relationWithOtherFaction = otherfaction.RelationWith(currFac);
            var repChange = (factionPenalty * -relationWithOtherFaction) / dampeningFactor;
            currFac.Cache[playerFaction] = Mathf.Clamp(currFac.RelationWith(playerFaction) + repChange, -1, 1);
            playerFaction.Cache[currFac] = Mathf.Clamp(playerFaction.RelationWith(currFac) + repChange, -1, 1);
            if(_playerRelationsBackup != null)
            {
                _playerRelationsBackup[currFac] = Mathf.Clamp(_playerRelationsBackup[currFac] + repChange, -1, 1);
            }
        }

        //DisplayFactionRelations();
        return rep;
    }


    private void SetTemporaryFactionHostility(Faction otherfaction)
    {
        // Copy relations, but dont overwrite 
        if(_playerRelationsBackup == null)
        {
            _playerRelationsBackup = new Dictionary<Faction, float>();
            foreach (Faction f in ObjectFactory.Instance.Factions)
            {
                _playerRelationsBackup.Add(f, playerFaction.RelationWith(f));
            }
        }

        otherfaction.Cache[playerFaction] = playerFaction.Cache[otherfaction] = -1;

        foreach (var currFac in ObjectFactory.Instance.Factions)
        {
            if (currFac == playerFaction || currFac == otherfaction)
                continue;

            var relationWithOtherFaction = otherfaction.RelationWith(currFac);
            if (!(relationWithOtherFaction > 0)) continue;
            // Turn it hostile too
            currFac.Cache[playerFaction] = -1;
            playerFaction.Cache[currFac] = -1;
        }

    }

    /// <summary>
    /// Returns the space-separated faction relation coefficients for game saving
    /// </summary>
    /// <returns></returns>
    public string GetReputationString()
    {
        var rep = "";

        if (_playerRelationsBackup == null)
        {
            foreach (var f in ObjectFactory.Instance.Factions)
            {
                if (f != Player.Instance.playerFaction)
                    rep += playerFaction.RelationWith(f) + " ";
            }
        }
        else
        {
            foreach (var f in ObjectFactory.Instance.Factions)
            {
                if (f != Player.Instance.playerFaction)
                    rep += _playerRelationsBackup[f] + " ";
            }
        }

        return rep;
    }

    /// <summary>
    /// Returns the list of faction relation coeffiecients for game saving
    /// </summary>
    /// <returns></returns>
    public float[] GetReputations()
    {
        float[] rep = new float[ObjectFactory.Instance.Factions.Length-1];

        if (_playerRelationsBackup == null)
        {
            for (var i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
            {
                var f = ObjectFactory.Instance.Factions[i];

                if (f != Player.Instance.playerFaction)
                    rep[i] = playerFaction.RelationWith(f);
            }
        }
        else
        {
            for (var i = 0; i < ObjectFactory.Instance.Factions.Length; i++)
            {
                var f = ObjectFactory.Instance.Factions[i];

                if (f != Player.Instance.playerFaction)
                    rep[i] = playerFaction.RelationWith(f);
            }
        }

        return rep;
    }

    /// <summary>
    /// Debug function.
    /// </summary>
    private void DisplayFactionRelations()
    {
        var relationsText = "";

        foreach (var otherFaction in ObjectFactory.Instance.Factions)
        {
            relationsText += otherFaction.name + " = " + playerFaction.RelationWith(otherFaction) + "; ";
        }

        //Debug.Log(relationsText);
        ConsoleOutput.PostMessage(relationsText);
    }

    #endregion faction relations
}

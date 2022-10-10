using System;
using SpaceSimFramework.Code.UI.HUD;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ties all the primary ship components together.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ShipPhysics))]
[RequireComponent(typeof(ShipPlayerInput))]
[RequireComponent(typeof(ShipCargo))]
[RequireComponent(typeof(ShipEquipment))]
[RequireComponent(typeof(ShipAI))]
public class Ship : MonoBehaviour
{
    public static event EventHandler ShipDestroyedEvent;

    #region ship components
    // Ship cargo
    public ShipCargo ShipCargo => _shipCargo;
    private ShipCargo _shipCargo;

    // Player controls
    public ShipPlayerInput PlayerInput => _playerInput;
    private ShipPlayerInput _playerInput;

    // Artificial intelligence controls
    public ShipAI AIInput => _aiInput;
    private ShipAI _aiInput;

    // Weapon systems
    public ShipEquipment Equipment
    {
        get
        {
            if (_shipEquipment == null)
                _shipEquipment = GetComponent<ShipEquipment>();
            return _shipEquipment;
        }
    }
    private ShipEquipment _shipEquipment;

    // Ship rigidbody physics
    public ShipPhysics Physics => _physics;
    private ShipPhysics _physics;

    // Getters for external objects to reference things like input.
    public bool UsingMouseInput
    {
        get => _playerInput.useMouseInput;
        set => _playerInput.useMouseInput = value;
    }
    public bool InSupercruise {
        get => inSupercruise;
        set => inSupercruise = value;
    }
   [Header("Game Set Bools")] 
    public bool inSupercruise = false;
    public bool keyboardMode = true;//mouse, keyboard
    //[HideInInspector]
    [Tooltip("Hail Mary fix")]
    public bool doNotFireOnMe = false;
    //[HideInInspector]
    public bool isSpeedLimited = false;
    public static bool IsShipInputDisabled = true;
    [HideInInspector]
    public bool initialized;
    [HideInInspector]
    public bool isDestroyed = false;
    public bool isShipInputDisabled = false;
    public bool isPlayerControlled = true;
    public float distanceToSectorCenter;
    
    // Keep a static reference for whether or not this is the player ship. It can be used
    // by various gameplay mechanics. Returns the player ship if possible, otherwise null.
    public static Ship PlayerShip { get => _playerShip;
        set => _playerShip = value;
    }
    private static Ship _playerShip;
    [Header("Ship Models")] 
    public ModelInfo shipModelInfo;
    public Transform cockpit;
    public MeshRenderer bodyVisuals;
    [Header("Ship instance info")]
    public Faction faction;
    // Maximum armor value can be modified by equipment
    public float maxArmor;
    public float armor;
    public string stationDocked = "none"; 
    public Vector3 Velocity => _physics.Rigidbody.velocity;
    public float velocity;
    public float throttle;
    public float Throttle => _playerInput.throttle;

    private AudioSource _engineSound;
    private Ship _portWingman = null, _starboardWingman = null;
    [Tooltip("This will be set if we're in a tractor beam. Just in case we need to shut it off manually...")]
    public GameObject controllingTractorBeam;
    
    [Header("DamageFX (Testing)")]
    public GameObject fireEffect;
    [Tooltip("Object with the effect")]
    public DamageFX dfx;
    #endregion ship components
    
    private void Awake()
    {
        initialized = false;
        // Initialize ship properties
        maxArmor = shipModelInfo.MaxArmor;
        armor = maxArmor;
        _engineSound = GetComponent<AudioSource>();
        _playerInput = GetComponent<ShipPlayerInput>();
        _aiInput = GetComponent<ShipAI>();
        _physics = GetComponent<ShipPhysics>();
        _shipCargo = GetComponent<ShipCargo>();
        _shipEquipment = GetComponent<ShipEquipment>();

        if (isPlayerControlled)
        {
            Ship.PlayerShip = this;
            _playerShip = this;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StartScenario")
        {
            TextFlash.ShowYellowText("Welcome Commander!\nOur survival in this sector depends on you!");
            SectorStartScenario.Instance.SetScenario();
            IsShipInputDisabled = false;
            initialized = true;
            // newbie kit
            GetComponent<ShipCargo>().AddWare(HoldItem.CargoType.Ware, "MarkI Missile", 5);
            Player.Instance.credits = 5000;
        }
        else if (scene.name == "EmptyFlight")
        {
            //print("Searching for spawn pos");
            Invoke(nameof(SearchForSpawnPos),1.5f);
        }
    }

    private void SearchForSpawnPos()
    {
        // Move player to spawn position
        foreach (var gate in SectorNavigation.Jumpgates)
        {
            if (gate.GetComponent<Jumpgate>().NextSector == Player.Instance.previousSector)
            {
                //print("found you");
                transform.position = gate.GetComponent<Jumpgate>().SpawnPos.position;
                transform.rotation = gate.GetComponent<Jumpgate>().SpawnPos.rotation;
                break;
            }
        }  
    }
    private void Update()
    {
        velocity = Velocity.magnitude;
        throttle = Throttle;
        keyboardMode = !Ship.PlayerShip.UsingMouseInput;
        _engineSound.pitch = 1.0f + Throttle * 2.0f;
        _engineSound.volume = _physics.IsEngineOn ? 1f : 0f;

        if (InSupercruise)
            _shipEquipment.SupercruiseDrain();
        
        if (isPlayerControlled)
            _playerShip = this;

        // Ai don't need anything beyond this
        if(_playerShip != this)
            return;
        
        //// SECTOR STREAMING
        // var pos = transform.position;
        // // Constrain Z coord
        // if (pos.z > SectorNavigation.SECTORSIZE)
        //     pos.z = SectorNavigation.SECTORSIZE;
        // else if (pos.z < -SectorNavigation.SECTORSIZE)
        //     pos.z = -SectorNavigation.SECTORSIZE;
        // transform.position = pos;
        // // Check XY for traversal
        // if (pos.x > SectorNavigation.SECTORSIZE || pos.x < -SectorNavigation.SECTORSIZE ||
        //     pos.y > SectorNavigation.SECTORSIZE || pos.y < -SectorNavigation.SECTORSIZE)
        // {
        //     Vector2 adjacentSectorPosition = new Vector2();
        //     // North sector
        //     if (pos.x > SectorNavigation.SECTORSIZE)
        //     {
        //         adjacentSectorPosition =
        //             new Vector2(SectorNavigation.CurrentSector.x + 1, SectorNavigation.CurrentSector.y);
        //     }
        //     // East sector
        //     else if (pos.y > SectorNavigation.SECTORSIZE)
        //     {
        //         adjacentSectorPosition =
        //             new Vector2(SectorNavigation.CurrentSector.x, SectorNavigation.CurrentSector.y + 1);
        //     }
        //     // West sector
        //     else if (pos.y < -SectorNavigation.SECTORSIZE)
        //     {
        //         adjacentSectorPosition =
        //             new Vector2(SectorNavigation.CurrentSector.x, SectorNavigation.CurrentSector.y - 1);
        //     }
        //     // South sector
        //     else if (pos.x < -SectorNavigation.SECTORSIZE)
        //     {
        //         adjacentSectorPosition =
        //             new Vector2(SectorNavigation.CurrentSector.x - 1, SectorNavigation.CurrentSector.y);
        //     }
        //     
        //     print("Sector traversed. Stream to "+adjacentSectorPosition+"?");
        //     transform.position = Vector3.zero;
        //     // Player.Instance.CurrentSector = adjacentSectorPosition;
        //     // SceneManager.LoadScene("EmptyFlight");
        //     // LoadSectorIntoScene
        // }

        // if (Input.GetKeyUp(KeyCode.I) && PlayArcadeIntegration.Instance.devBuild)
        // {
        //     TargetScrollMenu.OpenInfoMenu(Ship.PlayerShip.gameObject);
        // }

        if (Input.GetKeyUp(KeyCode.B))
        {
            var dock = SectorNavigation.Instance.GetClosestStation(this.transform, Ship.PlayerShip.shipModelInfo.ScannerRange,
                Int32.MaxValue);

            if (dock != null)
            {
                if (Vector2.Distance(dock[0].transform.position, this.transform.position) > 750)
                {
                    TextFlash.ShowYellowText("No station close enough for docking");
                    return;
                }

                doNotFireOnMe = true;
                // Open Confirm Dialog
                GameObject subMenu = GameObject.Instantiate(UIElements.Instance.ConfirmDialog, CanvasViewController.Instance.Hud.transform.parent);
                subMenu.transform.SetParent(CanvasViewController.Instance.Hud.transform);
                // Reposition submenu
                RectTransform rt = subMenu.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-125, 150);
                PopupConfirmMenuController confirmDocking = subMenu.GetComponent<PopupConfirmMenuController>();
                confirmDocking.HeaderText.text = dock[0].gameObject.GetComponent<Station>().stationName + " Docking control. May I help you?";
                confirmDocking.portrait.sprite = dock[0].gameObject.GetComponent<Station>().stationChief;
                Cursor.visible = true;
                PlayerShip.UsingMouseInput = false;
                confirmDocking.AcceptButton.GetComponentInChildren<TMP_Text>().text = "Request Docking";
                confirmDocking.AcceptButton.onClick.AddListener(() => {
                    isPlayerControlled = false;
                    AIInput.DockAt(dock[0]);
                    GameObject.Destroy(confirmDocking.gameObject);
                    doNotFireOnMe = false;
                });
                confirmDocking.CancelButton.GetComponentInChildren<TMP_Text>().text = "Close Channel";
                confirmDocking.CancelButton.onClick.AddListener(() => {
                    GameObject.Destroy(confirmDocking.gameObject);
                    doNotFireOnMe = false;
                    Cursor.visible = false;
                    PlayerShip.UsingMouseInput = true;
                });
            } else Debug.Log("Dock returned null. Giving up.");
        }

        // Enable or disable autopilot
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.A))
        { 
            if (!isPlayerControlled)
            {
                _aiInput.throttle = 0;
                isPlayerControlled = true;
                ConsoleOutput.PostMessage("Autopilot off.", Color.red);
                TextFlash.ShowYellowText("Autopilot off.");
                AIInput.FinishOrder();
                // Bugging out, turn off any tractor beam
                Ship.PlayerShip.controllingTractorBeam.SetActive(false);
                Ship.PlayerShip.controllingTractorBeam = null;
            }
            /*else
            {
                GameObject selectedTarget = InputHandler.Instance.GetCurrentSelectedTarget();
                if (selectedTarget != null)
                {
                    ConsoleOutput.PostMessage("Autopilot on.", Color.green);
                    TextFlash.ShowYellowText("Autopilot on.");

                    if(selectedTarget.tag == "Station")
                        AIInput.DockAt(selectedTarget);
                    else if (selectedTarget.tag == "Ship")
                        AIInput.Follow(selectedTarget.transform);
                    else if (selectedTarget.tag == "Jumpgate")
                        AIInput.DockAt(selectedTarget);
                    else
                        AIInput.MoveTo(selectedTarget.transform);
                }
            }*/
        }
    }

    /// <summary>
    /// Invoked when this ship takes damage. Amount of damage is given.
    /// </summary>
    public void TakeDamage(float damage, bool isPlayerShot, bool shouldShake=true)
    {
        if (isDestroyed)
            return;

        // No pain while docked
        if (stationDocked != "none")
            return;

        if(this.isPlayerControlled)
        {
            if (shouldShake && damage > 10)
            {
                StartCoroutine(CameraController.Shake());
                MusicController.Instance.PlaySound(AudioController.Instance.SmallImpact);
            }
        }

        armor -= damage;
        if(armor < 0)
        {
            isDestroyed = true;
            //print(this.name + " destroyed by player "+isPlayerShot);
            if (ShipDestroyedEvent != null) ShipDestroyedEvent(gameObject, EventArgs.Empty);

            ParticleController.Instance.CreateShipExplosionAtPos(transform.position);
            if (InputHandler.Instance.GetCurrentSelectedTarget() == this.gameObject)
                InputHandler.Instance.SelectedObject = null;
            _shipCargo.OnShipDestroyed();

            if (isPlayerShot) {
                // Broadcast kill
                Progression.RegisterKill(this);
                MissionControl.RegisterKill(this);

                // Mark player kill
                if (!shipModelInfo.ExternalDocking)
                {
                    TextFlash.ShowYellowText(faction.name + " fighter destroyed!");
                    ConsoleOutput.PostMessage(faction.name + " fighter destroyed!");
                }
                else
                {
                    TextFlash.ShowYellowText(faction.name + " freighter destroyed!");
                    ConsoleOutput.PostMessage(faction.name + " capital ship destroyed!");
                }

                if(faction != Player.Instance.playerFaction)
                    Player.Instance.AddFactionPenalty(faction);
            }
            if (faction == Player.Instance.playerFaction)
            {
                if (this == PlayerShip)
                {
                    Ship.PlayerShip.isDestroyed = true;
                                Ship.IsShipInputDisabled = true;
                    //print(Player.Instance.Ships.Count+" num player ships");
                    // if (Player.Instance.Ships.Count == 0)
                    // {
                        //print("GameOver (Ship.cs)");
                        Player.Instance.GameOver();
                        MusicController.Instance.PlaySound(AudioController.Instance.HardImpact);
                    // }
                    // else
                    // {
                    //     Ship nextplayership = Player.Instance.Ships[0].GetComponent<Ship>();
                    //     nextplayership.IsPlayerControlled = true;
                    //     Camera.main.GetComponent<CameraController>().SetTargetShip(nextplayership);
                    // }
                }
                else
                {
                    Player.Instance.Ships.Remove(this.gameObject);
                }
            }

            GameObject.Destroy(this.gameObject);
        }
        // else if (armor < 0.35 * maxArmor && fireEffect == null)
        // {
        //     // Spawn particle effects
        //     var fireEffect = GameObject.Instantiate(ParticleController.Instance.ShipDamageEffect, transform);
        //     //print("Loaded smoke damage");
        // }
        else if (armor < 0.25 * maxArmor && GetComponent<Ship>().isPlayerControlled)
        {
            TextFlash.ShowYellowText("Emergency auto docking routine engaged!");
            print("Emergency bugout! play an alarm?");
            GetComponent<ShipAI>().EmergencyDock();
        }
        else if (fireEffect != null)
            GameObject.Destroy(fireEffect);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (SceneManager.GetActiveScene().name != "MainMenu")
            SectorNavigation.Ships.Remove(this.gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (SectorNavigation.Ships != null)
            SectorNavigation.Ships.Add(this.gameObject);
    }

    // Gets the formation position offset when invoked by an escort ship
    public Vector3 GetWingmanPosition(Ship requestee)
    {
        if (requestee == this)
            return Vector3.zero;

        if (_portWingman == null)            // Port slot not occupied
        {
            //Debug.Log("[WINGMAN]: Ship " + requestee.name + " is port wingman for " + name);
            _portWingman = requestee;
            return new Vector3(-shipModelInfo.CameraOffset, 0, -shipModelInfo.CameraOffset);
        }
        else if (_starboardWingman == null)  // Starboard slot not occupied
        {
            //Debug.Log("[WINGMAN]: Ship " + requestee.name + " is starboard wingman for " + name);
            _starboardWingman = requestee;
            return new Vector3(-shipModelInfo.CameraOffset, 0, shipModelInfo.CameraOffset);
        }
        else    // Both slots occupied, ask port wingman 
        {
            return new Vector3(shipModelInfo.CameraOffset, 0, -shipModelInfo.CameraOffset) + _portWingman.GetWingmanPosition(requestee);
        }
    }

    public void RemoveWingman(Ship wingman)
    {
        if (_portWingman == wingman)
            _portWingman = null;
        else
            _starboardWingman = null;
    }
    
    public Transform impactFX;
    public float impactSize = 0.3f;
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Projectile") && !_aiInput.isUndocking)
        {
            // Almost always, severe bumping into a solid object
            var dam = collision.relativeVelocity.magnitude;
            dam = Mathf.Clamp(dam, 0, 50);

            /*if (collision.collider.CompareTag("Asteroid"))
            {
                //print("Asteroid bump "+dam);
                // Mining?
                if(GetComponent<ShipCargo>().CargoOccupied < GetComponent<ShipCargo>().CargoSize)
                {
                    var res = collision.collider.gameObject.GetComponent<Asteroid>().Resource;
                    var amount = collision.collider.gameObject.GetComponent<Asteroid>().Yield;
                    TextFlash.ShowYellowText(amount + " " + res + " extracted");
                    GetComponent<ShipCargo>().AddWare(HoldItem.CargoType.Ware, res, amount);
                }
            }*/

            if (!this.Equipment.shieldActive)
                TakeDamage(dam, false);
        }
        else if (collision.collider.CompareTag("Projectile"))
        {
            //     if (StationDocked != "none")
            //         return;
            //     
            //     //print("projectile hit us "+this.Equipment.shieldActive+" (shield)");
            //     if (!this.Equipment.shieldActive)
            //     {
            //         if (dfx != null) 
            //             dfx.Hit(collision.GetContact(0).point, 0.8f, 1, 1, 1, 0.1f);
            //         if (ImpactFX != null)
            //         {
            //             var fx = Instantiate(ImpactFX, collision.GetContact(0).point,
            //                 Quaternion.LookRotation(collision.GetContact(0).normal));
            //             fx.localScale = Vector3.one * 0.8f + Vector3.one * ImpactSize;
            //             print("projectile marked at "+collision.GetContact(0).point);
            //         }
            //         // Damage should be handled in the weaponData?
            //         // TakeDamage(collision.gameObject.GetComponent<Projectile>().damage,
            //         //     false); // Dont take damage from projectiles and when undocking
            //     }

            // Assuming we're under attack, ensure battle music plays. It will switch off if that's a bad choice.
            if (this == Ship.PlayerShip)
            {
                MusicController.Instance.forceBattle = true;
                //MusicController.Instance.PlaySound(AudioController.Instance.HardImpact);
            }
        }
    }

    // Inspector button helper
    public void GenerateShipValues()
    {
        distanceToSectorCenter = Vector3.Distance(this.transform.position, Vector3.zero);
    }
}

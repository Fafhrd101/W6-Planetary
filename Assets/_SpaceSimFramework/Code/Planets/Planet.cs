using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Random = UnityEngine.Random;

#region StatTypes
public enum PlanetaryTypes
{
    None,
    [Description("Primarily vegetaded.")] 
    Lush,
    [Description("A planet with less than 10% hydro.")] 
    Arid,
    [Description("50% or greater hydro.")] 
    Water,
    [Description("A frozen planet")] 
    Ice,
    [Description("Burning gases dominate.")] 
    Fire,
    [Description("Almost constantly having a storm.")] 
    Stormy,
    [Description("Planet has been completely refunctioned as an artificial satellite.")] 
    Artificial,
    [Description("Gas Giant, usable only for fuel")] 
    GasGiant  
}

public enum PlanetaryFunction
{
    None, // hopefully, used only to force a randomization call
    [Description("There is much still to be discovered on these planets; prior expeditions have found interesting POIs.")] 
    Exploration,
    [Description("Homeworlds are the progenators of a species. Intelligent life begins here.")]
    HomeWorld,
    [Description("This world has specialized in trading goods from every category.")]
    Trade,
    [Description("Producing crops of all types is the primary goal of this world.")]
    Agriculture,
    [Description("Producing manufactured goods is the primary goal of this world.")]
    Manufacturing,
    [Description("Producing Ores/Alloys is the primary goal of this world.")]
    Mining,
    [Description("Producing wood, water, etc is the primary goal of this world.")]
    NaturalResources,
    Military
}

public enum PlanetaryShipyard
{ 
    None,
    LandingField,
    LimitedServices,
    StandardClass,
    StellarClass,
    ImperialClass
}

#endregion

public class Planet : MonoBehaviour 
{
    public string planetName;
    public string id;
    public string ownerID;

    public GameObject ringsphere;
    public GameObject stormsphere;
    public GameObject atmosphere;
    public GameObject cloudSphere;
 
    [Tooltip("Planet owner faction")]
    public Faction faction;
    
    [Tooltip("Planet type data holder")]
    public PlanetLoadout loadout;
    // [Range(0, 5)]
    // public int keplarRegion = 0;
    public float distanceToPlayerShip;
    private GameObject _shipUnmooring;
    private float _normalScale;
    private Camera _mainCam;
    private bool _menuOpened;
    public List<GameObject> dockedShips;
    private Vector3 _viewedPos;

	private void Awake () 
    {
        // if (keplarRegion <= 0)
        //     keplarRegion = (int)Random.Range(1, 5);
        _normalScale = transform.localScale.x;
    }
    
    private void Start()
    {
        _mainCam = Camera.main;
        if(loadout != null)
        {
            PlanetLoadout.ApplyLoadoutToPlanet(loadout, this);
        }
    }
    public void Update()
    {
        if (_menuOpened) return;
        if (Ship.PlayerShip == null) return;
        distanceToPlayerShip = Vector3.Distance(Ship.PlayerShip.transform.position, this.transform.position);
        switch (distanceToPlayerShip)
        {
            case < 2000:
                _menuOpened = true;
                TextFlash.ShowYellowText("Approaching "+planetName+" cut scene should ensue");
                DockShip(Ship.PlayerShip.gameObject);
                break;
            case < 3500:
            {
                _viewedPos = Ship.PlayerShip.transform.position;
                Ship.PlayerShip.isSpeedLimited = true;
                Ship.PlayerShip.inSupercruise = false;
                var scale = Mathf.Abs(distanceToPlayerShip / 3500);
                scale = Mathf.Clamp(scale, 0, 1);
                _mainCam.fieldOfView = 60 * scale;
                break;
            }
            case > 3500:
                Ship.PlayerShip.isSpeedLimited = false;
                _mainCam.fieldOfView = 60;
                break;
        }
    }
    // public void GeneratePlanet()
    // {
    //     id = "PL-" + GenerateRandomSector.RandomString(4);
    //     if (keplarRegion == 0)
    //         return;
    //     Vector2 point = GetPointOnRing();
    //     float y = Random.Range(-250, 250);
    //     this.transform.position = new Vector3(point.x, y, point.y); 
    // }
    //
    // public void ClearPlanet()
    // {
    //     id = "";
    // }
    //
    // private Vector2 GetPointOnRing()
    // {
    //     float innerRange = keplarRegion * 750 + 100;
    //     float outerRange = keplarRegion * 750 + 250;
    //     Vector2 v = Random.insideUnitCircle;
    //     return v.normalized * innerRange + v*(outerRange - innerRange);
    // }

    private void DockShip(GameObject ship)
    {
        CanvasController.Instance.CloseAllMenus();
        CanvasViewController.Instance.SetHUDActive(false);
        if (Camera.main is not null)
        {
            var cam = Camera.main.GetComponent<CameraController>();
            cam.State = CameraController.CameraState.Chase;
            cam.SetTargetStation(this.transform, new Vector3(0, 50, -2000));
        }
        if (CanvasViewController.IsMapActive)
        {
            if (Camera.main is not null) Camera.main.GetComponent<MapCameraController>().CanMove = false;
            CanvasViewController.Instance.TacticalCanvas.gameObject.SetActive(false);
        }
        InputHandler.Instance.gameObject.SetActive(false);
        dockedShips.Add(ship);
        ship.SetActive(false);
        
        OpenPlanetMenu(ship);
    }
    
    public void UndockShip(GameObject ship)
    {
        Ship shipComp = ship.GetComponent<Ship>();

        dockedShips.Remove(ship);
        shipComp.stationDocked = "none";
        // reverse ship direction?
        ship.transform.position = Vector3.Reflect(ship.transform.position, Vector3.forward);
        ship.SetActive(true);


        ship.GetComponent<Rigidbody>().velocity = Vector3.zero;

        if (Ship.PlayerShip.gameObject == ship)
        {
            CanvasController.Instance.CloseAllStationMenus();
            InputHandler.Instance.SelectedObject = null;
            InputHandler.Instance.gameObject.SetActive(true);
            if (Camera.main is not null) Camera.main.GetComponent<CameraController>().SetTargetPlayerShip();
            CanvasViewController.Instance.SetHUDActive(!CanvasViewController.IsMapActive);
            if (CanvasViewController.IsMapActive)
            {
                CanvasViewController.Instance.TacticalCanvas.gameObject.SetActive(true);
                InputHandler.Instance.SelectedObject = null;
            }
        }
        TextFlash.ShowYellowText("Auto-undock engaged\n control will be returned momentarily.");

        StartCoroutine(FlyShipAwayFromDock(shipComp));
    }

    /// <summary>
    /// Takes over ship control while undocking to ensure safe distance from dock.
    /// Wait 3 second for dock doors to open, then 2 seconds of full throttle.
    /// then stop.
    /// </summary>
    private IEnumerator FlyShipAwayFromDock(Ship shipComp)
    {
        bool wasPlayerControlled = shipComp.isPlayerControlled;
        shipComp.isPlayerControlled = false;

        shipComp.AIInput.isUndocking = true;
        float timer = 6.0f;

        while (timer > 0) {
            shipComp.AIInput.angularTorque = Vector3.zero;
            if (timer < 3)
            {
                shipComp.AIInput.throttle = 1f;//-0.2f;
            }
            timer -= Time.deltaTime;
            yield return null;
        }
        shipComp.AIInput.throttle = 0f;
        shipComp.AIInput.isUndocking = false;
        shipComp.isPlayerControlled = wasPlayerControlled;
        Ship.PlayerShip.UsingMouseInput = true;
        shipComp.Physics.Rigidbody.isKinematic = false;
    }
    
    private void OpenPlanetMenu(GameObject ship)
    {
        var planetMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.PlanetaryMenu)
            .GetComponent<PlanetaryMenu>();
        Ship.IsShipInputDisabled = true;
        Ship.PlayerShip.Physics.Rigidbody.isKinematic = true;
        Cursor.visible = true;
        Ship.PlayerShip.UsingMouseInput = false;

        planetMenu.PopulateMenuOptions(ship, this);
    }

    private void ClosePlanetMenu()
    {
        Ship.IsShipInputDisabled = false;
        Ship.PlayerShip.Physics.Rigidbody.isKinematic = false;
        Cursor.visible = false;
        Ship.PlayerShip.UsingMouseInput = true;
        _menuOpened = false;
        
        CanvasController.Instance.CloseAllMenus();
    }
}

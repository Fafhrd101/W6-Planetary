using System;
using SpaceSimFramework.Code.UI.HUD;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StationShipDealerMenu : MonoBehaviour
{
    public ScrollMenuController StationMenu, PlayerMenu;
    public ScrollTextController DetailsView;
    public TMP_Text CreditsText;
    public TMP_Text InfoText;
    public Button ConfirmButton;

    private Ship ship;
    private Station station;
    private StationDealer stationWares;

    private void Start()
    {
        var result = station.name.Replace("Station-", "");
        StationMenu.HeaderText.text = result + " ship dealer";
        //StationMenu.HeaderText.text = stationWares.gameObject.name + " ship dealer";
        UpdateCredits();

        // Disable station menu and focus ship menu
        StationMenu.DisableKeyInput = true;
        PlayerMenu.DisableKeyInput = false;
        PlayerMenu.selectedOption = 0;
    }

    private void UpdateCredits()
    {
        // Keep credits display updated
        CreditsText.text = "Credits: " + Player.Instance.credits;
    }

    private void Update()
    {
        if (StationMenu.SubMenu != null || PlayerMenu.SubMenu != null)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            StationMenu.DisableKeyInput = true;
            PlayerMenu.DisableKeyInput = false;
            PlayerMenu.selectedOption = 0;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            StationMenu.DisableKeyInput = false;
            StationMenu.selectedOption = 0;
            PlayerMenu.DisableKeyInput = true;
        }
    }

    /// <summary>
    /// Invoked when a trade menu was opened for a ship docked on a station.
    /// </summary>
    /// <param name="ship">Ship trading with station</param>
    /// <param name="station">Station to trade with</param>
    public void PopulateMenu(GameObject ship, Station station)
    {
        this.ship = ship.GetComponent<Ship>();
        this.station = station;
        stationWares = station.gameObject.GetComponent<StationDealer>();

        PlayerMenuSetOptions();
        StationMenuSetOptions();
    }

    /// <summary>
    /// Resets and updates ship menu options
    /// </summary>
    private void PlayerMenuSetOptions()
    {
        PlayerMenu.ClearMenuOptions();
        foreach(var dockedShip in station.dockedShips)
        {
            Ship shp = dockedShip.GetComponent<Ship>();
            if (shp.faction == Player.Instance.playerFaction)
            {
                PlayerMenu.AddMenuOption(shp.shipModelInfo.modelName, Color.white,
                    IconManager.Instance.GetShipIcon(shp.shipModelInfo.modelName), 1f, 80)
                    .AddListener(() => OnOwnedShipClicked(dockedShip));
            }
        }
    }

    /// <summary>
    /// Resets and updates ship menu options
    /// </summary>
    private void StationMenuSetOptions()
    {
        foreach (var shipForSale in stationWares.shipsForSale)
        {
            string modelName = shipForSale.ship.GetComponent<Ship>().shipModelInfo.modelName;
            StationMenu.AddMenuOption(modelName, Color.white, IconManager.Instance.GetShipIcon(modelName), 1f, 80)
                .AddListener(() => OnDealerShipClicked(shipForSale.ship));
        }
    }

    /// <summary>
    /// Adds a ship mounted equipment item to the left-side menu and handles its onClick.
    /// </summary>
    /// <param name="cargo">Cargo item to sell</param>
    public void OnOwnedShipClicked(GameObject ownedShip)
    {
        // Check for open submenus
        if (StationMenu.SubMenu != null)
            GameObject.Destroy(StationMenu.SubMenu);
        if (PlayerMenu.SubMenu != null)
            GameObject.Destroy(PlayerMenu.SubMenu);

        Ship shipForSale = ownedShip.GetComponent<Ship>();
        PopulateDetailsView(shipForSale.shipModelInfo);

        if (ownedShip == Ship.PlayerShip.gameObject) {
            InfoText.text = "Cannot sell your current ship!";
            return;
        }

        ConfirmButton.GetComponentInChildren<Text>().text = "Sell";
        ConfirmButton.gameObject.SetActive(true);
        ConfirmButton.onClick.RemoveAllListeners();
        ConfirmButton.onClick.AddListener(() => {
            int salePrice = (int)Mathf.Lerp(0, shipForSale.shipModelInfo.Cost / 2f, shipForSale.armor / shipForSale.maxArmor);

            // Open Confirm Dialog
            GameObject SubMenu = GameObject.Instantiate(UIElements.Instance.ConfirmDialog, transform.parent);
            // Reposition submenu
            RectTransform rt = SubMenu.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0);

            PopupConfirmMenuController confirmSaleMenu = SubMenu.GetComponent<PopupConfirmMenuController>();
            confirmSaleMenu.HeaderText.text = "Confirm selling " + ownedShip.name + " for " + salePrice + " cr.";
            confirmSaleMenu.AcceptButton.onClick.AddListener(() => {
                Player.Instance.Ships.Remove(ownedShip);
                station.dockedShips.Remove(ownedShip);
                GameObject.Destroy(ownedShip);
                Player.Instance.credits += salePrice;
                ConfirmButton.gameObject.SetActive(false);
                ConfirmButton.onClick.RemoveAllListeners();
                UpdateCredits();
                PlayerMenuSetOptions();
                GameObject.Destroy(confirmSaleMenu.gameObject);
            });
            confirmSaleMenu.CancelButton.onClick.AddListener(() => {
                ConfirmButton.gameObject.SetActive(false);
                ConfirmButton.onClick.RemoveAllListeners();

                GameObject.Destroy(confirmSaleMenu.gameObject);
            });
        });
    }

    private void OnDealerShipClicked(GameObject boughtShip)
    {
        // Check for open submenus
        if (StationMenu.SubMenu != null)
            GameObject.Destroy(StationMenu.SubMenu);
        if (PlayerMenu.SubMenu != null)
            GameObject.Destroy(PlayerMenu.SubMenu);

        Ship shipForSale = boughtShip.GetComponent<Ship>();
        PopulateDetailsView(shipForSale.shipModelInfo);

        ConfirmButton.GetComponentInChildren<Text>().text = "Buy";
        ConfirmButton.gameObject.SetActive(true);
        ConfirmButton.onClick.RemoveAllListeners();
        ConfirmButton.onClick.AddListener(() =>
        {
            int cost = shipForSale.shipModelInfo.Cost;

            if (cost > Player.Instance.credits) {
                InfoText.text = "Not enough credits to purchase this ship.";
                return;
            }

            // Open Confirm Dialog
            GameObject SubMenu = GameObject.Instantiate(UIElements.Instance.ConfirmDialog, transform.parent);
            // Reposition submenu
            RectTransform rt = SubMenu.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0);

            var buyDialog = SubMenu.GetComponent<PopupConfirmMenuController>();
            buyDialog.HeaderText.text = "Buy " + shipForSale.shipModelInfo.modelName + " for " + cost + " credits?";

            // What happens when Ok or Cancel is pressed
            buyDialog.AcceptButton.onClick.AddListener(() =>
            {
                if (shipForSale.shipModelInfo.ExternalDocking)
                {
                    StationMooring mooringPoint = station.GetFreeMooringPoint();
                    if (mooringPoint == null)
                    {
                        InfoText.text = "Unable to purchase selected vessel: All station mooring points are occupied!";
                        return;
                    }

                    // Buy large ship
                    Player.Instance.credits -= cost;

                    GameObject newShip = GameObject.Instantiate(boughtShip, station.dock.transform.position, Quaternion.identity);
                    newShip.GetComponent<Ship>().faction = Player.Instance.playerFaction;
                    newShip.transform.SetParent(Player.Instance.transform);
                    Player.Instance.Ships.Add(newShip);

                    newShip.transform.position = mooringPoint.transform.position;
                    newShip.transform.rotation = mooringPoint.transform.rotation;
                    newShip.GetComponent<ShipPlayerInput>().throttle = 0;
                    newShip.GetComponent<ShipPlayerInput>().strafe = 0;
                    newShip.GetComponent<ShipPhysics>().enabled = false;

                    station.dockedShips.Add(newShip);
                    mooringPoint.ship = newShip;

                    ConsoleOutput.PostMessage("Ship " + shipForSale.shipModelInfo.modelName + " purchased!", Color.green);
                    ConfirmButton.gameObject.SetActive(false);
                    ConfirmButton.onClick.RemoveAllListeners();

                    PlayerMenuSetOptions();
                    UpdateCredits();

                    GameObject.Destroy(buyDialog.gameObject);
                }
                else
                {
                    // Buy small ship
                    Player.Instance.credits -= cost;

                    GameObject newShip = GameObject.Instantiate(boughtShip, station.dock.transform.position, Quaternion.identity);
                    newShip.GetComponent<Ship>().faction = Player.Instance.playerFaction;
                    newShip.transform.SetParent(Player.Instance.transform);
                    newShip.SetActive(false);
                    station.dockedShips.Add(newShip);
                    Player.Instance.Ships.Add(newShip);

                    ConsoleOutput.PostMessage("Ship " + shipForSale.shipModelInfo.modelName + " purchased!", Color.green);
                    ConfirmButton.gameObject.SetActive(false);
                    ConfirmButton.onClick.RemoveAllListeners();

                    PlayerMenuSetOptions();
                    UpdateCredits();

                    GameObject.Destroy(buyDialog.gameObject);
                }
            });
            buyDialog.CancelButton.onClick.RemoveAllListeners();
            buyDialog.CancelButton.onClick.AddListener(() =>
            {
                ConfirmButton.gameObject.SetActive(false);
                ConfirmButton.onClick.RemoveAllListeners();

                GameObject.Destroy(buyDialog.gameObject);
            });
        });
    }

    private void PopulateDetailsView(ModelInfo shipModelInfo)
    {
        DetailsView.ClearItems();

        DetailsView.AddMenuItem("Ship", shipModelInfo.modelName, true, Color.white);
        DetailsView.AddMenuItem("Requires external docking", shipModelInfo.ExternalDocking + "", true, Color.white);
        DetailsView.AddMenuItem("Cost", shipModelInfo.Cost + "\n", true, Color.white);

        DetailsView.AddMenuItem("(comparison to your current ship)", false, Color.white);
        DetailsView.AddMenuItem("Class", shipModelInfo.Class + "", false,
            GetComparedColor(shipModelInfo.Class, ship.shipModelInfo.Class));
        DetailsView.AddMenuItem("Armor", shipModelInfo.MaxArmor + "", false, 
            GetComparedColor(shipModelInfo.MaxArmor, ship.shipModelInfo.MaxArmor));
        DetailsView.AddMenuItem("Equipment slots", shipModelInfo.EquipmentSlots + "", false, 
            GetComparedColor(shipModelInfo.EquipmentSlots, ship.shipModelInfo.EquipmentSlots));
        DetailsView.AddMenuItem("Cargo capacity", shipModelInfo.CargoSize + "", false,
            GetComparedColor(shipModelInfo.CargoSize, ship.shipModelInfo.CargoSize));
        DetailsView.AddMenuItem("Generator power", shipModelInfo.GeneratorPower + "", false,
            GetComparedColor(shipModelInfo.GeneratorPower, ship.shipModelInfo.GeneratorPower));
        DetailsView.AddMenuItem("Generator regeneration", shipModelInfo.GeneratorRegen + "", false,
            GetComparedColor((int)shipModelInfo.GeneratorRegen, (int)ship.shipModelInfo.GeneratorRegen));
    }

    private Color GetComparedColor(int newValue, int currentValue)
    {
        if (newValue > currentValue)
            return Color.green;
        if (newValue < currentValue)
            return Color.red;
        return Color.white;
    }

    public void OnCloseClicked()
    {
        PlayerMenu.OnCloseClicked();
    }

}

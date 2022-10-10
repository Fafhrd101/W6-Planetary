using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StationEquipmentMenu : MonoBehaviour {

    public ScrollMenuController StationMenu, ShipMenu;
    public ScrollTextController DetailsView;
    public TMP_Text CreditsText;
    public TMP_Text InfoText;
    public Image portrait;
    private ShipEquipment _shipWeapons;
    private StationDealer _stationWares;
    private ShipCargo _shipCargo;
    private Ship _ship;
    private GameObject _popup;

    private void Start()
    {
        //print("menu starting");
        //PopulateMenu(Ship.PlayerShip.gameObject, this.gameObject.GetComponent<Station>());
        //StationMenu.HeaderText.text = gameObject.name + " equipment dealer";
        UpdateCredits();

        // Disable station menu and focus ship menu
        StationMenu.DisableKeyInput = true;
        ShipMenu.DisableKeyInput = false;
        ShipMenu.selectedOption = 0;
    }

    private void UpdateCredits()
    {
        // Keep credits display updated
        CreditsText.text = "Credits: " + Player.Instance.credits;
    }

    /// <summary>
    /// Invoked when a trade menu was opened for a ship docked on a station.
    /// </summary>
    /// <param name="ship">Ship trading with station</param>
    /// <param name="station">Station to trade with</param>
    public void PopulateMenu(GameObject ship, Station station)
    {
        //print("populating equipment menu");
        this._ship = ship.GetComponent<Ship>() ;
        _shipWeapons = ship.GetComponent<ShipEquipment>();
        _stationWares = station.gameObject.GetComponent<StationDealer>();
        _shipCargo = ship.GetComponent<ShipCargo>();
        portrait.sprite = station.equipmentSales;
        ShipMenuSetOptions();
        StationMenuSetOptions();
    }

    #region left side, ship menu
    /// <summary>
    /// Resets and updates ship menu options
    /// </summary>
    private void ShipMenuSetOptions()
    {
        ShipMenu.ClearMenuOptions();
        // foreach (GunHardpoint hardpoint in _shipWeapons.Guns)
        // {
        //     if (hardpoint.mountedWeapon != null)
        //         ShipMenu.AddMenuOption(hardpoint.mountedWeapon.name, Color.white, IconManager.Instance.GetWeaponIcon((int)IconManager.EquipmentIcons.Gun), 2f)
        //             .AddListener(() => OnMountedWeaponClicked(hardpoint));
        //     else
        //         ShipMenu.AddMenuOption(hardpoint.name, Color.grey, IconManager.Instance.GetWeaponIcon((int)IconManager.EquipmentIcons.Gun), 2f);
        // }
        // foreach (TurretHardpoint hardpoint in _shipWeapons.Turrets)
        // {
        //     if (hardpoint.mountedWeapon != null)
        //         ShipMenu.AddMenuOption(hardpoint.mountedWeapon.name, Color.white, IconManager.Instance.GetWeaponIcon((int)IconManager.EquipmentIcons.Turret), 2f)
        //             .AddListener(() => OnMountedWeaponClicked(hardpoint));
        //     else
        //         ShipMenu.AddMenuOption(hardpoint.name, Color.grey, IconManager.Instance.GetWeaponIcon((int)IconManager.EquipmentIcons.Gun), 2f);
        // }
        // foreach(HoldItem cargoItem in _shipCargo.CargoContents)
        // {
        //     if(cargoItem.cargoType == HoldItem.CargoType.Weapon)
        //         ShipMenu.AddMenuOption(cargoItem.itemName +" ("+cargoItem.amount+")").AddListener(() => {
        //             WeaponData weapon = ObjectFactory.Instance.GetWeaponByName(cargoItem.itemName);
        //             if (weapon == null)
        //             {
        //                 Debug.LogError("Ship contains unknown weapon! ("+cargoItem.itemName+")");
        //                 return;
        //             }
        //
        //             OnUnmountedWeaponClicked(weapon);
        //         });
        // }
        foreach(Equipment mountedItem in _ship.Equipment.MountedEquipment)
        {
            ShipMenu.AddMenuOption(mountedItem.name, Color.white,
                IconManager.Instance.GetEquipmentIcon(mountedItem.name), 1, 80).AddListener(() =>
            {
                OnSellEquipmentClicked(mountedItem);
            });
        }
    }

    /// <summary>
    /// Enables the user to sell a 
    /// </summary>
    /// <param name="mountedItem"></param>
    private void OnSellEquipmentClicked(Equipment mountedItem)
    {
        // Open Sell Dialog
        if(_popup != null)
        {
            CanvasController.Instance.CloseMenu();
            _popup = null;
        }
        _popup = CanvasController.Instance.OpenMenuAtPosition(UIElements.Instance.ConfirmDialog, new Vector2(-125, 100), false);
        // Reposition popup
        RectTransform rt = _popup.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 0);

        // Populate text menus
        PopupConfirmMenuController sellMenu = _popup.GetComponent<PopupConfirmMenuController>();
        int wareSellPrice = (int)(mountedItem.Cost / 2f);
        if (!mountedItem.isSellable)
            sellMenu.HeaderText.text = "This item cannot be removed once installed.";
        else
        {
            sellMenu.HeaderText.text = "Confirm selling " + mountedItem.name + " for " + wareSellPrice;

            // What happens when Ok or Cancel is pressed
            sellMenu.AcceptButton.onClick.RemoveAllListeners();
            sellMenu.AcceptButton.onClick.AddListener(() =>
            {
                // Sell item
                InfoText.text = "Sold " + mountedItem.name + " for " + wareSellPrice;
                _ship.Equipment.UnmountEquipmentItem(mountedItem);
                Player.Instance.credits += wareSellPrice;
                UpdateCredits();

                ShipMenuSetOptions();

                CanvasController.Instance.CloseMenu(); // Close popup
            });
        }

        sellMenu.CancelButton.onClick.RemoveAllListeners();
        sellMenu.CancelButton.onClick.AddListener(() => {
            CanvasController.Instance.CloseMenu();  // Close popup
        });
    }

    /// <summary>
    /// Adds an unmounted ship menu item to the left-side menu and handles its onClick.
    /// </summary>
    /// <param name="cargo">Cargo item to sell</param>
    public void OnUnmountedWeaponClicked(WeaponData weapon)
    {
        PopulateDetailsView(weapon);

        if (_popup != null)
        {
            CanvasController.Instance.CloseMenu();
            _popup = null;
        }
        _popup = CanvasController.Instance.OpenMenuAtPosition(UIElements.Instance.SimpleMenu, 
            new Vector2(Screen.width/2, Screen.height/2), false);

        SimpleMenuController weaponMenu = _popup.GetComponent<SimpleMenuController>();
        weaponMenu.HeaderText.text = "Mounted weapon";
        weaponMenu.AddMenuOption("Sell Weapon").AddListener(() =>
        {
            SellWeapon(weapon);
        });
        weaponMenu.AddMenuOption("Mount").AddListener(() =>
        {
            // Check class 
            if(weapon.Class > _ship.shipModelInfo.Class)
            {
                InfoText.text = "Your ship (class " + _ship.shipModelInfo.Class + ") cannot mount this weapon (class "+weapon.Class+")!";
                return;
            }

            GunHardpoint hardpoint = HardpointAvailable();
            if (hardpoint == null)
            {
                InfoText.text = "Your ship has no free hardpoints to mount this weapon!";
                return;
            }

            _shipCargo.RemoveCargoItem(weapon.name, 1);
            hardpoint.SetWeapon(weapon);
            ShipMenuSetOptions();
            InfoText.text = weapon.name+ " mounted to hardpoint "+hardpoint.name;
            CanvasController.Instance.CloseMenu(); 
        });
    }

    /// <summary>
    /// Adds a ship mounted equipment item to the left-side menu and handles its onClick.
    /// </summary>
    /// <param name="cargo">Cargo item to sell</param>
    public void OnMountedWeaponClicked(GunHardpoint hardpoint)
    {
        if (hardpoint.mountedWeapon == null)
            return;

        PopulateDetailsView(hardpoint.mountedWeapon);

        if (_popup != null)
        {
            CanvasController.Instance.CloseMenu();
            _popup = null;
        }
        _popup = CanvasController.Instance.OpenMenuAtPosition(UIElements.Instance.SimpleMenu,
            new Vector2(Screen.width / 2, Screen.height / 2), false);

        SimpleMenuController weaponMenu = _popup.GetComponent<SimpleMenuController>();
        weaponMenu.HeaderText.text = "Mounted weapon";
        weaponMenu.AddMenuOption("Sell Weapon").AddListener(() => 
        {
            SellWeapon(hardpoint);
        }
        );
        weaponMenu.AddMenuOption("Unmount").AddListener(() =>
        {
            // Check for cargospace
            if(_shipCargo.cargoOccupied < _shipCargo.cargoSize)
            {
                _shipCargo.AddWare(HoldItem.CargoType.Weapon, hardpoint.mountedWeapon.name, 1);
                hardpoint.SetWeapon(null);
                ShipMenuSetOptions();
            }
            else
            {
                InfoText.text = "Not enough free cargospace to unmount weapon!";
            }
            CanvasController.Instance.CloseMenu(); // Close sell/unmount menu
        });
    }

    /// <summary>
    /// Sells a player owned weapon to the dealer.
    /// </summary>
    /// <param name="weapon"></param>
    private void SellWeapon(WeaponData weapon)
    {
        int wareSellPrice = weapon.Cost;

        // Open Sell Dialog
        if (_popup != null)
        {
            CanvasController.Instance.CloseMenu();
            _popup = null;
        }
        _popup = CanvasController.Instance.OpenMenuAtPosition(UIElements.Instance.ConfirmDialog, Vector2.zero, false);
        // Reposition popup
        RectTransform rt = _popup.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 0);

        // Populate text menus
        PopupConfirmMenuController sellMenu = _popup.GetComponent<PopupConfirmMenuController>();
        sellMenu.HeaderText.text = "Confirm selling " + weapon.name + " for " + wareSellPrice;

        // What happens when Ok or Cancel is pressed
        sellMenu.AcceptButton.onClick.RemoveAllListeners();
        sellMenu.AcceptButton.onClick.AddListener(() => {
        // Sell item
            InfoText.text ="Sold " + weapon.name + " for " + wareSellPrice;
            _shipCargo.RemoveCargoItem(weapon.name, 1);
            Player.Instance.credits += wareSellPrice;
            UpdateCredits();

            ShipMenuSetOptions();

            CanvasController.Instance.CloseMenu();  // First close popup
            CanvasController.Instance.CloseMenu();  // Close the parent menu as well
        });
        sellMenu.CancelButton.onClick.RemoveAllListeners();
        sellMenu.CancelButton.onClick.AddListener(() => {
            CanvasController.Instance.CloseMenu();  // First close popup
            CanvasController.Instance.CloseMenu();  // Close the parent menu as well
        });
    }

    /// <summary>
    /// Sells a player owned weapon to the dealer.
    /// </summary>
    /// <param name="weapon"></param>
    private void SellWeapon(GunHardpoint hardpoint)
    {
        WeaponData weapon = hardpoint.mountedWeapon;
        int wareSellPrice = weapon.Cost;

        // Open Sell Dialog
        GameObject popup = CanvasController.Instance.OpenMenuAtPosition(UIElements.Instance.ConfirmDialog, Vector2.zero, false);
        // Reposition popup
        RectTransform rt = popup.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 0);

        // Populate text menus
        PopupConfirmMenuController sellMenu = popup.GetComponent<PopupConfirmMenuController>();
        sellMenu.HeaderText.text = "Confirm selling " + weapon.name + " for " + wareSellPrice;

        // What happens when Ok or Cancel is pressed
        sellMenu.AcceptButton.onClick.RemoveAllListeners();
        sellMenu.AcceptButton.onClick.AddListener(() => {
            // Sell item
            InfoText.text = "Sold " + weapon.name + " for " + wareSellPrice;
            Player.Instance.credits += wareSellPrice;
            UpdateCredits();
            hardpoint.SetWeapon(null);

            ShipMenuSetOptions();
            CanvasController.Instance.CloseMenu();  // First close popup
            CanvasController.Instance.CloseMenu();  // Close the parent menu as well
        });
        sellMenu.CancelButton.onClick.RemoveAllListeners();
        sellMenu.CancelButton.onClick.AddListener(() => {
            CanvasController.Instance.CloseMenu();  // First close popup
            CanvasController.Instance.CloseMenu();  // Close the parent menu as well
        });
    }
    #endregion left side, ship menu

    #region right side, station menu
    /// <summary>
    /// Resets and updates ship menu options
    /// </summary>
    private void StationMenuSetOptions()
    {
        foreach (var weapon in _stationWares.weaponsForSale)
        {
            StationMenu.AddMenuOption(weapon.weapon.name + " (Class " + weapon.weapon.Class + ", " + weapon.itemPrice + " Cr)",
                Color.white, IconManager.Instance.GetWeaponIcon((int)IconManager.EquipmentIcons.Gun), 2f)
                .AddListener(() => OnStationWeaponClicked(weapon.weapon));
        }
        foreach (var item in _stationWares.equipmentForSale)
        {
            StationMenu.AddMenuOption(item.eq.name + " \n(Cost: " + item.itemPrice + " Cr)",
                Color.white, IconManager.Instance.GetEquipmentIcon(item.eq.name), 1, 80)
                .AddListener(() => OnStationEquipmentClicked(item.eq, item.itemPrice));
        }

    }

    private void OnStationEquipmentClicked(Equipment item, int price)
    {
        // Check if there is a free weapon hardpoint
        PopulateDetailsView(item);
        bool isMissile = item.ItemName == "Missile Refill" ? true : false;
        
        // Open Buy Dialog
        if (_popup != null)
        {
            CanvasController.Instance.CloseMenu();
            _popup = null;
        }
        _popup = CanvasController.Instance.OpenMenuAtPosition(UIElements.Instance.ConfirmDialog, Vector2.zero, false);
        // Reposition popup
        RectTransform rt = _popup.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 0);

        // Populate text menus
        PopupConfirmMenuController wareMenu = _popup.GetComponent<PopupConfirmMenuController>();
        var wareName = item.name;
        var prefix = "";
        // if (isMissile) prefix = " " + numMissiles + " ";
        wareMenu.HeaderText.text = "Buy " + prefix + wareName + " for " + price + "?";

        // What happens when Ok or Cancel is pressed
        wareMenu.AcceptButton.onClick.RemoveAllListeners();
        wareMenu.AcceptButton.onClick.AddListener(() => {
            if (item.Cost <= Player.Instance.credits && _shipCargo.cargoOccupied < _shipCargo.cargoSize)
            {
                // Buy ware
                if (_ship.Equipment.MountEquipmentItem(item))
                {
                    Player.Instance.credits -= price;
                    UpdateCredits();
                    InfoText.text = "Bought " + item.name + " for " + price;
                    ShipMenuSetOptions();
                }
                else
                {
                    InfoText.text = "Unable to buy equipment because there are no free equipment slots!";
                }
            }
            else
            {
                InfoText.text = "Unable to buy equipment item due to insufficient funds!";
            }

            CanvasController.Instance.CloseMenu();  // First close popup
        });
        wareMenu.CancelButton.onClick.RemoveAllListeners();
        wareMenu.CancelButton.onClick.AddListener(() => {
            CanvasController.Instance.CloseMenu();  // First close popup
        });
    }

    /// <summary>
    /// Displays a ware sold by the station in the appropriate menu and handles
    /// onClick events when invoked.
    /// </summary>
    private void OnStationWeaponClicked(WeaponData weapon)
    {
        // Check if there is a free weapon hardpoint
        PopulateDetailsView(weapon);

        // Open Buy Dialog
        if (_popup != null)
        {
            CanvasController.Instance.CloseMenu();
            _popup = null;
        }
        _popup = CanvasController.Instance.OpenMenuAtPosition(UIElements.Instance.ConfirmDialog, Vector2.zero, false);
        // Reposition popup
        RectTransform rt = _popup.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        // Populate text menus
        PopupConfirmMenuController wareMenu = _popup.GetComponent<PopupConfirmMenuController>();
        wareMenu.HeaderText.text = "Buy " + weapon.name + " for "+weapon.Cost+ "?";

        // What happens when Ok or Cancel is pressed
        wareMenu.AcceptButton.onClick.RemoveAllListeners();
        wareMenu.AcceptButton.onClick.AddListener(() => {
            if (weapon.Cost <= Player.Instance.credits && _shipCargo.cargoOccupied < _shipCargo.cargoSize)
            {
                // Buy ware
                _shipCargo.AddWare(HoldItem.CargoType.Weapon, weapon.name, 1);
                Player.Instance.credits -= weapon.Cost;
                UpdateCredits();
                InfoText.text = "Bought " + weapon.name + " for " + weapon.Cost;

                ShipMenuSetOptions();
            }
            else
            {
                InfoText.text = "Unable to buy weapon due to insufficient funds or not enough available cargospace!";
            }
            CanvasController.Instance.CloseMenu();  // First close popup
        });
        wareMenu.CancelButton.onClick.RemoveAllListeners();
        wareMenu.CancelButton.onClick.AddListener(() => {
            CanvasController.Instance.CloseMenu();  // First close popup
        });

    }
    #endregion right side, station menu

    #region utility methods
    /// <summary>
    /// Finds a free hardpoint on the ship. Returns null if there are no free hardpoints.
    /// </summary>
    /// <returns>Free hardpoint or null if none are available</returns>
    private GunHardpoint HardpointAvailable()
    {
        foreach (GunHardpoint hardpoint in _shipWeapons.Guns)
        {
            if (hardpoint.mountedWeapon == null)
                return hardpoint;
        }
        foreach (TurretHardpoint hardpoint in _shipWeapons.Turrets)
        {
            if (hardpoint.mountedWeapon == null)
                return hardpoint;
        }

        return null;
    }

    private void PopulateDetailsView(WeaponData weapon)
    {
        DetailsView.ClearItems();

        DetailsView.AddMenuItem("Class", weapon.Class + "", false, Color.white);
        DetailsView.AddMenuItem("Weapon", weapon.name, true, Color.white);
        DetailsView.AddMenuItem("Range", weapon.Range+"", false, Color.white);
        if(weapon is ProjectileWeaponData) { 
            DetailsView.AddMenuItem("Damage", ((ProjectileWeaponData)weapon).Damage+"", false, Color.white);
            DetailsView.AddMenuItem("Projectile Speed", ((ProjectileWeaponData)weapon).ProjectileSpeed+"", false, Color.white);
        }
        DetailsView.AddMenuItem("Cost", weapon.Cost+"", false, Color.white);
        DetailsView.AddMenuItem(weapon.Description, false, Color.white);
    }

    private void PopulateDetailsView(Equipment item)
    {
        // DetailsView.ClearItems();
        //
        // DetailsView.AddMenuItem("Item; ", item.name, true, Color.white);
        // DetailsView.AddMenuItem("Cost: ", item.Cost+" Cr", false, Color.white);
        // DetailsView.AddMenuItem(item.Description, false, Color.white);
        DetailsView.HeaderText.text = "Item: " + item.name + "\nCost: " + item.Cost + "\n" + item.Description;
    }

    public void OnCloseClicked()
    {
        ShipMenu.OnCloseClicked();
    }
    #endregion utility methods
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using SpaceSimFramework.Code.UI.HUD;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class StationTradeMenu : MonoBehaviour {

    public ScrollMenuController StationMenu, ShipMenu;
    public TMP_Text CreditsText;

    private ShipCargo _shipCargo;
    private StationDealer _stationWares;
    private Station _station;

    private void Start()
    {
        //StationMenu.HeaderText.text = "Trade with " + _stationWares.gameObject.name;
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
        _shipCargo = ship.GetComponent<ShipCargo>();
        _stationWares = station.gameObject.GetComponent<StationDealer>();
        this._station = station;

        ShipMenuSetOptions();
        StationMenuSetOptions();
    }

    /// <summary>
    /// Resets and updates ship menu options
    /// </summary>
    private void ShipMenuSetOptions()
    {
        ShipMenu.ClearMenuOptions();
        foreach (HoldItem cargo in _shipCargo.cargoContents)
        {
            // Add only trade wares
            if(cargo.cargoType == HoldItem.CargoType.Ware)
            {
                Color color = Commodities.Instance.GetWareSellColor(cargo.itemName, GetWareSellingPrice(cargo.itemName));
                ShipMenu.AddMenuOption(cargo.itemName + " (" + cargo.amount + ")", Color.white, IconManager.Instance.GetWareIcon(cargo.itemName), 1, 80, color)
                    .AddListener(() => AddShipCargo(cargo));
            }
                
        }

    }

    /// <summary>
    /// Services a cargo item to the left-hand side Sell menu, opening an appropriate sell menu
    /// when invoked.
    /// </summary>
    private void AddShipCargo(HoldItem cargo)
    {
        var stationMenuSubMenu = StationMenu.SubMenu;
        if (stationMenuSubMenu != null)
            StationMenu.SubMenu = null;
        
        int wareSellPrice = GetWareSellingPrice(cargo.itemName);

        // Open Sell Dialog
        ShipMenu.SubMenu = GameObject.Instantiate(UIElements.Instance.SliderDialog, ShipMenu.transform.parent);
        // Reposition submenu
        RectTransform rt = ShipMenu.SubMenu.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 0);

        // Populate text menus
        PopupSliderMenuController sellMenu = ShipMenu.SubMenu.GetComponent<PopupSliderMenuController>();
        sellMenu.SetTextFields("Sell " + cargo.itemName + ", select amount:", "Amount: 0");

        // Edit slider value
        sellMenu.Slider.maxValue = cargo.amount;        
        sellMenu.Slider.onValueChanged.AddListener(value => {
            sellMenu.InfoText.text = "Amount: " + value;
            sellMenu.AmountText.text = "Credits: " + value* wareSellPrice;
        });

        // What happens when Ok or Cancel is pressed
        sellMenu.AcceptButton.onClick.RemoveAllListeners();
        sellMenu.AcceptButton.onClick.AddListener(() => {
            int profit = (int)sellMenu.Slider.value * wareSellPrice;

            // Sell cargo
            _shipCargo.RemoveCargoItem(cargo.itemName, (int)sellMenu.Slider.value);
            Player.Instance.credits += profit;

            var msg = "Sold " + sellMenu.Slider.value + " cargo units for " + profit + ", cargo occupied: " + _shipCargo.cargoOccupied;
            ConsoleOutput.PostMessage(msg);
            Debug.Log(msg);

            if(MissionControl.CurrentJob != null)
            {
                if (MissionControl.CurrentJob.Type == Mission.JobType.CargoDelivery)
                {
                    CargoDelivery job = (CargoDelivery)MissionControl.CurrentJob;
                    if (job.StationID == _station.id && job.Ware == cargo.itemName)
                    {
                        job.Amount -= (int)sellMenu.Slider.value;
                        if (job.Amount <= 0)
                            job.FinishJob();
                    }
                }
                else if(MissionControl.CurrentJob.Type == Mission.JobType.Courier)
                {
                    Courier job = (Courier)MissionControl.CurrentJob;
                    if (job.StationID == _station.id && job.Ware == cargo.itemName)
                    {
                        job.Amount -= (int)sellMenu.Slider.value;
                        if (job.Amount <= 0)
                            job.FinishJob();
                    }
                }
            }
            UpdateCredits();

            ShipMenuSetOptions();

            GameObject.Destroy(sellMenu.gameObject);
        });
        sellMenu.CancelButton.onClick.RemoveAllListeners();
        sellMenu.CancelButton.onClick.AddListener(() => {
            GameObject.Destroy(sellMenu.gameObject);
        });

    }

    /// <summary>
    /// Gets the buying price of the ware on the station. If ware is not found, returns 
    /// average ware price.
    /// </summary>
    /// <param name="item">Name of the ware</param>
    /// <returns>Selling price of ware on station</returns>
    private int GetWareSellingPrice(string item)
    {
        int itemPrice = (Commodities.Instance.GetWareByName(item).MinPrice + Commodities.Instance.GetWareByName(item).MaxPrice) / 3;

        foreach(var pair in _stationWares.goodsForSale)
        {
            var wareName = pair.itemName;
            var stationPrice = pair.itemPrice;

            if (item == wareName)
                return stationPrice;
        }
        return itemPrice;
    }
   
    /// <summary>
    /// Resets and updates ship menu options
    /// </summary>
    private void StationMenuSetOptions()
    {
        if (_stationWares.goodsForSale == null)
            return;
        foreach (var goods in _stationWares.goodsForSale)
        {
            Color color = Commodities.Instance.GetWareSellColor(goods.itemName, goods.itemPrice);
            StationMenu.AddMenuOption(goods.itemName + " " + goods.itemPrice + "Cr", Color.white, IconManager.Instance.GetWareIcon(goods.itemName), 1, 80, color)
                .AddListener(() => AddStationWare(goods));
        }
    }

    /// <summary>
    /// Displays a ware sold by the station in the appropriate menu and handles
    /// onClick events when invoked.
    /// </summary>
    /// <param name="ware">Ware sold at station</param>
    private void AddStationWare(StationGoodsForSale ware)
    {
        var shipMenuSubMenu = ShipMenu.SubMenu;
        if (shipMenuSubMenu != null)
            ShipMenu.SubMenu = null;

        // Open Sell Dialog
        StationMenu.SubMenu = GameObject.Instantiate(UIElements.Instance.SliderDialog, StationMenu.transform.parent);
        // Reposition submenu
        RectTransform rt = StationMenu.SubMenu.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(/*gameObject.GetComponent<RectTransform>().sizeDelta.x / 2*/0, 0);

        // Populate text menus
        PopupSliderMenuController wareMenu = StationMenu.SubMenu.GetComponent<PopupSliderMenuController>();
        wareMenu.SetTextFields("Buy " + ware.itemPrice + ", select amount:", "Amount: 0");

        // Edit slider value
        wareMenu.Slider.maxValue = _shipCargo.cargoSize - _shipCargo.cargoOccupied;
        wareMenu.Slider.onValueChanged.AddListener(value => {
            wareMenu.InfoText.text = "Amount: " + value;
            wareMenu.AmountText.text = "Price: " + value * ware.itemPrice;
        });

        // What happens when Ok or Cancel is pressed
        wareMenu.AcceptButton.onClick.RemoveAllListeners();
        wareMenu.AcceptButton.onClick.AddListener(() => {
            int price = (int)wareMenu.Slider.value * ware.itemPrice;
            if (price <= Player.Instance.credits)
            {
                // Buy ware
                _shipCargo.AddWare(HoldItem.CargoType.Ware, ware.itemName, (int)wareMenu.Slider.value);
                Player.Instance.credits -= price;
                UpdateCredits();

                var msg = "Bought " + wareMenu.Slider.value + " cargo units for " + price + ", cargo occupied: " + _shipCargo.cargoOccupied;
                ConsoleOutput.PostMessage(msg);
                Debug.Log(msg);
            }

            ShipMenuSetOptions();

            GameObject.Destroy(wareMenu.gameObject);
        });
        wareMenu.CancelButton.onClick.RemoveAllListeners();
        wareMenu.CancelButton.onClick.AddListener(() => {
            GameObject.Destroy(wareMenu.gameObject);
        });

    }

    public void OnCloseClicked()
    {
        ShipMenu.OnCloseClicked();
    }

}

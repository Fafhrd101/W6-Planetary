using System;
using SpaceSimFramework.Code.UI.HUD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RepairDialogueMenu : MenuController
{
    public TMP_Text messageBody;
    public TMP_Text priceBody;
    public Button acceptButton, partialButton, cancelButton;
    private int _repairCost;
    private float _damagePercentage;
    private float _repairPerPercentage;
    protected new void Start()
    {
        base.Start();
    }
    
    protected new void Update()
    {
        base.Update();
    }
    
    public void PopulateMenu(string storeText, Station station, Planet planet, string message = "")
    {
        // simple auto recharge. Expand later.
        Ship.PlayerShip.Equipment.energyAvailable = Ship.PlayerShip.Equipment.energyCapacity;
        
        Portrait.sprite = station ? station.repairDealer : null;
        this.HeaderText.text = storeText;
        //this.messageBody.text = message;
        float hullPercentage = (Ship.PlayerShip.armor / (float) Ship.PlayerShip.maxArmor) - 1;
        _damagePercentage = Mathf.Abs(hullPercentage);
        //print("Damage % = "+damagePercentage);

        var faction = station ? station.faction : planet.faction;
        var modifier = 2f;
        if (Ship.PlayerShip.faction.RelationWith(faction) >= .75)
            modifier = .5f;
        else if (Ship.PlayerShip.faction.RelationWith(faction) >= 0)
            modifier = 1f;
        
        // Cost should baseline around 50 credits per point
        _repairCost = (int)((Ship.PlayerShip.shipModelInfo.Cost/100000f) * modifier / _damagePercentage);
        print("repair cost per point = "+Ship.PlayerShip.shipModelInfo.Cost/100000f * modifier);
        _repairCost = Mathf.Clamp(_repairCost, 0, Int32.MaxValue);
        _repairPerPercentage = (int)((Ship.PlayerShip.shipModelInfo.Cost/100000f) * modifier);
        var repairText = _repairCost > 0
                ? "Repair hull damage for " + _repairCost + " credits?"
                : "\nYour hull looks fine to me. See someone else if you're looking for upgrades.\n";
        priceBody.text = "\nWe've recharged your energy supply, no charge.\n" + repairText;
        
        if (_repairCost > 0 && _repairCost <= Player.Instance.credits)
            acceptButton.interactable = true;
        else
            acceptButton.interactable = false;
    }
    
    #region button callbacks
    public void OnAcceptClicked()
    {
        Ship.PlayerShip.armor = Ship.PlayerShip.maxArmor;
        Player.Instance.credits -= _repairCost;
        
        if (IsSubMenu)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
        // Submenu should catch esc keypresses
        if (SubMenu == null)
            EventManager.OnCloseClicked(System.EventArgs.Empty, gameObject);
    }

    public void OnPartialClicked()
    {
        print("partial repair clicked");
        // Open Sell Dialog
        SubMenu = GameObject.Instantiate(UIElements.Instance.SliderDialog, transform.parent);
        // Reposition submenu
        RectTransform rt = SubMenu.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 0);

        // Populate text menus
        PopupSliderMenuController repairMenu = SubMenu.GetComponent<PopupSliderMenuController>();
        repairMenu.SetTextFields("Hull Repair", "Amount: 0");

        // Edit slider value
        repairMenu.Slider.maxValue = _repairCost;        
        repairMenu.Slider.onValueChanged.AddListener(value => {
            repairMenu.InfoText.text = "Amount: " + value;
            repairMenu.AmountText.text = "Credits: " + value * (int)_repairPerPercentage;
        });

        // What happens when Ok or Cancel is pressed
        repairMenu.AcceptButton.onClick.RemoveAllListeners();
        repairMenu.AcceptButton.onClick.AddListener(() => {
            int cost = (int)repairMenu.Slider.value * (int)_repairPerPercentage;
            Player.Instance.credits -= cost;
            var msg = "Repaired " + repairMenu.Slider.value + " HP for " + cost;
            Ship.PlayerShip.armor += repairMenu.Slider.value;
            Player.Instance.credits -= cost;
            ConsoleOutput.PostMessage(msg);
            Debug.Log(msg);

            GameObject.Destroy(repairMenu.gameObject);
            OnCloseClicked();
        });
        repairMenu.CancelButton.onClick.RemoveAllListeners();
        repairMenu.CancelButton.onClick.AddListener(() => {
            GameObject.Destroy(repairMenu.gameObject);
        });        
    }
    protected override void OnOptionSelected(int option)
    {
        if (SubMenu != null)
            return;

        MusicController.Instance.PlaySound(AudioController.Instance.SelectSound);
        _availableOptions[option].GetButtonOnClick().Invoke();
    }
    #endregion button callbacks
}

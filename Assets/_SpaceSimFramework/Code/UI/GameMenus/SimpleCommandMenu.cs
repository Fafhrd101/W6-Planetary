using UnityEngine;

public class SimpleCommandMenu : MenuController
{
    
    public GameObject closeButton;
    
    protected new void Start()
    {
        base.Start();
        closeButton.transform.SetParent(OptionContainer.transform, false);
    }
    
    protected new void Update()
    {
        base.Update();
    }
    
    public void PopulateMenu()
    {
        this.HeaderText.text = "Command Console";
        ShipAI shipAI = Ship.PlayerShip.gameObject.GetComponent<ShipAI>();

        this.AddMenuOption("Move To ...").AddListener(() => { shipAI.MoveTo(null); });
        //this.AddMenuOption("Follow me").AddListener(() => { shipAI.FollowMe(); });
        this.AddMenuOption("Follow ...").AddListener(() => { shipAI.Follow(null); });
        this.AddMenuOption("Idle").AddListener(() => { shipAI.Idle(); });
        this.AddMenuOption("Attack enemies").AddListener(() => { shipAI.AttackAll(); });
        this.AddMenuOption("Dock at ...").AddListener(() => { shipAI.DockAt(null); });
        this.AddMenuOption("Land at ...").AddListener(() => { shipAI.DockAt(null); });
        //this.AddMenuOption("Trade in sector").AddListener(() => { shipAI.AutoTrade(); });
    }
    
    #region component-specific functionality
    protected override void OnOptionSelected(int option)
    {
        if (SubMenu != null)
            return;

        MusicController.Instance.PlaySound(AudioController.Instance.SelectSound);
        _availableOptions[option].GetButtonOnClick().Invoke();
    }
    #endregion component-specific functionality
}

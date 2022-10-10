using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PortraitDialogueMenu : MenuController
{
    
    public GameObject closeButton;
    public Image portraitSlot;
    public TMP_Text messageBody;
    
    protected new void Start()
    {
        base.Start();
    }
    
    protected new void Update()
    {
        base.Update();
    }
    
    public void PopulateMenu(string storeText, Station station, string message = "")
    {
        portraitSlot.sprite = station.barTender;
        this.HeaderText.text = storeText;
        //this.messageBody.text = message;
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

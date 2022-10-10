using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RumourDialogueMenu : MenuController
{
    
    public GameObject closeButton;
    public Image portraitSlot;
    public TMP_Text queryText;
    public TMP_Text responseText;
    public Button tipButton;
    
    [TextArea]
    public string[] inGameRumours;
    protected new void Start()
    {
        base.Start();
    }
    
    protected new void Update()
    {
        base.Update();
        if (Player.Instance.credits < 5)
            tipButton.interactable = false;
    }
    
    public void PopulateMenu(string storeText, Station station, string message = "")
    {
        portraitSlot.sprite = station.barTender;
        this.HeaderText.text = storeText;
        //this.queryText.text = message;
    }
    
    #region component-specific functionality
    protected override void OnOptionSelected(int option)
    {
        // if (SubMenu != null)
        //     return;
        //
        // MusicController.Instance.PlaySound(AudioController.Instance.SelectSound);
        // _availableOptions[option].GetButtonOnClick().Invoke();
    }

    public void ButtonTipBartender()
    {
        Player.Instance.credits -= 5;
        this.responseText.text = Random.value < 0.5f ? Rumours.GenerateRumour() : inGameRumours[Random.Range(0, inGameRumours.Length)];
    }
    #endregion component-specific functionality
}

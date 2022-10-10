using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class StationNFT : MenuController
{

    public Image SpriteHolder;
    private new void Start()
    {
        base.Start();
        if (PlayArcadeIntegration.Instance != null)
        {
           // loop through any NFTs owned, add to a display list
           SpriteHolder.sprite = PlayArcadeIntegration.Instance.coinSprite;
        }
    }

    public void PopulateMenu(GameObject ship, Station station)
    {
        this.HeaderText.text = "JarJar Binks Ship Tattoo Shop";
        SpriteHolder.sprite = station.barTender;
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

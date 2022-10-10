using System;
using SpaceSimFramework.Code.UI.HUD;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ControlStatusUI : Singleton<ControlStatusUI>
{
    public TMP_Text controlText, totalHostiles, totalShips;
    
    public TMP_Text shieldStatus;
    
    public GameObject ramScoop;
    public TMP_Text ramScoopYield;
    public TMP_Text miningType;
    public bool ramScoopActive = false;

    private void Update()
    {
        var stat = Ship.PlayerShip.Equipment.energyAvailable > 25f ? "On" : "Off";
        shieldStatus.text = "Shield: " + stat;
        totalHostiles.text = Progression.totalEnemiesAlive.ToString();
        totalShips.text = SectorNavigation.GetShipsInRange(Ship.PlayerShip.transform, Single.MaxValue, Int32.MaxValue).Count.ToString();

        switch (ramScoopActive)
        {
            case true when !ramScoop.activeSelf:
                ramScoop.SetActive(true);
                break;
            case false when ramScoop.activeSelf:
                ramScoop.SetActive(false);
                break;
        }
    }
    
    public void SetControlStatusText(string controlStatus)
    { 
        controlText.text = controlStatus;
        StartCoroutine(FadeTextToZeroAlpha(1f, controlText));
    }

    private IEnumerator FadeTextToZeroAlpha(float t, TMP_Text i)
    {
        i.color = new Color(i.color.r, i.color.g, i.color.b, 1);
        while (i.color.a > 0.0f)
        {
            i.color = new Color(i.color.r, i.color.g, i.color.b, i.color.a - (Time.deltaTime / t));
            yield return null;
        }
    }
    
    #region on click listeners for UI buttons

    public void OnControlStatusClicked()
    {
        Camera.main.GetComponent<CameraController>().ToggleFlightMode();
    }

 
 
    #endregion on click listeners for UI buttons
}

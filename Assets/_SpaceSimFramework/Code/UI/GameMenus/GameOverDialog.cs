using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverDialog : MonoBehaviour
{
    public TMP_Text level;
    public TMP_Text exp;
    public TMP_Text credits;
    public TMP_Text shipsOwned;
    public TMP_Text reputation;
    public TMP_Text shipKills;
    public TMP_Text freighterKills;
    public TMP_Text stationKills;
    public TMP_Text wavesPassed;
    
    private void OnEnable()
    {
        PlayArcadeIntegration.Instance.SubmitScore(Progression.Experience, 
            "Salvage:"+Player.Instance.salvageCollected+"|"+
            "ShipsDestroyed:"+Player.Instance.shipsDestroyed+"|"+
            "FreightersDestroyed:"+Player.Instance.freightersDestroyed+"|"+
            "StationsDestroyed:"+Player.Instance.stationsDestroyed);
        // switch to keyboard mode so we can access buttons
        Ship.PlayerShip.UsingMouseInput = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        //level.text = ("Level: " + Progression.Level);
        exp.text = ("Final Score: " + Progression.Experience);
        credits.text = ("Credits: " + Player.Instance.credits);
        shipsOwned.text = ("Ships owned: " + (Player.Instance.Ships.Count + Player.Instance.OOSShips.Count));
        // foreach (Faction otherFaction in ObjectFactory.Instance.Factions)
        // {
        //     if (otherFaction == Player.Instance.PlayerFaction)
        //         continue;
        //     reputationMenu.AddMenuItem(otherFaction.name + ": " + Player.Instance.PlayerFaction.RelationWith(otherFaction).ToString("0.0"),
        //         false, Player.Instance.PlayerFaction.GetRelationColor(otherFaction));
        // }
        reputation.text = ("Player reputation");
        shipKills.text = ("Hostile fighters destroyed: " + Player.Instance.shipsDestroyed);
        freighterKills.text = ("Hostile freighters destroyed: " + Player.Instance.freightersDestroyed);
        stationKills.text = ("Stations destroyed: " + Player.Instance.stationsDestroyed);
    }

    public void buttonMainMenu()
    {
        //Application.Quit();
        SceneManager.LoadScene("MainMenu");
    }
}

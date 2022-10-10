using DunGen.DungeonCrawler;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanelUI  : Singleton<WarningUI>
{
    public TMP_Text credits,
        experience,
        hostileKills,
        hullArmor,
        shieldEnergy,
        missileCount,
        velocity,
        playerName,
        collectionName,
        totalJobs,
        raidersKilledByPlayer,
        raidersKilledByOthers;
    public Image portrait;
    private PlanetaryBaseSetup _setup;
    
    private void Start () 
    {
        _setup = GameObject.FindObjectOfType<PlanetaryBaseSetup>();
    }
    
    private void Update()
    {
        if (Ship.PlayerShip != null)
        {
            portrait.sprite = IconManager.Instance.players[Player.Instance.pilotIconNumber];
            var sign = Ship.PlayerShip.throttle >= 0 ? "" : "-";
            velocity.text = "Velocity: " + sign + Ship.PlayerShip.velocity.ToString("F0");
            hullArmor.text = "Hull Integrity: " + Ship.PlayerShip.armor.ToString("F0");
            shieldEnergy.text = "Shield Energy: " + Ship.PlayerShip.Equipment.energyAvailable.ToString("F0");
            missileCount.text = "Missiles: " + Ship.PlayerShip.Equipment.curMissiles;
            playerName.text = PlayArcadeIntegration.Instance.playerName;
            hostileKills.text = "Raiders Killed: " + Progression.Kills;
            collectionName.text = "League of Good Players";
            experience.text = "Score: " + Progression.Experience;
        }
        credits.text = "Credits Earned: "+ Player.Instance.credits;
        totalJobs.text = "Production Jobs: " + _setup.workers.Count;
        raidersKilledByPlayer.text = "Raider Kills: "+ Player.Instance.raidersKilledByPlayer;
        raidersKilledByOthers.text = "Raiders Removed: "+ Player.Instance.raidersKilledByOthers;
    }
}

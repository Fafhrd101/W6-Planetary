using UnityEngine;

/// <summary>
/// Keeps track of the player's progression and experience.
/// </summary>
public class Progression : MonoBehaviour
{
    public static int Level = 0;
    public static int[] LevelExperienceReq = { 1000, 2000, 4500, 8000, 14000, 22000 };
    public static int Experience = 0;
    public static int Kills = 0;
    public static int StationKills = 0;
    public static int totalEnemiesAlive = -1;
    
    public static void RegisterKill(Ship ship)
    {
        print("Kill registered");
        if (ship.faction == Player.Instance.playerFaction)
        {
            print("No exp for that kill");
            return;
        }

        if (ship.shipModelInfo.ExternalDocking)
        {
            AddExperience(800);
            Player.Instance.freightersDestroyed += 1;
        }
        else
        {
            AddExperience(400);
            Player.Instance.shipsDestroyed += 1;
        }
        Kills += 1;
    }
    public static void RegisterKill(Station station)
    {
        if (station.faction == Player.Instance.playerFaction)
        {
            print("No exp for that kill");
            return;
        }

        AddExperience(5000);
        StationKills += 1;
        Player.Instance.stationsDestroyed += 1;
    }
    public static void MissionCompleted()
    {
        AddExperience(500);
    }

    public static void AddExperience(int amount)
    {
        var bonus =  1 + (PlayNFT.Instance.score * .1f);
        amount = (int) (amount * bonus);
        
        Experience += amount;
        Player.Instance.experience = Experience;
        //print(amount+" Experience gained");
        
        if (Level < LevelExperienceReq.Length && Experience > LevelExperienceReq[Level])
        {
            Level++;
            //TextFlash.ShowYellowText("You have advanced to level " + Level + "!");
        }
    }
}

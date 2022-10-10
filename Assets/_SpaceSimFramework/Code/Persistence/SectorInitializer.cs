using UnityEngine;
using UnityEngine.SceneManagement;

public class SectorInitializer : Singleton<SectorInitializer>
{
    // Loading sequence: 
    // First - load savegame to obtain current sector
    // Second - load the sector data into scene
    // Third - load player's exploration state
    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "EmptyFlight")
        {
            // We don't want to load for the start sector, as it's been loaded once at main menu
            // Loading every other scene, as it tells us which sector we jumped into
            LoadGame.LoadAutosave();
            SectorNavigation.Instance.Awake();
            SectorLoader.LoadSectorData(Universe.Sectors[Player.Instance.currentSector].Name);

            // Start mission if needed
            if (MissionControl.CurrentJob != null)
                MissionControl.CurrentJob.OnMissionStarted();
        }
        else if (SceneManager.GetActiveScene().name != "EmptyPlanet")
        {
            Player.Instance.currentSector = new Vector2(-1, -1);
            // because you can't jump into -1,-1
            Player.Instance.previousSector = new Vector2(0, 0);
        }

        if (PlayerPrefs.HasKey("globalSoundVolume"))
            MusicController.Instance.globalSoundVolume = PlayerPrefs.GetFloat("globalSoundVolume");
        if (PlayerPrefs.HasKey("globalMusicVolume"))
            MusicController.Instance.globalMusicVolume = PlayerPrefs.GetFloat("globalMusicVolume");
        if (!PlayerPrefs.HasKey("helpScreenDone"))
            IngameMenuController.Instance.displayHelp();
        if (SceneManager.GetActiveScene().name != "EmptyPlanet")
        {
            SectorNavigation.ChangeSector(Player.Instance.currentSector, false);
            print("Fully loaded sector " + Player.Instance.currentSector);
        }

        // Save on entry, save on exit
        if (SceneManager.GetActiveScene().name != "EmptyPlanet")
            SaveGame.SaveAutosave(Player.Instance.currentSector);
        // Remove the sector loader once it has loaded everything
        GameObject.Destroy(gameObject);
    }
}

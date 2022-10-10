using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuDialog : MonoBehaviour
{
    public Slider musicSlider;
    public Slider soundSlider;
    
    public void OnEnable()
    {
                    Ship.IsShipInputDisabled = true;
        Cursor.visible = true;
        Ship.PlayerShip.UsingMouseInput = false;
        soundSlider.value = MusicController.Instance.globalSoundVolume; 
        musicSlider.value = MusicController.Instance.globalMusicVolume;
    }
    
    public void SoundChange()
    {
        MusicController.Instance.globalSoundVolume = soundSlider.value;
        PlayerPrefs.SetFloat("globalSoundVolume", MusicController.Instance.globalSoundVolume);
        PlayerPrefs.Save();
    }
    public void MusicChange()
    {
        MusicController.Instance.globalMusicVolume = musicSlider.value;
        PlayerPrefs.SetFloat("globalMusicVolume", MusicController.Instance.globalMusicVolume);
        PlayerPrefs.Save();
    }
    public void buttonCloseMenu()
    {
        Ship.IsShipInputDisabled = false;
        Cursor.visible = false;
        Ship.PlayerShip.UsingMouseInput = true;
        IngameMenuController.Instance.closeMenu();
        Destroy(this.gameObject);
    }
}

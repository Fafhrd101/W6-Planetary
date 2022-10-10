using UnityEngine;

public class AvatarMenuController : Singleton<AvatarMenuController>
{

    [HideInInspector]
    public float HUDDamage;

    [Header("HUD")]
    public GameObject HUDHealthBar;
    public GameObject HUDStaminaBar;
    public GameObject HUDDamageFullScreen;
    public GameObject HUDWeaponPicture;
    public GameObject HUDWeaponName;
    public GameObject HUDWeaponBullets;
    public GameObject HUDWeaponBulletsBar;
    public GameObject HUDWeaponClips;
    public GameObject HUDGrenadesIcon;
    public GameObject HUDGrenadesCount;

    [Header("Quick Reload HUD")]
    public GameObject quickReloadPanel;
    public GameObject quickReloadMarker;
    public GameObject quickReloadZone;
    [HideInInspector]
    public bool useQuickReload = true;

    [Header("Game Over")]
    public GameObject gameOverScreen;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

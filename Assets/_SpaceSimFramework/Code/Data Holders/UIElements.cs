using UnityEngine;

/// <summary>
/// Class containing prefab references
/// </summary>
[CreateAssetMenu(menuName = "DataHolders/UIElements")]
public class UIElements : SingletonScriptableObject<UIElements>{

    public GameObject FlashingText;
    public GameObject InstructionText;
    [Header("Menus")]
    public GameObject ScrollMenu;
    public GameObject ScrollText;
    public GameObject SimpleMenu;
    public GameObject ShipInfo;
    [Header("Game Menus")]
    public GameObject UniverseMap;
    public GameObject TargetMenu;
    public GameObject StationMainMenu;
    public GameObject StationTradeMenu;
    public GameObject StationEquipmentMenu;
    public GameObject StationDealershipMenu;
    public GameObject StationNFTMenu;
    public GameObject StationRepairMenu;
    public GameObject PlanetaryMenu;
    public GameObject SimpleCommandMenu;
    public GameObject StationAnnouncementMenu;
    public GameObject ComputerLibraryMenu;
    public GameObject StationJobsMenu;
    
    [Header("Dialogs")]
    public GameObject SliderDialog;
    public GameObject InputDialog;
    public GameObject ConfirmDialog;
    public GameObject PortraitDialog;
    public GameObject ClosedShopDialog;
    public GameObject HelpMenuDialog;
    public GameObject RumourDialog;
    public GameObject SettingsDialog;
    
    [Header("Elements")]
    public GameObject ClickableText;
    public GameObject ClickableTextChoice;
    public GameObject ClickableImageText;
    public GameObject TextPanel;
    public GameObject TwoTextPanel;


}

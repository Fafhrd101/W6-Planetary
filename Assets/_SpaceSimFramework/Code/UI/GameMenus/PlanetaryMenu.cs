using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlanetaryMenu : ScrollMenuController
{
    public ScrollMenuController mainMenu, detailMenu;
    private GameObject _shipLandedAtPlanet;
    public TMP_Text starportText;
    public TMP_Text factionText;
    public TMP_Text functionText;
    public TMP_Text planetTypeText;
    public Image background;
    private Planet _planet;
    
    public void PopulateMenuOptions(GameObject ship, Planet planet)
    {
        _shipLandedAtPlanet = ship;
        _planet = planet;
        HeaderText.text = planet.planetName;
        starportText.text = "Starport: "+planet.loadout.shipyard.ToString();
        factionText.text = "Faction: "+planet.faction.name;
        functionText.text = "Function: "+planet.loadout.function.ToString();
        planetTypeText.text = "Biome: "+planet.loadout.planetType.ToString();
        MainMenuSetOptions();
        detailMenuSetOptions();
    }

    private void MainMenuSetOptions()
    {
        mainMenu.ClearMenuOptions();
        AddMenuOption("Repair Ship").AddListener(() =>
        {
            var repairMenu = CanvasController.Instance.OpenMenu(UIElements.Instance.StationRepairMenu);
            var repairDialog = repairMenu.GetComponent<RepairDialogueMenu>();
            repairDialog.PopulateMenu("", null, _planet, "");
        });
        AddMenuOption("View Planet").AddListener(() => {
        });
        AddMenuOption("Explore Planet").AddListener(() =>
        {
            Player.Instance.previousPosition = Ship.PlayerShip.transform.position;
            SaveGame.SaveAutosave(Player.Instance.currentSector);
            SceneManager.LoadScene("EmptyPlanet");
        });
        AddMenuOption("Leave Planet").AddListener(() => {
            CanvasController.Instance.CloseMenu();
            _planet.UndockShip(_shipLandedAtPlanet);
        });   
    }
private void detailMenuSetOptions(){}

}

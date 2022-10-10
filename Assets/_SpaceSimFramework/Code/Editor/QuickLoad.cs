using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class QuickLoad
{
    [MenuItem("Tools/Load MainMenu", false, 20)]
    static void MainMenu()
    {
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        if (SceneManager.GetActiveScene().name!="MainMenu")
            EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/MainMenu.unity");
    }
    
    [MenuItem("Tools/Load StartScenario", false, 20)]
    static void StartScenario()
    {
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        if (SceneManager.GetActiveScene().name!="StartScenario")
            EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/StartScenario.unity");
    }
    
    [MenuItem("Tools/Load EmptyFlight", false, 20)]
    static void EmptyFlight()
    {
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        if (SceneManager.GetActiveScene().name!="EmptyFlight")
            EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/EmptyFlight.unity");
    }
    [MenuItem("Tools/Load EmptyPlanet", false, 20)]
    static void EmptyPlanet()
    {
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        if (SceneManager.GetActiveScene().name!="EmptyPlanet")
            EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/EmptyPlanet.unity");
    }
    [MenuItem("Tools/Ship Editor", false, 10)]
    static void ShipEditor()
    {
        if (SceneManager.GetActiveScene().name!="ShipEditor")
            EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/TestScenes/ShipEditor.unity");
    }   
    
    // [MenuItem("Tools/Station Editor", false, 10)]
    // static void StationEditor()
    // {
    //     if (SceneManager.GetActiveScene().name!="StationEditor")
    //         EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/TestScenes/StationEditor.unity");
    // } 
    //
    // [MenuItem("Tools/Planet Editor", false, 10)]
    // static void PlanetEditor()
    // {
    //     if (SceneManager.GetActiveScene().name!="PlanetEditor")
    //         EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/TestScenes/PlanetEditor.unity");
    // }

    [MenuItem("Tools/Open Data Window", false, 0)]
    static void openExplorer()
    {
        EditorUtility.RevealInFinder(Utils.SECTORS_FOLDER);
    }
}

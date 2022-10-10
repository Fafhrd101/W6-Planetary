using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SystemEditor : EditorWindow
{
    
    private int _skyboxIndex;
    private int _flareIndex;
    private Vector2 _mapPosition;
    private string[] _skyboxList;
    private string[] _flareList;
    private Color _skyTint;
    private int MapRange;
    private int counter;
    private Flare _sun = null;
    private Material _skybox = null;
    private Color _skyboxTint;
    private GameObject[] _stations, _jumpgates, _fields, _planets, _sectorObjects;

    private float _timer = 0;
    private int sectorSize = 0;
    
    [MenuItem("Tools/Sector Editor", false, 10)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindowWithRect(typeof(SystemEditor), new Rect(0, 0, 450, 650));
        if (SceneManager.GetActiveScene().name!="SectorEditor")
            EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/TestScenes/SectorEditor.unity");
    }

    private void Awake()
    {
        FindObjectsForExport();
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if(_timer < 0)
        {
            _timer = 1f;
            if(EditorSceneManager.GetActiveScene().name != "SectorEditor")
                EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/SectorEditor.unity");
            FindObjectsForExport();
        }
    }

    void OnGUI()
    {
        if (_skyboxList == null || _skyboxList.Length == 0)
        {
            // Init _skyboxList
            _skyboxList = new string[SectorVisualData.Instance.skybox.Length];
            for (int i = 0; i < SectorVisualData.Instance.skybox.Length; i++)
                _skyboxList[i] = SectorVisualData.Instance.skybox[i].name;
        }
        if (_flareList == null || _flareList.Length == 0)
        {
            // Init Flare list
            _flareList = new string[SectorVisualData.Instance.flares.Length];
            for (int i = 0; i < SectorVisualData.Instance.flares.Length; i++)
                _flareList[i] = SectorVisualData.Instance.flares[i].name;
        }
        /*
        * Clear data
        */
        GUILayout.Space(10);
        if (GUILayout.Button("Clear Sector"))
        {
            EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/TestScenes/SectorEditor.unity");
            ClearSector();
        }

        /*
        * Save sector data
        */
        GUILayout.Space(5);
        if (GUILayout.Button("Save Sector"))
        {
            SaveSectorToFile();
        }

        /*
         * Sector loading
         */
        GUILayout.Space(5);
        if (GUILayout.Button("Load Sector"))
        {
            EditorSceneManager.OpenScene("Assets/_SpaceSimFramework/Content/Scenes/TestScenes/SectorEditor.unity");
            LoadSectorFromFile();
        }
        GUILayout.Space(5);
        if (GUILayout.Button("Generate Random Sector"))
        {
            ClearSector();
            GenerateRandomSector.GenerateSectorAtPosition(new Vector2(_mapPosition.x, _mapPosition.y), Vector2.one*9999);
            counter = 1;
        }
        
        GUILayout.Space(5);
        int x=0, y=0;
        if (GUILayout.Button("Generate All Sectors In Universe"))
        {
            Universe.ClearUniverse();
            counter = 0;
            for (x = 0; x < MapRange; x++)
            {
                for (y = 0; y < MapRange; y++)
                {
                    // if (x == 1 && y == 1) // special sector, handmade
                    //     continue;
                    ClearSector();
                    _mapPosition.x = x;
                    _mapPosition.y = y;
                    GenerateRandomSector.GenerateSectorAtPosition(new Vector2(_mapPosition.x, _mapPosition.y), Vector2.one*9999, MapRange-1);
                    SaveSectorToFile();
                    counter++;
                    //Debug.Log("file = _x"+x+"y"+y);
                } 
            }
        }
        // GUILayout.Space(5);
        // if (GUILayout.Button("Regenerate Universe File"))
        // {
        //     Universe.SaveUniverse();
        // }
        /*
        * Sector data editing
        */
        GUILayout.Space(5);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        _skyboxTint = RenderSettings.skybox.GetColor("_Tint");
        _skyboxIndex = SectorVisualData.Instance.GetSkyboxIndex();
        _flareIndex = SectorVisualData.Instance.GetFlareIndex();
        GUILayout.Space(5);  
        GUILayout.Label("SKY OPTIONS", EditorStyles.centeredGreyMiniLabel);
        _skyboxIndex = EditorGUILayout.Popup("Starbox", _skyboxIndex, _skyboxList);
        _flareIndex = EditorGUILayout.Popup("Lens Flare", _flareIndex, _flareList);
        _skyboxTint = EditorGUILayout.ColorField("Tint", _skyboxTint);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        GUILayout.Label("SET THIS FOR MANUAL GENERATION", EditorStyles.centeredGreyMiniLabel);
        _mapPosition = EditorGUILayout.Vector2Field("", _mapPosition);
        
        GUILayout.Space(10);
        GUILayout.Label("SET THIS FOR AUTOMATED GENERATION", EditorStyles.centeredGreyMiniLabel);
        MapRange = EditorGUILayout.IntField("GridSize", MapRange);

        /*
        * Find and display export objects
        */
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(5);
        GUILayout.Label("SECTOR CONTENTS", EditorStyles.centeredGreyMiniLabel);
        GUILayout.Space(5);
        GUILayout.Label("Sun flare: " + _sun.name);
        GUILayout.Label("Skybox: " + _skybox.name);
        GUILayout.Label("SkyTint: " + _skyboxTint);

        foreach (var station in _stations)
        {
            if (station != null)
                GUILayout.Label("Station: " + station.name);
        }
        foreach (var planet in _planets)
        {
            if (planet != null)
                GUILayout.Label("Planet: " + planet.name);
        }
        foreach (var gate in _jumpgates)
        {
            if (gate != null)
                GUILayout.Label("Jumpgate: " + gate.name);
        }
        foreach (var field in _fields)
        {
            if (field != null)
                GUILayout.Label("Asteroid Field: " + field.name);
        }
        foreach (var field in _sectorObjects)
        {
            if (field != null)
                GUILayout.Label("Sector Objects: " + field.name);
        }
        
        GUILayout.Space(25);
        GUILayout.Label("Sectors generated: " + counter);
    }

    private void FindObjectsForExport()
    {
        // Get sun flare
        _sun = GameObject.FindGameObjectWithTag("Sun").GetComponent<Light>().flare;
        // Get skybox
        _skybox = RenderSettings.skybox;

        // Get stations
        _stations = GameObject.FindGameObjectsWithTag("Station");
        // Get planets
        _planets = GameObject.FindGameObjectsWithTag("Planet");
        // Get jumpgates
        _jumpgates = GameObject.FindGameObjectsWithTag("Jumpgate");
        // Get asteroid fields
        _fields = GameObject.FindGameObjectsWithTag("AsteroidField");
        // Get sector objects
        _sectorObjects = GameObject.FindGameObjectsWithTag("SectorObject");
    }

    private void SaveSectorToFile()
    {
        // string path = EditorUtility.SaveFilePanel(
        //      "Save sector to file",
        //      "",
        //      "UNNAMED_SECTOR_" + Time.timeSinceLevelLoad,
        //      "");
        string path = Utils.SECTORS_FOLDER + "_x" + _mapPosition.x + "y" + _mapPosition.y;
        if (!Directory.Exists(Utils.SECTORS_FOLDER))
        {
            Directory.CreateDirectory(Utils.SECTORS_FOLDER);
        }
        
        if (path != "")
        {
            FindObjectsForExport();
            foreach (var station in _stations)
            {
                sectorSize++;
                station.GetComponent<Station>().id = "ST-" + GenerateRandomSector.RandomString(4);
                station.GetComponent<Station>().stationName = RandomNameGenerator.GetRandomStationName();
            }
            foreach (var planet in _planets)
            {
                sectorSize++;
                planet.GetComponent<Planet>().id = "PL-" + GenerateRandomSector.RandomString(4);
            }
            foreach (var gate in _jumpgates)
            {
                sectorSize++;
                gate.GetComponent<Jumpgate>().id = "JG-" + GenerateRandomSector.RandomString(4);
            }
            foreach (var field in _fields)
            {
                sectorSize++;
                field.GetComponent<AsteroidField>().ID = "AF-" + GenerateRandomSector.RandomString(4);
            }
            foreach (var field in _sectorObjects)
            {
                sectorSize++;
                field.GetComponent<SectorObject>().ID = "SO-" + GenerateRandomSector.RandomString(4);
            }

            SectorSaver.SaveSectorToPath(_stations, _planets, _jumpgates, _fields, _sectorObjects, sectorSize, path);
        }
    }

    private void LoadSectorFromFile()
    {
        sectorSize = 0;
        string path = EditorUtility.OpenFilePanel("Import sector file", "Assets/_SpaceSimFramework/Data/Sectors", "");
        string sectorName = Path.GetFileNameWithoutExtension(path);
        if (!string.IsNullOrEmpty(path))
        {
            ClearSector();
            SectorLoader.LoadSectorIntoScene(path);
        }
    }

    private void ClearSector()
    {
        sectorSize = 0;
        // Clear sector
        GameObject[] objects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].tag != "Sun" && objects[i].tag != "MainCamera" && objects[i].tag != "Maze")
            {
                GameObject.DestroyImmediate(objects[i]);
            }
        }
    }

    private void UpdateUniverseMap(string saveFilename)
    {
        // Load sectors
        Dictionary<SerializableVector2, SerializableUniverseSector> existingSectors = Universe.LoadUniverse();

        List<GameObject> jumpgates = new List<GameObject>(GameObject.FindGameObjectsWithTag("Jumpgate"));

        if (jumpgates.Count > 0)
            Universe.AddSector(new Vector2(_mapPosition.x, _mapPosition.y), jumpgates, saveFilename);
    }
}

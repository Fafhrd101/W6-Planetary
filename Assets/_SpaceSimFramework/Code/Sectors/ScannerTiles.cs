using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScannerTiles : MonoBehaviour
{
    public GameObject cellObjectPrefab;
    public GameObject radarView;
    public int cellScale = 1000;
    public Transform cellHolder;
    public bool visible = true;
    public int radius;
    public List<Vector3> tileCoordinates = new List<Vector3>();
    //public List<Vector2> hexCoordinates = new List<Vector2>();
    public List<GameObject> spiralOrderedObjects = new List<GameObject>();
    private Vector3 _previousPos;
    private RectTransform _rectTransform;
    public void Start()
    {
        radius = Ship.PlayerShip.shipModelInfo.ScannerRange / cellScale / 2;
        BuildMap();
        BuildList();
        ToggleVisibility();
        InvokeRepeating(nameof(RedrawHex), 1, 1);
        _rectTransform = radarView.GetComponent<RectTransform>();
        _rectTransform.sizeDelta =
            new Vector2(Ship.PlayerShip.shipModelInfo.ScannerRange, Ship.PlayerShip.shipModelInfo.ScannerRange);
        
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) 
        {
            // Click on hex mesh, convert to cell address
            HandleInput();
        }
        if (CanvasViewController.IsMapActive && !visible || !CanvasViewController.IsMapActive && visible)
            ToggleVisibility();
        
        _rectTransform.Rotate( new Vector3( 0, 0, -1 ) );
    }

    private void HandleInput ()
    {
        if (Camera.main is null) return;
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(inputRay, out var hit)) {
            if (hit.collider.CompareTag("SectorMap"))
                MoveToCell(hit.collider.transform.position);
        }
    }

    private void MoveToCell(Vector3 position)
    {
        //print("Centered at "+position);
        if (Ship.PlayerShip != null && Ship.PlayerShip.AIInput != null)
            Ship.PlayerShip.AIInput.MoveTo(position);
        //TextFlash.ShowYellowText("Order received. Moving.");
    }

    private void ToggleVisibility()
    {
        if (Ship.PlayerShip == null) return;
        // if has equipment for this
        Equipment radar = Ship.PlayerShip.Equipment.mountedEquipment.SingleOrDefault(obj => obj.name == "Radar");
        if (radar != null)
            print("radar mounted");
        cellHolder.gameObject.SetActive(!visible);
        radarView.SetActive(!visible);
        visible = !visible;
    }
    
    public void RedrawHex()
    {
        if (Ship.PlayerShip != null)
        {
            if (Vector3.Distance(_previousPos, Ship.PlayerShip.transform.position) < cellScale)
                return;
            //Turn all renderers off
            foreach (var obj in spiralOrderedObjects)
            {
                obj.GetComponent<Renderer>().enabled = true;
            }
            // // Turn them back on in a spiral, slowed
            // StartCoroutine(turnOnSpiral());
            var position = Ship.PlayerShip.transform.position;
            transform.position = position;
            _previousPos = position;
        }
    }

    private void BuildList()
    {
        for (int i = 0; i < tileCoordinates.Count; i++)
        {
            spiralOrderedObjects[i] = cellHolder.transform.GetChild(i).gameObject;
        }
    }

    private void BuildMap()
    {
        // Adds them in a spiral pattern. Just not sure how best to use it...
        //https://stackoverflow.com/questions/2142431/algorithm-for-creating-cells-by-spiral-on-the-hexagonal-field
        // int x = 0, y = 0;
        // hexCoordinates.Add(new Vector2(x, y));
        // for (int N = 1; N <= radius; ++N)
        // {
        //     for(int i = 0;i<N;++i) hexCoordinates.Add(new Vector2(++x, y));
        //     for(int i = 0;i<-N;++i) hexCoordinates.Add(new Vector2(x, ++y));
        //     for(int i = 0;i<N;++i) hexCoordinates.Add(new Vector2(--x, ++y));
        //     for(int i = 0;i<N;++i) hexCoordinates.Add(new Vector2(--x, y));
        //     for(int i = 0;i<N;++i) hexCoordinates.Add(new Vector2(x, --y));
        //     for(int i = 0;i<N;++i) hexCoordinates.Add(new Vector2(++x, --y));
        // }
        int radiusToUse = radius+1; // ensures it sticks past radar
        
            //Code to fill the list with coordinates
            //Adds the middle tile
             tileCoordinates.Add(new Vector3(0, 0, 0));
             spiralOrderedObjects.Add(null);
             //Generates the central row
             for (int i = 0; i < radiusToUse; i++)
             {
                 tileCoordinates.Add(new Vector3(0, 0, i + 1));
                 spiralOrderedObjects.Add(null);
                 tileCoordinates.Add(new Vector3(0, 0, -i - 1));
                 spiralOrderedObjects.Add(null);
             }

             //Generates remaining rows

             int rowsRemaining = radiusToUse * 2; //Tracks amount of rows left to generate
             float horizontalDisplacement = 0; //How far the generated tile should be moved horizontally
             float verticalDisplacement = 0; //How far the generated tile should be moved vertically
             int currentRowLength = radiusToUse * 2; //Length of the current row being generated (amount of tiles)

             //This loops runs once for each row remaining
             for (int rowID = 0; rowID < rowsRemaining; rowID++)
             {
                 //If past half the rows (thus switching to lower rows), reset counters
                 if (rowID == radiusToUse)
                 {
                     horizontalDisplacement = 0;
                     verticalDisplacement = 0;
                     currentRowLength = radiusToUse * 2;
                 }

                 //For each row, update the counters
                 horizontalDisplacement = horizontalDisplacement + 0.5f;
                 currentRowLength = currentRowLength - 1;
                 //If it's an upper row
                 if (rowID < radiusToUse)
                 {
                     verticalDisplacement = verticalDisplacement + 0.866f;
                 }
                 //If it's a lower row
                 else
                 {
                     verticalDisplacement = verticalDisplacement - 0.866f;
                 }

                 //Generate the tile coordinates for this row
                 for (int tileID = 0; tileID <= currentRowLength; tileID++)
                 {
                     tileCoordinates.Add(new Vector3(verticalDisplacement, 0,
                         radiusToUse - tileID - horizontalDisplacement));
                     spiralOrderedObjects.Add(null);
                 }
             }

             //Use the generated list of coordinates to instantiate the tile prefabs
             var newTile = (GameObject) Instantiate(cellObjectPrefab, cellHolder, true);
             for (var i = 0; i < tileCoordinates.Count; i++)
             {
                 //Create new tile and name it
                 newTile.gameObject.name = "hex" +i+ tileCoordinates[i].x+"," + tileCoordinates[i].y+"," + tileCoordinates[i].z;
                 newTile.transform.localPosition = tileCoordinates[i];
                 newTile.transform.rotation = Quaternion.Euler(-90, 0, 0);
             }
             var cellSize = new Vector3(cellScale, cellScale, cellScale);
             cellHolder.localScale = cellSize;
        }
}

using UnityEngine;
using System.Collections.Generic;
using System;

//!!!!!!! To uniform terminology: every Vector2(x, y) has x = col and y = row !!!!!!!!!

// Enum that contains serializable ints to mark the state of each grid. 
public enum TileState
{
    AVAILABLE_STATE, // allows plant to be able to grow into this tile
    OCCUPIED_STATE, // prevents a plant from being planted at this tile
    WATER_STATE, // for water level. Only lilypads can be planted on water state currently
}

public class GridSquare
{
    public Vector2 gridPos; // position of grid square in grid coordinates
    public List<PlantScript> plantsOnTop; // a list of plants planted at this grid square
    public bool plantable; // plantable tiles. These are the tiles that will have a planted_grass or unplanted_grass prefab on them.
    public GameObject grassObject; // grass prefab. If this GridSquare is plantable, grassObject will be the grass prefab located at this GridSquare's gridPos. If this GridSquare is unplantable, then grassObject will be null

    public GridSquare(Vector2 gridPos, bool plantable){
        this.gridPos = gridPos;
        plantsOnTop = new List<PlantScript>();
        this.plantable = plantable;
        grassObject = null;
    }

    public void AddGrassObject(GameObject newGrass){
        grassObject = newGrass;
    }
}

// an encapsulated class with static methods to access and manipulate the CURRENT LEVEL's gridmap only. 
public class GridScript : MonoBehaviour 
{
    const float sizeFactor = 2; // default is 32 by 32 per tile. Since the sizeFactor is 2, it's now 16 by 16.
    // Make sure PPU of the map is set to 64, and that the map sprite scales to 2, 
    // has its sprite pivot set at bottom left corner, and is placed at (0, 0). 
    // Don't forget to set people PPU to 64 as well.
    const float unitSize = 1 * (1 / sizeFactor); // Keep it at 1, because already 32 ppu. 
    static int columns; // x
    static int rows; // y
    static TileState[,] mapGrid; // encapsulated 2D map coord array. 
    static bool[,] plantableGrid; // encapsulated 2D map coord array dictating whether this tile is plantable
    static LevelData levelData; // reference to save

    public GameObject square; // a visualization object. Can be deleted along with the prefab later. 
    public static GridSquare[,] mapSquare; // a 2D array representing all the squares on the screen
    public Transform squareParent; // a visualization object. Can be deleted along with the prefab later. 
    private static GameObject squareStatic; // a static version of square prefab, to be used for static methods in GridScript.
    public bool visualizeGrid; // boolean that can be set in Inspector whether to visualize grid or not

    public GameObject grass; // the prefab to visualize ground tiles
    public static GameObject grassStatic; // a static version of grass prefab, to be used for static methods in GridScript.
    public Sprite plantedGrass; // the sprite for planted tiles
    public static Sprite plantedGrassStatic; // a static version of plantedGrass sprite, to be used for static methods in GridScript.
    public Sprite unplantedGrass; // the sprite for unplanted tiles
    public static Sprite unplantedGrassStatic; // a static version of unplantedGrass sprite, to be used for static methods in GridScript.
    public static float offset = -0.13f; // offset needed to instantiate prefab at the right location

    public Transform grassParent;
    public static Transform grassParentStatic;
    public Transform plantParent;
    public static Transform plantParentStatic;

    // Set the dimension of the grid and spawn in a new grid. Important. Called between level transitions.
    public static void SpawnGrid(Vector2 levelDim, LevelData saveLevelData)
    {
        columns = (int)(levelDim.x * sizeFactor);
        rows = (int)(levelDim.y * sizeFactor);
        levelData = saveLevelData;

        if (levelData.mapGrid == null || levelData.mapGrid.Length == 0) // new level data. Spawn in default config for this level.
        {
            //Debug.Log("New Map detected, Loading in default map config: ");
            var mapGrid = GridConfigs.levelGridConfigs[LevelManager.currentLevelID](); // only need to flip for default. 
            GridConfigs.FlipRows(ref mapGrid, rows, columns);
            levelData.mapGrid = GridConfigs.Convert2DArrayTo1D(ref mapGrid, rows, columns);

            var plantableGrid = GridConfigs.levelPlantableConfigs[LevelManager.currentLevelID](); // only need to flip for default. 
            Debug.Log(plantableGrid);
            GridConfigs.FlipRows(ref plantableGrid, rows, columns);
            levelData.plantableGrid = GridConfigs.Convert2DArrayTo1D(ref plantableGrid, rows, columns);
            Debug.Log(plantableGrid);
        }

        mapGrid = new TileState[rows, columns]; // reset the map. All default to available state.
        mapSquare = new GridSquare[rows, columns]; // initialize map square array;
        for (int row = 0; row < rows; row++) // transform the map into enum state
        {
            for (int col = 0; col < columns; col++)
            {
                mapGrid[row, col] = (TileState)levelData.mapGrid[GridConfigs.TwoDIndexToOneD(row, col, columns)];
                mapSquare[row, col] = new GridSquare(new Vector2(row, col), levelData.plantableGrid[GridConfigs.TwoDIndexToOneD(row, col, columns)]);
                Vector2 grassCoords = GridToCoordinates(new Vector2(col, row)) + new Vector3(0, offset, 0);
                if (mapSquare[row, col].plantable){
                    GameObject newGrass = Instantiate(grassStatic, grassCoords, Quaternion.identity, grassParentStatic);
                    mapSquare[row, col].AddGrassObject(newGrass);
                }
            }
        }
    }

    // Math to go from world coord to a grid index
    public static Vector2 CoordinatesToGrid(Vector2 worldPos)
    {
        return new Vector2((int)(worldPos.x / unitSize), (int)(worldPos.y / unitSize));
    }

    // Math to go from a grid index to world coord
    public static Vector3 GridToCoordinates(Vector2 gridPos, float offset = 0f)
    {   // unit size is int... watch out.
        return new Vector3(gridPos.x * unitSize + unitSize / 2.0f, gridPos.y * unitSize + offset, 0);
    }

    // return the state of current gridPos
    public static TileState GetTileState(Vector2 gridPos)
    {
        // maybe switch x and y? YES
        return mapGrid[(int)gridPos.y, (int)gridPos.x];
    }

    // return the state of current gridPos
    public static GridSquare GetGridSquare(Vector2 gridPos)
    {
        // maybe switch x and y? YES
        return mapSquare[(int)gridPos.y, (int)gridPos.x];
    }

    // Ensures that the entire space is cleared.
    public static bool CheckCenterTileAvailability(Vector2 centerGridPos, GameObject prefab)
    {
        if (CheckOutOfBounds(centerGridPos) || GetTileState(centerGridPos) == TileState.AVAILABLE_STATE){
            if (prefab.TryGetComponent<PlantScript>(out PlantScript plantScript)){
                if (plantScript.plantSO.unlockPlantability){
                    return false;
                }
            }
        }
        if (CheckOutOfBounds(centerGridPos) || GetTileState(centerGridPos) == TileState.OCCUPIED_STATE)
        {
            Debug.Log("Grid " + centerGridPos.ToString() + " is occupied!");
            return false;
        }
        if (CheckOutOfBounds(centerGridPos) || GetTileState(centerGridPos) == TileState.WATER_STATE)
        {
            if (prefab.TryGetComponent<PlantScript>(out PlantScript plantScript)){
                if (plantScript.plantSO.unlockPlantability){
                    return true;
                }
            }
            Debug.Log("Relative grid " + centerGridPos.ToString() + " is a water state and you're not planting a plant that has the unlock plantability feature!");
            return false;
        }
        return true;
    }
    public static bool CheckOtherTilesAvailability(Vector2 centerGridPos, GameObject gameObject, Vector2[] additionRelativeGrids = null)
    {
        foreach (Vector2 gridPos in additionRelativeGrids)
        {
            Vector2 tile = centerGridPos + gridPos;
            if (CheckOutOfBounds(tile) || GetTileState(tile) == TileState.AVAILABLE_STATE){
                if (gameObject.TryGetComponent<PlantScript>(out PlantScript plantScript)){
                    if (plantScript.plantSO.unlockPlantability){
                        return false;
                    }
                }
            }
            if (CheckOutOfBounds(tile) || GetTileState(tile) == TileState.OCCUPIED_STATE)
            {
                Debug.Log("Relative grid " + gridPos.ToString() + " is occupied!");
                return false;
            }
            if (CheckOutOfBounds(tile) || GetTileState(tile) == TileState.WATER_STATE)
            {
                if (gameObject.TryGetComponent<PlantScript>(out PlantScript plantScript)){
                    if (plantScript.plantSO.unlockPlantability){
                        return true;
                    }
                }
                Debug.Log("Relative grid " + gridPos.ToString() + " is a water state and you're not planting a plant that has the unlock plantability feature!");
                return false;
            }
        }
        return true;
    }

    // encapsulates here, returns the instantiated object to the user. Should only be called whenever planting a new plant, not when plant is about grow up in life stage.
    public static GameObject SpawnObjectAtGrid(Vector2 centerGridPos, GameObject prefab, float offset, Vector2[] additionRelativeGrids = null)
    {
        // Check if the grid tiles satisfy the current spacing availabilities. 
        if(!CheckCenterTileAvailability(centerGridPos, prefab) || !CheckOtherTilesAvailability(centerGridPos, prefab, additionRelativeGrids) || !GetGridSquare(centerGridPos).plantable) return null; // need to make sure enough space.

        // Have space! Time to add it in. 
        if(prefab.TryGetComponent<PlantScript>(out PlantScript plantScript)){
            if (plantScript.plantSO.unlockPlantability && GetTileState(centerGridPos) == TileState.WATER_STATE){
                SetTileStates(centerGridPos, TileState.AVAILABLE_STATE, additionRelativeGrids);
            } else {
                SetTileStates(centerGridPos, TileState.OCCUPIED_STATE, additionRelativeGrids);
                Debug.Log("New Plant spawned at grid " + centerGridPos.ToString());
            }
        }
        GetGridSquare(centerGridPos).plantsOnTop.Add(prefab.GetComponent<PlantScript>());
        return Instantiate(prefab, GridToCoordinates(centerGridPos, offset), prefab.transform.rotation, plantParentStatic);
    }

    public static bool PlaceObjectAtGrid(Vector2 centerGridPos, GameObject gameObject, float offset, Vector2[] additionRelativeGrids = null)
    {
        // Check if the grid tiles satisfy the current spacing availabilities. 
        if (!CheckCenterTileAvailability(centerGridPos, gameObject) || !CheckOtherTilesAvailability(centerGridPos, gameObject, additionRelativeGrids)) return false; // need to make sure enough space.
        // Have space! Time to add it in. 
        if(gameObject.TryGetComponent<PlantScript>(out PlantScript plantScript)){
            if (plantScript.plantSO.unlockPlantability && GetTileState(centerGridPos) == TileState.WATER_STATE){
                SetTileStates(centerGridPos, TileState.AVAILABLE_STATE, additionRelativeGrids);
            } else {
                SetTileStates(centerGridPos, TileState.OCCUPIED_STATE, additionRelativeGrids);
                Debug.Log("New plant placed at grid " + centerGridPos.ToString());
            }
            Debug.Log("Plant placed at grid " + centerGridPos.ToString());
        }
        gameObject.transform.position = GridToCoordinates(centerGridPos, offset);
        GetGridSquare(centerGridPos).plantsOnTop.Add(gameObject.GetComponent<PlantScript>());
        return true;
    }

    public static void SetTileStates(Vector2 centerGridPos, TileState state, Vector2[] additionRelativeGrids = null)
    {
        mapGrid[(int)centerGridPos.y, (int)centerGridPos.x] = state;
        GridSquare gridSquare = mapSquare[(int)centerGridPos.y, (int)centerGridPos.x];
        if (state == TileState.AVAILABLE_STATE && gridSquare.plantable) gridSquare.grassObject.GetComponent<SpriteRenderer>().sprite = unplantedGrassStatic;
        if (state == TileState.OCCUPIED_STATE && gridSquare.plantable) gridSquare.grassObject.GetComponent<SpriteRenderer>().sprite = plantedGrassStatic;

        levelData.mapGrid[GridConfigs.TwoDIndexToOneD((int)centerGridPos.y, (int)centerGridPos.x, columns)] = (int)state;
        if (additionRelativeGrids != null){
            foreach (Vector2 gridPos in additionRelativeGrids)
            {
                Vector2 tile = centerGridPos + gridPos;
                mapGrid[(int)tile.y, (int)tile.x] = state;
                GridSquare additionGridSquare = mapSquare[(int)tile.y, (int)tile.x];
                if (state == TileState.AVAILABLE_STATE && additionGridSquare.plantable) additionGridSquare.grassObject.GetComponent<SpriteRenderer>().sprite = unplantedGrassStatic;
                if (state == TileState.OCCUPIED_STATE && additionGridSquare.plantable) additionGridSquare.grassObject.GetComponent<SpriteRenderer>().sprite = plantedGrassStatic;
                levelData.mapGrid[GridConfigs.TwoDIndexToOneD((int)tile.y, (int)tile.x, columns)] = (int)state;
            }
        }
    }

    // Marks that grid location as available. Also, other object's responsibility to destroy itself.
    public static void RemoveObjectFromGrid(Vector2 centerGridPos, PlantScript plantScript, Vector2[] additionRelativeGrids = null)
    {
        if (GetTileState(centerGridPos) == TileState.OCCUPIED_STATE){
            SetTileStates(centerGridPos, TileState.AVAILABLE_STATE, additionRelativeGrids);
        }
        else if (GetTileState(centerGridPos) == TileState.AVAILABLE_STATE && plantScript.plantSO.unlockPlantability){
            SetTileStates(centerGridPos, TileState.WATER_STATE, additionRelativeGrids);
        }
        mapSquare[(int)centerGridPos.y, (int)centerGridPos.x].plantsOnTop.Remove(plantScript);
    }

    private static bool CheckOutOfBounds(Vector2 tile) // (col, row)
    {
        int col = (int)tile.x, row = (int)tile.y;
        return row < 0 || row >= rows || col < 0 || col >= columns;
    }

    // Visualization of the grid squares. Internal testing function. Important! Don't delete. Save for manual map measurements.
    public void VisualizeGrids()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 worldLoc = GridToCoordinates(new Vector2(col, row)); // col is x, row is y
                var sprObj = Instantiate(square, worldLoc, Quaternion.identity, squareParent);
                sprObj.transform.localScale = new Vector3(0.1f, 0.1f, 1);
                // Blue is available, red is occupied, green is water. 
                if (mapGrid[row, col] == TileState.AVAILABLE_STATE) sprObj.GetComponent<SpriteRenderer>().color = Color.blue;
                else if (mapGrid[row, col] == TileState.OCCUPIED_STATE) sprObj.GetComponent<SpriteRenderer>().color = Color.red;
                else sprObj.GetComponent<SpriteRenderer>().color = Color.green;
            }
        }
    }
    
    void Awake(){
        squareStatic = square;
        grassStatic = grass;
        plantedGrassStatic = plantedGrass;
        unplantedGrassStatic = unplantedGrass;
        grassParentStatic = grassParent;
        plantParentStatic = plantParent;
    }
    
    // Delete the below when ready. Understand its application. 
    private void Start()
    {
        // Example of how a grid should be set at the beginning of each level. 
        SpawnGrid(GridConfigs.levelGridDimensions[LevelManager.currentLevelID], 
            PersistentData.GetLevelData(LevelManager.currentLevelID));

        if (visualizeGrid){ VisualizeGrids(); }
    }

    // Delete below when ready. They are just debugging print statements. 
    /*
    Vector2 prevGrid;
    private void Update()
    {
        var currGrid = CoordinatesToGrid(FindObjectOfType<PlayerScript>().transform.position);

        if(currGrid != prevGrid)
        {
            Debug.Log("New grid: " + currGrid.ToString());
            prevGrid = currGrid;
        }
    } */
}

// a data container class that stores manual configs and dims for each map
public class GridConfigs
{
    // 0 is available state, 1 is occupied state (also same as prohibited, insect, etc)
    // these are default configurations! By doing so, dims don't have to unite
    public static Func<int[,]>[] levelGridConfigs =
    {
        // Level 0 (The current sample scene rn)
        generateLevel00MapGrid,
        // Insert level 1 function below.
    };

    public static Func<bool[,]>[] levelPlantableConfigs = {
        // Level 0 (The current sample scene rn)
        generateLevel00PlantableGrid,
    };

    // x is width (num columns), y is height (num rows)
    public static Vector2[] levelGridDimensions =
    {
        // Level 0
        new Vector2( 30, 10-2 ) // derive from picture dim, manually readjust floor, and remove useless rows or cols
    };

    static int[,] generateLevel00MapGrid()
    {
        /*
        int[,] gridMapDefault = // this is a 32x32 grid tile map
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        }; */
        int[,] gridMapDefault = // this is a 16 x 16 grid tile map
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        return gridMapDefault;
    }

    static bool[,] generateLevel00PlantableGrid()
    {
        bool[,] gridPlantableDefault = // this is a 16 x 16 grid tile map. True represents plantable and false represents unplantable
        {
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false },
            { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true },
        };
        return gridPlantableDefault;
    }

    // this is a reason why multidimension is inconvenient... can't just simply cycle through rows...
    // need to flip rows because y indices are flipped visually in default vs in game. Not that much run time w/e.
    public static void FlipRows(ref int[,] upsideDownRepresentation, int rows, int cols)
    {
        for(int row = 0; row < rows / 2; row++)
        {
            for(int col = 0; col < cols; col++)
            {
                int temp = upsideDownRepresentation[row, col];
                upsideDownRepresentation[row, col] = upsideDownRepresentation[rows - 1 - row, col];
                upsideDownRepresentation[rows - 1 - row, col] = temp;
            }
        }
    }

    public static void FlipRows(ref bool[,] upsideDownRepresentation, int rows, int cols)
    {
        for(int row = 0; row < rows / 2; row++)
        {
            for(int col = 0; col < cols; col++)
            {
                bool temp = upsideDownRepresentation[row, col];
                upsideDownRepresentation[row, col] = upsideDownRepresentation[rows - 1 - row, col];
                upsideDownRepresentation[rows - 1 - row, col] = temp;
            }
        }
    }

    public static int[] Convert2DArrayTo1D(ref int[,] twoDArray, int rows, int cols)
    {
        int[] converted = new int[rows * cols];
        for(int row = 0; row < rows; row++)
        {
            for(int col = 0; col < cols; col++)
            {
                converted[TwoDIndexToOneD(row, col, cols)] = twoDArray[row, col];
            }
        }
        return converted;
    }

    public static bool[] Convert2DArrayTo1D(ref bool[,] twoDArray, int rows, int cols)
    {
        bool[] converted = new bool[rows * cols];
        for(int row = 0; row < rows; row++)
        {
            for(int col = 0; col < cols; col++)
            {
                converted[TwoDIndexToOneD(row, col, cols)] = twoDArray[row, col];
            }
        }
        return converted;
    }

    public static int TwoDIndexToOneD(int row, int col, int numCols)
    {
        return row * numCols + col;
    }

    public static Vector2 OneDIndexToTwoD(int index, int numCols)
    {
        return new Vector2( (index / numCols) , (index % numCols) ); // row, col
    }
}

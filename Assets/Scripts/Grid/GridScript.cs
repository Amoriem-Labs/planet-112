using UnityEngine;
using System.Collections.Generic;
using System;

//!!!!!!! To uniform terminology: every Vector2(x, y) has x = col and y = row !!!!!!!!!

// Enum that contains serializable ints to mark the state of each grid. 
public enum TileState
{
    AVAILABLE_STATE, // default state
    OCCUPIED_STATE,
    WATER_STATE,
}

public class GridSquare
{
    public Vector2 gridPos; // position of grid square in grid coordinates
    public List<PlantScript> plantsOnTop; // a list of plants planted at this grid square

    public GridSquare(Vector2 gridPos){
        this.gridPos = gridPos;
        plantsOnTop = new List<PlantScript>();
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
    static LevelData levelData; // reference to save

    public GameObject square; // a visualization object. Can be deleted along with the prefab later. 
    public static GridSquare[,] mapSquare; // a 2D array representing all the squares on the screen
    private static GameObject squareStatic; // a static version of square prefab, to be used for static methods in GridScript.
    public bool visualizeGrid;

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
        }

        mapGrid = new TileState[rows, columns]; // reset the map. All default to available state.
        mapSquare = new GridSquare[rows, columns]; // initialize map square array;
        for (int row = 0; row < rows; row++) // transform the map into enum state
        {
            for (int col = 0; col < columns; col++)
            {
                mapGrid[row, col] = (TileState)levelData.mapGrid[GridConfigs.TwoDIndexToOneD(row, col, columns)];
                mapSquare[row, col] = new GridSquare(new Vector2(row, col));
            }
        }
    }

    // Math to go from world coord to a grid index
    public static Vector2 CoordinatesToGrid(Vector2 worldPos)
    {
        return new Vector2((int)(worldPos.x / unitSize), (int)(worldPos.y / unitSize));
    }

    // Math to go from a grid index to world coord
    public static Vector3 GridToCoordinates(Vector2 gridPos)
    {   // unit size is int... watch out.
        return new Vector3(gridPos.x * unitSize + unitSize / 2.0f, gridPos.y * unitSize, 0);
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

    // encapsulates here, returns the instantiated object to the user.
    public static GameObject SpawnObjectAtGrid(Vector2 centerGridPos, GameObject prefab, Vector2[] additionRelativeGrids = null)
    {
        print(mapGrid[(int)centerGridPos.y, (int)centerGridPos.x]);
        // Check if the grid tiles satisfy the current spacing availabilities. 
        if(!CheckCenterTileAvailability(centerGridPos, prefab) || !CheckOtherTilesAvailability(centerGridPos, prefab, additionRelativeGrids)) return null; // need to make sure enough space.

        // Have space! Time to add it in. 
        if(prefab.TryGetComponent<PlantScript>(out PlantScript plantScript)){
            if (plantScript.plantSO.unlockPlantability && GetTileState(centerGridPos) == TileState.WATER_STATE){
                SetTileStates(centerGridPos, TileState.AVAILABLE_STATE, additionRelativeGrids);
            } else {
                SetTileStates(centerGridPos, TileState.OCCUPIED_STATE, additionRelativeGrids);
                Debug.Log("New Plant spawned at grid " + centerGridPos.ToString());
            }
        }
        
        mapSquare[(int)centerGridPos.y, (int)centerGridPos.x].plantsOnTop.Add(prefab.GetComponent<PlantScript>());
        return Instantiate(prefab, GridToCoordinates(centerGridPos), prefab.transform.rotation);
    }

    public static bool PlaceObjectAtGrid(Vector2 centerGridPos, GameObject gameObject, Vector2[] additionRelativeGrids = null)
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
        gameObject.transform.position = GridToCoordinates(centerGridPos);
        mapSquare[(int)centerGridPos.y, (int)centerGridPos.x].plantsOnTop.Add(gameObject.GetComponent<PlantScript>());
        return true;
    }

    public static void SetTileStates(Vector2 centerGridPos, TileState state, Vector2[] additionRelativeGrids = null)
    {
        mapGrid[(int)centerGridPos.y, (int)centerGridPos.x] = state;
        levelData.mapGrid[GridConfigs.TwoDIndexToOneD((int)centerGridPos.y, (int)centerGridPos.x, columns)] = (int)state;
        if (additionRelativeGrids != null){
            foreach (Vector2 gridPos in additionRelativeGrids)
            {
                Vector2 tile = centerGridPos + gridPos;
                mapGrid[(int)tile.y, (int)tile.x] = state;
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
        else if (GetTileState(centerGridPos) == TileState.AVAILABLE_STATE){
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
                var sprObj = Instantiate(square, worldLoc, Quaternion.identity);
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
        generateLevel00MapGrid
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
        int[,] gridMapDefault = // this is a 16 x 16 grid tild map
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

    public static int TwoDIndexToOneD(int row, int col, int numCols)
    {
        return row * numCols + col;
    }

    public static Vector2 OneDIndexToTwoD(int index, int numCols)
    {
        return new Vector2( (index / numCols) , (index % numCols) ); // row, col
    }
}

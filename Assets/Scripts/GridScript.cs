using UnityEngine;
using System.Collections.Generic;
using System;

//!!!!!!! To uniform terminology: every Vector2(x, y) has x = col and y = row !!!!!!!!!

// Enum that contains serializable ints to mark the state of each grid. 
public enum TileState
{
    AVAILABLE_STATE, // default state
    OCCUPIED_STATE,
}

// an encapsulated class with static methods to access and manipulate the CURRENT LEVEL's gridmap only. 
public class GridScript : MonoBehaviour 
{
    // Make sure PPU of the map is set to 64, and that the map sprite scales to 2, 
    // has its sprite pivot set at bottom left corner, and is placed at (0, 0). 
    // Don't forget to set people PPU to 64 as well.
    const int unitSize = 1; // Keep it at 1, because already 32 ppu. 
    static int columns; // x
    static int rows; // y
    static TileState[,] mapGrid; // encapsulated 2D map coord array. 
    static LevelData levelData; // reference to save

    public GameObject square; // a visualization object. Can be deleted along with the prefab later. 

    // Set the dimension of the grid and spawn in a new grid. Important. Called between level transitions.
    public static void SpawnGrid(Vector2 levelDim, LevelData saveLevelData)
    {
        columns = (int)levelDim.x;
        rows = (int)levelDim.y;
        levelData = saveLevelData;

        if (levelData.mapGrid == null || levelData.mapGrid.Length == 0) // new level data. Spawn in default config for this level.
        {
            Debug.Log("New Map detected, Loading in default map config: ");
            var mapGrid = GridConfigs.levelGridConfigs[LevelManager.currentLevelID](); // only need to flip for default. 
            GridConfigs.FlipRows(ref mapGrid, rows, columns);
            levelData.mapGrid = GridConfigs.Convert2DArrayTo1D(ref mapGrid, rows, columns);
        }

        mapGrid = new TileState[rows, columns]; // reset the map. All default to available state.
        for (int row = 0; row < rows; row++) // transform the map into enum state
        {
            for (int col = 0; col < columns; col++)
            {
                mapGrid[row, col] = (TileState)levelData.mapGrid[GridConfigs.TwoDIndexToOneD(row, col, columns)];
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

    // encapsulates here, returns the instantiated object to the user.
    // TODO: pass in a list or width/height of gridPositions to disable, ex. a wide and tall tree occupies more than 1 space. 
    public static GameObject SpawnObjectAtGrid(Vector2 centerGridPos, GameObject prefab, Vector2[] additionRelativeGrids = null)
    {
        // Check if the grid tiles satisfy the current spacing availabilities. 
        if (CheckOutOfBounds(centerGridPos) || GetTileState(centerGridPos) == TileState.OCCUPIED_STATE)
        {
            Debug.Log("Grid " + centerGridPos.ToString() + " is occupied!");
            return null;
        }
        foreach (Vector2 gridPos in additionRelativeGrids)
        {
            Vector2 tile = centerGridPos + gridPos;
            if (CheckOutOfBounds(tile) || GetTileState(tile) == TileState.OCCUPIED_STATE)
            {
                Debug.Log("Relative grid " + gridPos.ToString() + " is occupied!");
                return null;
            }
        }

        // Have space! Time to add it in. 
        mapGrid[(int)centerGridPos.y, (int)centerGridPos.x] = TileState.OCCUPIED_STATE;
        levelData.mapGrid[GridConfigs.TwoDIndexToOneD((int)centerGridPos.y, (int)centerGridPos.x, columns)] = (int)TileState.OCCUPIED_STATE;
        foreach (Vector2 gridPos in additionRelativeGrids)
        {
            Vector2 tile = centerGridPos + gridPos;
            mapGrid[(int)tile.y, (int)tile.x] = TileState.OCCUPIED_STATE;
            levelData.mapGrid[GridConfigs.TwoDIndexToOneD((int)tile.y, (int)tile.x, columns)] = (int)TileState.OCCUPIED_STATE;
        }
        Debug.Log("New Plant spawned at grid " + centerGridPos.ToString());
        
        return Instantiate(prefab, GridToCoordinates(centerGridPos), prefab.transform.rotation);
    }

    // Marks that grid location as available. Also, other object's responsibility to destroy itself.
    // TODO: pass in a list or width/height of gridPositions to enable, ex. a wide and tall tree occupies more than 1 space. 
    public static void RemoveObjectFromGrid(Vector2 centerGridPos, Vector2[] additionRelativeGrids = null)
    {
        mapGrid[(int)centerGridPos.y, (int)centerGridPos.x] = TileState.AVAILABLE_STATE;
        levelData.mapGrid[GridConfigs.TwoDIndexToOneD((int)centerGridPos.y, (int)centerGridPos.x, columns)] = (int)TileState.AVAILABLE_STATE;
        foreach (Vector2 gridPos in additionRelativeGrids)
        {
            Vector2 tile = centerGridPos + gridPos;
            mapGrid[(int)tile.y, (int)tile.x] = TileState.AVAILABLE_STATE;
            levelData.mapGrid[GridConfigs.TwoDIndexToOneD((int)tile.y, (int)tile.x, columns)] = (int)TileState.AVAILABLE_STATE;
        }
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
                // Blue is available, red is occupied. 
                if (mapGrid[row, col] == TileState.AVAILABLE_STATE) sprObj.GetComponent<SpriteRenderer>().color = Color.blue;
                else sprObj.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }

    // Delete the below when ready. Understand its application. 
    private void Start()
    {
        // Example of how a grid should be set at the beginning of each level. 
        SpawnGrid(GridConfigs.levelGridDimensions[LevelManager.currentLevelID], 
            PersistentData.GetLevelData(LevelManager.currentLevelID));

        VisualizeGrids();
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
        int[,] gridMapDefault =
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
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

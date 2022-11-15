using UnityEngine;
using System.Collections.Generic;

// an encapsulated class with static methods to access and manipulate the CURRENT LEVEL's gridmap only. 
public class GridScript : MonoBehaviour 
{
    // Make sure PPU of the map is set to 64, and that the map sprite scales to 2, 
    // has its sprite pivot set at bottom left corner, and is placed at (0, 0). 
    // Don't forget to set people PPU to 64 as well.
    const int unitSize = 1; // Keep it at 1, because already 32 ppu. 
    static int columns; // x
    static int rows; // y
    static State[,] array; // encapsulated 2D map coord array. 

    public GameObject square; // a visualization object. Can be deleted along with the prefab later. 

    // Enum that contains serializable ints to mark the state of each grid. 
    public enum State
    {
        AVAILABLE_STATE,
        OCCUPIED_STATE
    }

    // Set the dimension of the grid and spawn in a new grid. Important. Called between level transitions.
    public static void SetGridDim(int numCols, int numRows)
    {
        columns = numCols;
        rows = numRows;
        array = new State[rows, columns];
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
    public static State GetState(Vector2 gridPos)
    {
        // maybe switch x and y? YES
        return array[(int)gridPos.y, (int)gridPos.x];
    }

    // encapsulates here, returns the instantiated object to the user.
    // TODO: add in dynamic data save editing
    // TODO: pass in a list or width/height of gridPositions to disable, ex. a wide and tall tree occupies more than 1 space. 
    public static GameObject SpawnObjectAtGrid(Vector2 gridPos, GameObject prefab)
    {
        if (GetState(gridPos) == State.OCCUPIED_STATE)
        {
            Debug.Log("Grid " + gridPos.ToString() + " is occupied!");
            return null;
        }
        else
        {
            Debug.Log("New Plant spawned at grid " + gridPos.ToString());
            array[(int)gridPos.y, (int)gridPos.x] = State.OCCUPIED_STATE;
            return Instantiate(prefab, GridToCoordinates(gridPos), prefab.transform.rotation);
        }
    }

    // Marks that grid location as available. Also, other object's responsibility to destroy itself.
    // TODO: add in dynamic data save editing
    // TODO: pass in a list or width/height of gridPositions to enable, ex. a wide and tall tree occupies more than 1 space. 
    public static void RemoveObjectFromGrid(Vector2 gridPos)
    {
        array[(int)gridPos.y, (int)gridPos.x] = State.AVAILABLE_STATE;
    }


    // Delete the below when ready. Understand its application. 
    private void Start()
    { 
        // Example of how a grid should be set at the beginning of each level. 
        int numCols = 30, numRows = 10 - 2; // derive from picture dim, manually readjust floor, and remove useless rows or cols
        SetGridDim(numCols, numRows);

        // Visualization of the grid squares. 
        for(int row=0; row<rows; row++)
        {
            for(int col=0; col<columns; col++)
            {
                Vector3 worldLoc = GridToCoordinates(new Vector2(col, row)); // col is x, row is y
                var sprObj = Instantiate(square, worldLoc, Quaternion.identity);
                sprObj.transform.localScale = new Vector3(0.1f, 0.1f, 1);
            }
        }
    }

    // Delete below when ready. They are just debugging print statements. 
    Vector2 prevGrid;
    private void Update()
    {
        var currGrid = CoordinatesToGrid(FindObjectOfType<PlayerScript>().transform.position);

        if(currGrid != prevGrid)
        {
            Debug.Log("New grid: " + currGrid.ToString());
            prevGrid = currGrid;
        }
    } 
}

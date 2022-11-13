using UnityEngine;

public class GridScript : MonoBehaviour
{
    // replace hard code with level width and height divided by 32
    const int unitSize = 32;
    const int columns = 30;
    const int rows = 10;

    enum State
    {
        AVAILABLE_STATE,
        OCCUPIED_STATE
    }

    State[,] array = new State[rows, columns];

    Vector2 CoordinatesToGrid(Vector2 worldPos)
    {
        return new Vector2((int)(worldPos.x / unitSize), (int)(worldPos.y / unitSize));
    }

    Vector2 GridToCoordinates(Vector2 gridPos)
    {
        return new Vector2(gridPos.x * unitSize + unitSize / 2, gridPos.y * unitSize);
    }

    State GetState(Vector2 place)
    {
        // maybe switch x and y?
        return array[(int)place.x, (int)place.y];
    }
}

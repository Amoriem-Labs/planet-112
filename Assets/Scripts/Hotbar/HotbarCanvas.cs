using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarCanvas : MonoBehaviour
{
    public GameObject player;

    // This script is to update hotbar position so that it moves along with the player in the game.
    void Update()
    {
        var pos = transform.position;
        pos.x = player.transform.position.x;
        transform.position = pos;
    }
}

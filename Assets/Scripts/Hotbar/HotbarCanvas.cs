using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarCanvas : MonoBehaviour
{
    public Camera cam;

    // This script is to update hotbar position so that it moves along with the camera in the game.
    void Update()
    {
        var pos = transform.position;
        pos.x = cam.transform.position.x;
        transform.position = pos;
    }
}

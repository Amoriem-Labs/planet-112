using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarCanvas : MonoBehaviour
{
    public GameObject player;
    public float y_offset;

    void Start(){
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
    }

    // This script is to update hotbar position so that it moves along with the camera in the game.
    void Update()
    {
        var pos = transform.position;
        pos.x = player.transform.position.x;
        pos.y = player.transform.position.y + y_offset;
        transform.position = pos;
    }
}

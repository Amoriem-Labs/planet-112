using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCanvas : MonoBehaviour
{
    public GameObject player;
    public float y_offset;

    // This script is to update the position of InventoryCanvas to match camera position every time inventory is loaded.
    void Update()
    {
        if (player.GetComponent<PlayerScript>().inventoryIsLoaded){
            var pos = transform.position;
            pos.x = player.transform.position.x;
            pos.y = player.transform.position.y + y_offset;
            transform.position = pos;
        }
    }
}

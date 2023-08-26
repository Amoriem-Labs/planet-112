using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedPacket : MonoBehaviour, ICollectible
{    
    private GameObject player;
    public PlantName plantName;
    public GameObject linkedInventoryItem;

    public void Collect(){
        // seed is not meant to be collected off the ground, so this method is just here to avoid compile error from not implementing Collect() from the interface
    }

    // Is triggered whenever player uses seed in hotbar.
    public void Use(){
        player = GameObject.FindGameObjectWithTag("Player");
        GameObject plantPrefab = GameManager.SpawnPlant(plantName, GridScript.CoordinatesToGrid(player.transform.position));
        if (plantPrefab != null){
            InventoryItem inventoryItem = linkedInventoryItem.GetComponent<InventoryItem>();
            if (inventoryItem.stackable){
                inventoryItem.Use(1, inventoryItem.gameObject);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedPacket : MonoBehaviour, ICollectible
{    
    private GameObject player;
    public string displayName;
    public PlantName plantName;

    [HideInInspector]public InventoryItem linkedInventoryItem;

    public void LinkInventoryItem(InventoryItem inventoryItem){
        linkedInventoryItem = inventoryItem;
    }

    public void Collect(){
        // seed is not meant to be collected off the ground, so this method is just here to avoid compile error from not implementing Collect() from the interface
    }

    // Is triggered whenever player uses seed in hotbar.
    public void Use(){
        player = GameObject.FindGameObjectWithTag("Player");
        GameObject plantPrefab = GameManager.SpawnPlant(plantName, GridScript.CoordinatesToGrid(player.transform.position));
        if (plantPrefab != null){
            if (linkedInventoryItem != null){
                if (linkedInventoryItem.stackable) linkedInventoryItem.Use(1, linkedInventoryItem.gameObject);
            } else {
                ////Debug.Log("Didn't find the matching seed packet in inventory!");
            }
        }        
    }
}

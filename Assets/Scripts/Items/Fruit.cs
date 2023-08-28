using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Fruit : MonoBehaviour, ICollectible
{
    public delegate void HandleFruitCollected(GameObject fruitInventoryPrefab);
    public static event HandleFruitCollected OnFruitCollected;
    public GameObject fruitInventoryPrefab;
    public string fruitType;
    [HideInInspector]public InventoryItem linkedInventoryItem;

    public void LinkInventoryItem(InventoryItem inventoryItem){
        linkedInventoryItem = inventoryItem;
    }

    public void Collect(){
        AudioManager.GetSFX("collectFruitSFX").Play();
        Destroy(gameObject);
        OnFruitCollected?.Invoke(fruitInventoryPrefab);
    }

    // Is triggered whenever player uses fruit in hotbar.
    public void Use(){
        //print("using fruit");
    }
}

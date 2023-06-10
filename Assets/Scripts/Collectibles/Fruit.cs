using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Fruit : MonoBehaviour, ICollectible
{
    public delegate void HandleFruitCollected(GameObject fruitInventoryPrefab);
    public static event HandleFruitCollected OnFruitCollected;
    public GameObject fruitInventoryPrefab;

    public void Collect(){
        Destroy(gameObject);
        OnFruitCollected?.Invoke(fruitInventoryPrefab);
        // TODO: play audio when collectible is collected
    }
}

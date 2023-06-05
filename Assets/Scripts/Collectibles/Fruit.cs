using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Fruit : MonoBehaviour, ICollectible
{
    public delegate void HandleFruitCollected(ItemData itemData);
    public static event HandleFruitCollected OnFruitCollected;
    public ItemData fruitData;

    public void Collect(){
        Destroy(gameObject);
        OnFruitCollected?.Invoke(fruitData);
        // TODO: play audio when collectible is collected
    }
}

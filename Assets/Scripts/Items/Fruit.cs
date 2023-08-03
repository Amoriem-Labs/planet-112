using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Fruit : MonoBehaviour, ICollectible
{
    public delegate void HandleFruitCollected(GameObject fruitInventoryPrefab);
    public static event HandleFruitCollected OnFruitCollected;
    public GameObject fruitInventoryPrefab;
    public AudioManager audioManager;
    public string fruitType;

    void Awake(){
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public void Collect(){
        audioManager.collectFruitSFX.Play();
        Destroy(gameObject);
        OnFruitCollected?.Invoke(fruitInventoryPrefab);
    }

    // Is triggered whenever player uses fruit in hotbar.
    public void Use(){
        //print("using fruit");
    }
}

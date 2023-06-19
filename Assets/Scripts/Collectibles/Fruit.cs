using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Fruit : MonoBehaviour, ICollectible
{
    public delegate void HandleFruitCollected(GameObject fruitInventoryPrefab);
    public static event HandleFruitCollected OnFruitCollected;
    public GameObject fruitInventoryPrefab;
    public AudioManager audio;
    public string fruitType;

    void Awake(){
        audio = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public void Collect(){
        audio.collectFruitSFX.Play();
        Destroy(gameObject);
        OnFruitCollected?.Invoke(fruitInventoryPrefab);
    }

    public void Use(){
        print("using fruit");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : MonoBehaviour, ICollectible
{
    public GameObject seedInventoryPrefab;
    public AudioManager audioManager;

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public void Collect(){
        // seed is not meant to be collected off the ground, so this method is just here to avoid compile error from not implementing Collect() from the interface
    }

    // Is triggered whenever player uses seed in hotbar.
    public void Use(){
        //print("planting seed");
        //TODO: implement planting of a specific seed
        audioManager.plantSFX.Play();
    }
}

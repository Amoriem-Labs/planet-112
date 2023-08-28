using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillerItem : MonoBehaviour, ICollectible
{
    public AudioManager audioManager;
    [HideInInspector]public InventoryItem linkedInventoryItem;

    void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public void LinkInventoryItem(InventoryItem inventoryItem){
        linkedInventoryItem = inventoryItem;
    }

    public void Collect(){
        // filler item is not meant to be collected off the ground, so this method is just here to avoid compile error from not implementing Collect() from the interface
    }

    public void Use(){
        // filler item is not meant to be used, so this method is just here to avoid compile error from not implementing Use() from the interface
    }
}

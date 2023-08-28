using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicPot : MonoBehaviour, ICollectible
{
    //public AudioManager audioManager;
    public PlayerScript player;
    [HideInInspector]public InventoryItem linkedInventoryItem;

    public void LinkInventoryItem(InventoryItem inventoryItem){
        linkedInventoryItem = inventoryItem;
    }

    void Awake()
    {
        //audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }

    public void Collect(){
        // filler item is not meant to be collected off the ground, so this method is just here to avoid compile error from not implementing Collect() from the interface
    }

    public void Use(){
        player.magicPotActivated = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedPacket : MonoBehaviour, ICollectible
{    
    private GameObject player;
    public PlantName plantName;

    public void Collect(){
        // seed is not meant to be collected off the ground, so this method is just here to avoid compile error from not implementing Collect() from the interface
    }

    // Is triggered whenever player uses seed in hotbar.
    public void Use(){
        player = GameObject.FindGameObjectWithTag("Player");
        GameManager.SpawnPlant(plantName, GridScript.CoordinatesToGrid(player.transform.position));
        AudioManager.GetSFX("plantSFX").Play();
    }
}

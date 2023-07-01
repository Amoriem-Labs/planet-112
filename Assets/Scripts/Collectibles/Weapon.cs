using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Weapon : MonoBehaviour, ICollectible
{
    public delegate void HandleWeaponCollected(GameObject weaponInventoryPrefab);
    public static event HandleWeaponCollected OnWeaponCollected;
    public GameObject weaponInventoryPrefab;
    public AudioManager audioManager;

    void Awake(){
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    // Is triggered whenever player picks up stick off ground.
    public void Collect(){
        audioManager.collectGenericSFX.Play();
        Destroy(gameObject);
        OnWeaponCollected?.Invoke(weaponInventoryPrefab);
    }

    // Is triggered whenever player uses stick to attack.
    public void Use(){
        print("using stick");
    }
}

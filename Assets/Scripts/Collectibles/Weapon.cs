using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Weapon : MonoBehaviour, ICollectible
{
    public delegate void HandleWeaponCollected(GameObject weaponInventoryPrefab);
    public static event HandleWeaponCollected OnWeaponCollected;
    public GameObject weaponInventoryPrefab;
    public AudioManager audio;

    void Awake(){
        audio = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public void Collect(){
        audio.collectGenericSFX.Play();
        Destroy(gameObject);
        OnWeaponCollected?.Invoke(weaponInventoryPrefab);
    }

    public void Use(){
        print("using stick");
    }
}

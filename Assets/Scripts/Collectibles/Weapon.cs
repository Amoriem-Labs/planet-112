using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Weapon : MonoBehaviour, ICollectible
{
    public delegate void HandleWeaponCollected(GameObject weaponInventoryPrefab);
    public static event HandleWeaponCollected OnWeaponCollected;
    public GameObject weaponInventoryPrefab;

    public void Collect(){
        Destroy(gameObject);
        OnWeaponCollected?.Invoke(weaponInventoryPrefab);
        // TODO: play audio when collectible is collected
    }
}

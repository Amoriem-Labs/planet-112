using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Weapon : MonoBehaviour, ICollectible
{
    public delegate void HandleWeaponCollected(GameObject weaponInventoryPrefab);
    public static event HandleWeaponCollected OnWeaponCollected;
    public GameObject weaponInventoryPrefab;
    public string weaponName;

    // Is triggered whenever player picks up stick off ground.
    public void Collect(){
        AudioManager.GetSFX("collectGenericSFX").Play();
        Destroy(gameObject);
        OnWeaponCollected?.Invoke(weaponInventoryPrefab);
    }

    // Is triggered whenever player uses stick to attack.
    public void Use(){
        if (weaponName.Equals("Stick")){
            AudioManager.GetSFX("thudSFX").Play();
        }
        if (weaponName.Equals("Bow")){
            AudioManager.GetSFX("arrowSFX").Play();
        }
    }
}

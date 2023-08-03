using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
// Blueprint for easily designing new shop items types
public class ShopWeapon : ShopItem
{
    public string range; // range of weapon
}
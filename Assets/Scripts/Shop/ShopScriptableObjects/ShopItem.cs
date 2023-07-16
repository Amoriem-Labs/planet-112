using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Item", menuName = "Shop Item")]
public class ShopItem : ScriptableObject
{
    public string itemName; // name of item
    public string rarity; // rarity of item
    public int[] cost = new int[4]; // 1D array of size 4 with each element representing the cost in each fruit type. Index 0 represents the amount of seafoam icura the item costs, index 1 represents the amount of sunset icura the item costs, index 2 represents the represents the amount of amethyst icura the item costs, and index 3 represents the amount of crystalline icura the item costs
    public GameObject inventoryItemPrefab; // prefab that will be used to represent the shop item in the inventory UI
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Starting Item", menuName = "Starting Item")]
public class StartingItemSO : ScriptableObject
{
    public string itemName;
    public int inventorySlotIndex;
    public GameObject itemPrefab;
    public int count;
}

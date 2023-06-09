using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Inventory : MonoBehaviour
{
    public static event Action<List<InventoryItem>> OnInventoryChange;

    public List<InventoryItem> inventory = new List<InventoryItem>();
    private Dictionary<ItemData, InventoryItem> itemDictionary = new Dictionary<ItemData, InventoryItem>();
    public GameObject slotPrefab;
    
    private void OnEnable(){
        Fruit.OnFruitCollected += Add;
    }

    private void OnDisabled(){
        Fruit.OnFruitCollected -= Add;
    }

    // Adds new item to list and dictionary if item is not already in dictionary; otherwise, increase stack size of item in dictionary
    public void Add(ItemData itemData){
        if (itemDictionary.TryGetValue(itemData, out InventoryItem item)){
            item.AddToStack();
            //Debug.Log($"{item.itemData.displayName} total stack is now {item.stackSize}.");
            OnInventoryChange?.Invoke(inventory);
        }
        else
        {
            InventoryItem newItem = new InventoryItem(itemData);
            inventory.Add(newItem);
            itemDictionary.Add(itemData, newItem);
            //Debug.Log($"Added {itemData.displayName} to the inventory for the first time.");
            OnInventoryChange?.Invoke(inventory);
        }
    }

    public void Remove(ItemData itemData){
        if (itemDictionary.TryGetValue(itemData, out InventoryItem item)){
            item.RemoveFromStack();
            if (item.stackSize == 0){
                inventory.Remove(item);
                itemDictionary.Remove(itemData);
            }
            OnInventoryChange?.Invoke(inventory);
        }
    }
}

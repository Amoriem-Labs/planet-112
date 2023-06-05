using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int stackSize;

    public InventoryItem(ItemData item){
        itemData = item;
        if (itemData.stackable){
            AddToStack();
        }
        else {
            stackSize = 1;
        }
    }

    public void AddToStack(){
        stackSize++;
    }

    public void RemoveFromStack(){
        stackSize--;
    }
}

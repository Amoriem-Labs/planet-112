using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Trash : MonoBehaviour, IDropHandler
{
    // Upon dropping item into trash icon, delete that item from inventory.
    public void OnDrop(PointerEventData eventData){
        GameObject dropped = eventData.pointerDrag;
        InventoryItem droppedInventoryItem = dropped.GetComponent<InventoryItem>();
        droppedInventoryItem.Delete();
    }
}

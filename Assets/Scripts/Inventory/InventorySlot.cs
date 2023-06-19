using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Transform slotTransform;
    public int inventorySlotIndex;
    public InfoBarScript infoBar;

    // Initializing variables
    void Awake(){
        slotTransform = transform.GetChild(0);
        infoBar = GameObject.FindGameObjectWithTag("infoBar").GetComponent<InfoBarScript>();
    }

    // Deletes item in slot.
    public void ClearSlot(){
        if (slotTransform.childCount > 0){
            Transform itemTransform = slotTransform.GetChild(0);
            Destroy(itemTransform.gameObject);
            infoBar.UndisplayInfo();
        }
    }

    // Creates new item in slot.
    public void DrawSlot(GameObject inventoryItemPrefab){
        GameObject newInventoryItem = Instantiate(inventoryItemPrefab, slotTransform);
        newInventoryItem.GetComponent<InventoryItem>().AddToStack();
    }

    // Drops an item into a current inventory slot upon dragging. If the current slot already
    //   has an item, then swap the two items.
    public void OnDrop(PointerEventData eventData){
        GameObject dropped = eventData.pointerDrag;
        InventoryItem droppedInventoryItem = dropped.GetComponent<InventoryItem>();
        
        if (slotTransform.childCount > 0){
            InventoryItem oldInventoryItem = slotTransform.GetComponentInChildren<InventoryItem>();
            oldInventoryItem.transform.SetParent(droppedInventoryItem.parentAfterDrag);
            oldInventoryItem.parentAfterDrag = droppedInventoryItem.parentAfterDrag;
        }
        droppedInventoryItem.parentAfterDrag = transform.GetChild(0).transform; // sets the parent of the dragged item to Slot transform
        droppedInventoryItem.transform.SetParent(droppedInventoryItem.parentAfterDrag);

        InventoryManager inventoryManager = transform.GetComponentInParent<InventoryManager>();
        HotbarManagerScript hotbarManager = inventoryManager.hotbar.GetComponent<HotbarManagerScript>();
        hotbarManager.UpdateHotbar();
    }

    // Displays info text upon pointer entering slot
    public void OnPointerEnter(PointerEventData eventData){
        if (slotTransform.childCount > 0){
            infoBar.DisplayInfo(transform.GetComponentInChildren<InventoryItem>());
        }
    }

    // Undisplay info text upon pointer exiting slot
    public void OnPointerExit(PointerEventData eventData){
        infoBar.UndisplayInfo();
    }
}

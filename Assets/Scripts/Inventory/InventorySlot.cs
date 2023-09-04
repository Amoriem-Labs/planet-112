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
    public Transform linkedSellSlotTransform;
    public InfoBarScript infoBar;

    // Initializing variables
    void Awake(){
        infoBar = GameObject.FindGameObjectWithTag("infoBar").GetComponent<InfoBarScript>();
        inventorySlotIndex = transform.GetSiblingIndex();
    }

    // Deletes item in slot.
    public void ClearSlot(){
        if (slotTransform.childCount > 0){
            Transform itemTransform = slotTransform.GetChild(0);
            Destroy(itemTransform.gameObject);
            infoBar.UndisplayInfo();
        }
    }

    // Creates new item in slot with initial stack size of stackSize.
    public InventoryItem DrawSlot(GameObject inventoryItemPrefab, int stackSize){
        GameObject newInventoryItemObject = Instantiate(inventoryItemPrefab, slotTransform);
        InventoryItem newInventoryItem = newInventoryItemObject.GetComponent<InventoryItem>();
        newInventoryItem.AddToStack(stackSize);
        return newInventoryItem;
    }

    // Drops an item into a current inventory slot upon dragging. If the current slot already
    //   has an item, then swap the two items.
    public void OnDrop(PointerEventData eventData){
        if (!TimeManager.IsGamePaused()){
            GameObject dropped = eventData.pointerDrag;
            InventoryItem droppedInventoryItem = dropped.GetComponent<InventoryItem>();
            InventoryItem droppedShopItem = droppedInventoryItem.rootInventorySlot.GetComponent<InventorySlot>().linkedSellSlotTransform.GetComponentInChildren<InventoryItem>();
            SellSlot droppedSellSlot = droppedShopItem.parentAfterDrag.GetComponent<SellSlot>();

            if (slotTransform.childCount > 0){
                InventoryItem oldInventoryItem = slotTransform.GetComponentInChildren<InventoryItem>();
                oldInventoryItem.transform.SetParent(droppedInventoryItem.parentAfterDrag);
                oldInventoryItem.parentAfterDrag = droppedInventoryItem.parentAfterDrag;
                InventoryItem oldShopItem = linkedSellSlotTransform.GetComponentInChildren<InventoryItem>();
                linkedSellSlotTransform.GetComponent<SellSlot>().linkedInventoryItem = droppedInventoryItem;
                linkedSellSlotTransform.GetComponent<SellSlot>().linkedShopItem = droppedShopItem;
                oldShopItem.transform.SetParent(droppedShopItem.parentAfterDrag);
                oldShopItem.parentAfterDrag = droppedShopItem.parentAfterDrag;
                droppedSellSlot.linkedInventoryItem = oldInventoryItem;
                droppedSellSlot.linkedShopItem = oldShopItem;
            } else {
                droppedSellSlot.linkedInventoryItem = null;
                droppedSellSlot.linkedShopItem = null;
            }
            droppedInventoryItem.parentAfterDrag = transform.GetChild(0).transform; // sets the parent of the dragged item to Slot transform
            droppedInventoryItem.transform.SetParent(transform.GetChild(0).transform);
            
            droppedShopItem.parentAfterDrag = linkedSellSlotTransform;
            droppedShopItem.transform.SetParent(linkedSellSlotTransform);
            SellSlot oldSellSlot = linkedSellSlotTransform.GetComponent<SellSlot>();
            oldSellSlot.linkedInventoryItem = droppedInventoryItem;
            oldSellSlot.linkedShopItem = droppedShopItem;

            InventoryManager inventoryManager = transform.GetComponentInParent<InventoryManager>();
            HotbarManagerScript hotbarManager = inventoryManager.hotbar.GetComponent<HotbarManagerScript>();
            hotbarManager.UpdateHotbar();
        }
    }

    // Displays info text upon pointer entering slot
    public void OnPointerEnter(PointerEventData eventData){
        if (slotTransform.childCount > 0 && !TimeManager.IsGamePaused()){
            infoBar.DisplayInfo(transform.GetComponentInChildren<InventoryItem>());
        }
    }

    // Undisplay info text upon pointer exiting slot
    public void OnPointerExit(PointerEventData eventData){
        if (!TimeManager.IsGamePaused()){
            infoBar.UndisplayInfo();
        }
    }
}

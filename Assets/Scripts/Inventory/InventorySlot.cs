using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject hoverPanel;
    private Transform slotTransform;

    void Awake(){
        slotTransform = transform.GetChild(0);
    }

    public void ClearSlot(){
        if (slotTransform.childCount > 0){
            Transform itemTransform = slotTransform.GetChild(0);
            Destroy(itemTransform.gameObject);
        }

        if (transform.childCount > 1){
            hoverPanel.SetActive(false);
        }
    }

    public void DrawSlot(GameObject inventoryItemPrefab){
        Instantiate(inventoryItemPrefab, slotTransform);
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
    }

    // Displays hover text upon pointer entering slot
    public void OnPointerEnter(PointerEventData eventData){
        if (slotTransform.childCount > 0){
            hoverPanel.SetActive(true);
            hoverPanel.GetComponentInChildren<TextMeshProUGUI>().text = slotTransform.GetComponentInChildren<InventoryItem>().hoverText;
        }
    }

    // Hides hover text upon pointer exiting slot
    public void OnPointerExit(PointerEventData eventData){
        hoverPanel.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image icon;
    public TextMeshProUGUI stackSizeText;
    public GameObject hoverPanel;
    public TextMeshProUGUI hoverText;
    
    void Awake(){
        icon.enabled = false;
        stackSizeText.enabled = false;
        hoverPanel.SetActive(false);
    }

    public void ClearSlot(){
        icon.enabled = false;
        stackSizeText.enabled = false;
        hoverPanel.SetActive(false);
    }

    public void DrawSlot(InventoryItem item){
        if (item == null){
            ClearSlot();
        }

        icon.sprite = item.itemData.icon;
        stackSizeText.text = item.stackSize.ToString();
        hoverText.text = item.itemData.hoverText.ToString();

        icon.enabled = true;
        stackSizeText.enabled = true;
    }

    public void DisplayHoverText(){
        if (icon.enabled){
            hoverPanel.SetActive(true);
        }
    }

    public void UndisplayHoverText(){
        hoverPanel.SetActive(false);
    }

    // Drops an item into a new inventory slot upon dragging
    public void OnDrop(PointerEventData eventData){
        GameObject dropped = eventData.pointerDrag;
        DraggableItem draggableItem = dropped.GetComponent<DraggableItem>();
        draggableItem.parentAfterDrag = transform.GetChild(0).transform; // sets the parent of the dragged item to Slot transform
    }
}

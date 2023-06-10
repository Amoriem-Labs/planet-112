using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

[Serializable]
public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public string displayName;
    public Image image;
    public bool stackable;
    public int stackSize;
    public TextMeshProUGUI stackSizeText;
    public GameObject hoverPanel;
    public string hoverText;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public Transform rootInventorySlot;
    private bool draggingItem;

    void Awake(){
        rootInventorySlot = transform.parent.parent;
        draggingItem = rootInventorySlot.GetComponentInParent<InventoryManager>().draggingItem;
        hoverPanel.SetActive(false);
        if (stackable){
            stackSize++;
            stackSizeText.text = stackSize.ToString();
        }
        else {
            stackSize = 1;
            stackSizeText.text = "";
        }
    }

    public void AddToStack(){
        if (stackable){
            stackSize++;
            stackSizeText.text = stackSize.ToString();
        }
    }

    public void RemoveFromStack(){
        if (stackable){
            stackSize--;
            stackSizeText.text = stackSize.ToString();
        }
        else {
            stackSize = 0;
            stackSizeText.text = "";
        }
    }

    public void OnBeginDrag(PointerEventData eventData){
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        draggingItem = true;
    }

    public void OnDrag(PointerEventData eventData){
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        transform.position = worldPosition;
    }

    public void OnEndDrag(PointerEventData eventData){
        transform.SetParent(parentAfterDrag);
        image.raycastTarget = true;
        draggingItem = false;
    }

    public void OnPointerEnter(PointerEventData eventData){
        if (draggingItem){
            image.raycastTarget = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData){
        if (draggingItem){
            image.raycastTarget = true;
        }
    }
}

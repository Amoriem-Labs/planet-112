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
    public Sprite sprite;
    public bool stackable;
    public int stackSize;
    public TextMeshProUGUI stackSizeText;
    public string infoText;
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public Transform rootInventorySlot;
    private bool draggingItem;
    private bool thisBeingDragged;
    public bool isHotbarItem;
    public GameObject linkedItemPrefab;
    public HotbarManagerScript hotbar;
    public InfoBarScript infoBar;

    // Initializing inventory item properties.
    void Awake(){
        rootInventorySlot = transform.parent.parent;
        draggingItem = rootInventorySlot.GetComponentInParent<InventoryManager>().draggingItem;
        hotbar = GameObject.FindGameObjectWithTag("hotbar").GetComponent<HotbarManagerScript>();
        infoBar = GameObject.FindGameObjectWithTag("infoBar").GetComponent<InfoBarScript>();
    }

    #region Adding and Removing from Stack.
    public void AddToStack(){
        if (stackable){
            stackSize++;
            stackSizeText.text = stackSize.ToString();
        } else {
            stackSize = 1;
            stackSizeText.text = "";
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
    #endregion

    #region Dragging and Dropping Items
    public void OnBeginDrag(PointerEventData eventData){
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        image.raycastTarget = false;
        draggingItem = true;
        thisBeingDragged = true;
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
        thisBeingDragged = false;
        rootInventorySlot = transform.parent.parent;
    }

    public void OnPointerEnter(PointerEventData eventData){
        if (draggingItem && !thisBeingDragged){
            image.raycastTarget = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData){
        if (draggingItem && !thisBeingDragged){
            image.raycastTarget = true;
        }
    }
    #endregion

    // Deletes the inventory item. Is triggered when inventory item is dragged to trash icon.
    public void Delete(){
        infoBar.UndisplayInfo();
        hotbar.UpdateHotbar();
        if (linkedItemPrefab.TryGetComponent<Fruit>(out Fruit fruitScript)){
            if (fruitScript.fruitType.Equals("seafoam")){
                hotbar.fruitManager.nSeafoam -= stackSize;
            }
            if (fruitScript.fruitType.Equals("sunset")){
                hotbar.fruitManager.nSunset -= stackSize;
            }
            if (fruitScript.fruitType.Equals("amethyst")){
                hotbar.fruitManager.nAmethyst -= stackSize;
            }
            if (fruitScript.fruitType.Equals("crystalline")){
                hotbar.fruitManager.nCrystalline -= stackSize;
            }
        }
        hotbar.UpdateFruitText();
        Destroy(gameObject);
    }

    // Use inventory item. Is triggered when inventory item is in a hotbar slot and player presses a hotbar key.
    public void Use(){
        ICollectible item = linkedItemPrefab.GetComponent<ICollectible>();
        item.Use();
    }
}

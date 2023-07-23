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
    public ShopItem linkedShopItemSO; // Links the inventory item to the SellSlot in shop UI via a scriptable object.
    public string displayName;
    public Image image;
    public Sprite sprite;
    public bool stackable;
    public int stackSize;
    public TextMeshProUGUI stackSizeText;
    public string infoText;
    public Transform parentAfterDrag;
    [HideInInspector] public Transform rootInventorySlot;
    private bool draggingItem;
    private bool thisBeingDragged;
    public bool isHotbarItem;
    public GameObject linkedItemPrefab;
    public HotbarManagerScript hotbar;
    public InfoBarScript infoBar;
    public PlayerScript playerScript;

    // Initializing inventory item properties.
    void Awake(){
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        if (transform.parent.parent.TryGetComponent<InventorySlot>(out InventorySlot inventorySlot)){
            rootInventorySlot = transform.parent.parent;
            draggingItem = rootInventorySlot.GetComponentInParent<InventoryManager>().draggingItem;
            infoBar = GameObject.FindGameObjectWithTag("infoBar").GetComponent<InfoBarScript>();
        }
        hotbar = GameObject.FindGameObjectWithTag("hotbar").GetComponent<HotbarManagerScript>();
        parentAfterDrag = transform.parent;
    }

    #region Adding and Removing from Stack.
    public void AddToStack(int numAdd){
        if (stackable){
            stackSize += numAdd;
            stackSizeText.text = stackSize.ToString();
        } else {
            stackSize = 1;
            stackSizeText.text = "";
        }
    }

    public void RemoveFromStack(int numRemove){
        if (stackable){
            stackSize -= numRemove;
            stackSizeText.text = stackSize.ToString();
        } else {
            stackSize = 0;
            stackSizeText.text = "";
        }
    }
    #endregion

    #region Dragging and Dropping Items
    public void OnBeginDrag(PointerEventData eventData){
        if (!TimeManager.IsGamePaused() && playerScript.inventoryIsLoaded){
            parentAfterDrag = transform.parent;
            transform.SetParent(transform.root);
            transform.SetAsLastSibling();
            image.raycastTarget = false;
            draggingItem = true;
            thisBeingDragged = true;
        }
    }

    public void OnDrag(PointerEventData eventData){
        if (!TimeManager.IsGamePaused() && playerScript.inventoryIsLoaded){
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            transform.position = worldPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData){
        if (!TimeManager.IsGamePaused() && playerScript.inventoryIsLoaded){
            transform.SetParent(parentAfterDrag);
            image.raycastTarget = true;
            draggingItem = false;
            thisBeingDragged = false;
            rootInventorySlot = transform.parent.parent;
            
        }
    }

    public void OnPointerEnter(PointerEventData eventData){
        if (draggingItem && !thisBeingDragged && !TimeManager.IsGamePaused() && playerScript.inventoryIsLoaded){
            image.raycastTarget = false;
        }
    }

    public void OnPointerExit(PointerEventData eventData){
        if (draggingItem && !thisBeingDragged && !TimeManager.IsGamePaused() && playerScript.inventoryIsLoaded){
            image.raycastTarget = true;
        }
    }
    #endregion

    // Deletes the inventory item and corresponding shop sell UI item. Is triggered when inventory item is dragged to trash icon.
    public void DeleteFromTrashing(){
        infoBar.UndisplayInfo();
        if (linkedItemPrefab.TryGetComponent<Fruit>(out Fruit fruitScript)){
            if (fruitScript.fruitType.Equals("Seafoam")){
                hotbar.fruitManager.nSeafoam -= stackSize;
            }
            if (fruitScript.fruitType.Equals("Sunset")){
                hotbar.fruitManager.nSunset -= stackSize;
            }
            if (fruitScript.fruitType.Equals("Amethyst")){
                hotbar.fruitManager.nAmethyst -= stackSize;
            }
            if (fruitScript.fruitType.Equals("Crystalline")){
                hotbar.fruitManager.nCrystalline -= stackSize;
            }
        }
        hotbar.UpdateFruitText();
        Destroy(gameObject); // Destroys the item in the inventory...
        InventoryManager inventoryManager = rootInventorySlot.GetComponentInParent<InventoryManager>();
        int slotIndex = rootInventorySlot.GetComponent<InventorySlot>().inventorySlotIndex;
        GameObject sellShopUIgameObject = inventoryManager.linkedSellSlots[slotIndex].transform.GetChild(0).gameObject;
        Destroy(sellShopUIgameObject); // ... and also destroys the item in the shop sell UI's main panel
    }

    // Deletes the shop sell UI item and corresponding inventory item. Is triggered when an item is sold in the shop.
    public void DeleteFromSelling(GameObject inventoryItemObject){
        Destroy(gameObject); // Destroys the shop sell UI item...
        Destroy(inventoryItemObject); // ... and also destroys the item in the shop sell UI's main panel
    }

    // Deletes numSell amount of this inventory item. Is triggered when selling items to Mav.
    public void Sell(int numSell, GameObject inventoryItemObject){
        if (numSell == stackSize){
            DeleteFromSelling(inventoryItemObject);
        } else {
            stackSize -= numSell;
            stackSizeText.text = stackSize.ToString();
        }
        hotbar.UpdateHotbar();
        hotbar.UpdateFruitText();
    }

    // Use inventory item. Is triggered when inventory item is in a hotbar slot and player presses a hotbar key.
    public void Use(){
        ICollectible item = linkedItemPrefab.GetComponent<ICollectible>();
        item.Use();
    }
}

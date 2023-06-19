using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public List<InventorySlot> inventorySlots = new List<InventorySlot>(27);
    public bool draggingItem;
    public GameObject hotbar;
    public FruitManager fruitManager;

    // Initializing inventory.
    void Awake(){
        ResetInventory();
        fruitManager = GameObject.FindGameObjectWithTag("fruitManager").GetComponent<FruitManager>();
        draggingItem = false;
        Fruit.OnFruitCollected += UpdateInventory;
        Weapon.OnWeaponCollected += UpdateInventory;
        transform.parent.gameObject.SetActive(false);
    }

    #region Initializes inventory
    // Deletes all items in inventory and instantiates the slot prefabs.
    void ResetInventory(){
        foreach (Transform inventorySlotTransform in transform){
            Destroy(inventorySlotTransform.gameObject);
        }
        inventorySlots = new List<InventorySlot>(27);

        for (int i = 0; i < inventorySlots.Capacity; i++){
            CreateInventorySlot(i);
        }
    }

    void CreateInventorySlot(int inventorySlotIndex){
        GameObject newSlot = Instantiate(slotPrefab);
        newSlot.transform.SetParent(transform, false);

        InventorySlot newSlotComponent = newSlot.GetComponent<InventorySlot>();
        newSlotComponent.inventorySlotIndex = inventorySlotIndex;
        newSlotComponent.ClearSlot();
        inventorySlots.Add(newSlotComponent);
    }
    #endregion

    // Searches if item already exists in inventory, and if so, add to that item's stackSize.
    //   If item doesn't exist, then add item to first empty InventorySlot.
    //   This method is triggered whenever player picks up a new item.
    void UpdateInventory(GameObject inventoryItemPrefab){
        hotbar.GetComponent<HotbarManagerScript>().LinkSlotTransforms();
        HotbarManagerScript hotbarManager = hotbar.GetComponent<HotbarManagerScript>();

        // Searches if item already exists in inventory, and add to that item's stacksize if so.
        for (int i = 0; i < inventorySlots.Count; i++){
            InventorySlot inventorySlot = inventorySlots[i];
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            if (slotTransform.childCount > 0){
                InventoryItem inventoryItem = inventorySlot.transform.GetComponentInChildren<InventoryItem>();
                if (inventoryItem.displayName == inventoryItemPrefab.GetComponent<InventoryItem>().displayName){
                    inventoryItem.AddToStack();
                    hotbarManager.UpdateHotbar();
                    if (inventoryItem.linkedItemPrefab.TryGetComponent<Fruit>(out Fruit fruitScript)){
                        fruitManager.AddToFruitStack(fruitScript.fruitType);
                        hotbarManager.UpdateFruitText();
                    }
                    return;
                }
            }
        }

        // If item doesn't exist in inventory, then add that item to the first empty InventorySlot.
        for (int i = 0; i < inventorySlots.Count; i++){
            InventorySlot inventorySlot = inventorySlots[i];
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            if (slotTransform.childCount == 0){
                inventorySlot.DrawSlot(inventoryItemPrefab);
                hotbarManager.UpdateHotbar();
                if (inventoryItemPrefab.GetComponent<InventoryItem>().linkedItemPrefab.TryGetComponent<Fruit>(out Fruit fruitScript)){
                        fruitManager.AddToFruitStack(fruitScript.fruitType);
                        hotbarManager.UpdateFruitText();
                }
                return;
            }
        }
    }
}

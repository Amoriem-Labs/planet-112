using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public List<InventorySlot> inventorySlots = new List<InventorySlot>(27);
    public bool draggingItem;

    void Awake(){
        ResetInventory();
        draggingItem = false;
    }

    private void OnEnable(){
        Fruit.OnFruitCollected += UpdateInventory;
        Weapon.OnWeaponCollected += UpdateInventory;
    }

    private void OnDisable(){
        Fruit.OnFruitCollected -= UpdateInventory;
        Weapon.OnWeaponCollected -= UpdateInventory;
    }

    void ResetInventory(){
        foreach (Transform inventorySlotTransform in transform){
            Destroy(inventorySlotTransform.gameObject);
        }
        inventorySlots = new List<InventorySlot>(27);

        for (int i = 0; i < inventorySlots.Capacity; i++){
            CreateInventorySlot();
        }
    }

    void CreateInventorySlot(){
        GameObject newSlot = Instantiate(slotPrefab);
        newSlot.transform.SetParent(transform, false);

        InventorySlot newSlotComponent = newSlot.GetComponent<InventorySlot>();
        newSlotComponent.ClearSlot();
        inventorySlots.Add(newSlotComponent);
    }

    // Searches if item already exists in inventory, and if so, add to that item's stackSize.
    //   If item doesn't exist, then add item to first empty InventorySlot.
    void UpdateInventory(GameObject inventoryItemPrefab){
        foreach (InventorySlot inventorySlot in inventorySlots){
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            if (slotTransform.childCount > 0){
                InventoryItem inventoryItem = inventorySlot.transform.GetComponentInChildren<InventoryItem>();
                if (inventoryItem.displayName == inventoryItemPrefab.GetComponent<InventoryItem>().displayName){
                    inventoryItem.AddToStack();
                    return;
                }
            }
        }
        foreach (InventorySlot inventorySlot in inventorySlots){
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            if (slotTransform.childCount == 0){
                inventorySlot.DrawSlot(inventoryItemPrefab);
                return;
            }
        }
    }
}

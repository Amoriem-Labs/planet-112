using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public int numInventorySlots = 27;
    public List<InventorySlot> inventorySlots;
    public bool draggingItem;
    public GameObject hotbar;
    public FruitManager fruitManager;
    public List<GameObject> possibleItemPrefabs; // Reference the possible prefabs that can be items in the inspector

    // Initializing inventory.
    void Awake(){
        inventorySlots = new List<InventorySlot>(numInventorySlots);
        for (int i = 0; i < inventorySlots.Capacity; i++){
            inventorySlots.Add(transform.GetChild(i).GetComponent<InventorySlot>());
        }
        draggingItem = false;
        Fruit.OnFruitCollected += UpdateInventory;
        Weapon.OnWeaponCollected += UpdateInventory;
    }

    // Deletes all items in inventory. Currently un-used.
    void ResetInventory(){
        foreach (Transform inventorySlotTransform in transform){
            if (inventorySlotTransform.GetChild(0).childCount > 0){
                Destroy(inventorySlotTransform.GetChild(0).GetChild(0));
            }
        }
    }

    // Searches if item already exists in inventory, and if so, add to that item's stackSize.
    //   If item doesn't exist, then add item to first empty InventorySlot.
    //   This method is triggered whenever player picks up a new item.
    void UpdateInventory(GameObject inventoryItemPrefab){
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

    // This method is called whenever loading in a save file. Populates inventory with items from the save file.
    public void LoadInventory(PlayerData playerData){
        HotbarManagerScript hotbarManager = hotbar.GetComponent<HotbarManagerScript>();

        // Populates inventory item by item from inventoryItemDatas
        for (int i = 0; i < playerData.inventoryItemDatas.Count; i++){
            InventorySlot inventorySlot = inventorySlots[i];
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            InventoryItemData inventoryItemData = playerData.inventoryItemDatas[i];
            if (!inventoryItemData.itemName.Equals("empty")){
                // if there is an actual inventory item at that index, then search for what prefab could match that item
                foreach (GameObject itemPrefab in possibleItemPrefabs){
                    if (inventoryItemData.itemName.Equals(itemPrefab.GetComponent<InventoryItem>().displayName)){
                        // Once we find the prefab that matches the inventory item, instantiate the prefab in the correct slot
                        inventorySlot.DrawSlot(itemPrefab);
                        InventoryItem inventoryItem = inventorySlot.slotTransform.GetComponentInChildren<InventoryItem>();
                        inventoryItem.stackSize = inventoryItemData.count;
                        inventoryItem.stackSizeText.text = inventoryItem.stackSize.ToString();
                    }
                }
            }
        }
        fruitManager.nSeafoam = playerData.nSeafoam;
        fruitManager.nSunset = playerData.nSunset;
        fruitManager.nAmethyst = playerData.nAmethyst;
        fruitManager.nCrystalline = playerData.nCrystalline;
        hotbarManager.UpdateHotbar();
        hotbarManager.UpdateFruitText();
    }
}

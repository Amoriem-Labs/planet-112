using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public int numInventorySlots = 27;
    public List<InventorySlot> inventorySlots;
    public Transform sellUIMainPanelTransform;
    public List<SellSlot> linkedSellSlots; // The SellSlots in the shop UI that are linked to the inventory.
    public bool draggingItem;
    public GameObject hotbar;
    public FruitManager fruitManager;
    public ShopManager shopManager;
    public List<GameObject> possibleItemPrefabs; // Reference the possible prefabs that can be items in the inspector
    public List<StartingItemSO> startingItems; // a list of scriptable objects containing the starting items that a player will have when creating a new save file

    // Initializing inventory and shop sell UI's main panel.
    void Awake(){
        inventorySlots = new List<InventorySlot>(numInventorySlots);
        linkedSellSlots = new List<SellSlot>(numInventorySlots);
        for (int i = 0; i < inventorySlots.Capacity; i++){
            inventorySlots.Add(transform.GetChild(i).GetComponent<InventorySlot>());
            linkedSellSlots.Add(sellUIMainPanelTransform.GetChild(i).GetComponent<SellSlot>());
            inventorySlots[i].linkedSellSlotTransform = sellUIMainPanelTransform.GetChild(i);
        }
        draggingItem = false;
        Fruit.OnFruitCollected += UpdateInventory;
        Weapon.OnWeaponCollected += UpdateInventory;
    }

    // Deletes all items in inventory and shop sell UI's main panel. Is triggered by pressing R.
    public void ResetInventory(){
        print("Resetting Inventory");
        for (int i = 0; i < numInventorySlots; i++){
            Transform inventorySlotTransform = transform.GetChild(i);
            if (inventorySlotTransform.GetChild(0).childCount > 0){
                Destroy(inventorySlotTransform.GetChild(0).GetChild(0).gameObject);
                Destroy(linkedSellSlots[i].transform.GetChild(0).gameObject);
            }
        }
        HotbarManagerScript hotbarManager = hotbar.GetComponent<HotbarManagerScript>();
        hotbarManager.DeleteHotbar();
        fruitManager.Reset();
        hotbarManager.UpdateFruitText();
        shopManager.UpdateFruitStockText();
    }

    // Searches if item already exists in inventory, and if so, add to that item's stackSize.
    //   If item doesn't exist, then add item to first empty InventorySlot and shop SellSlot.
    //   This method is triggered whenever player picks up a new item.
    public void UpdateInventory(GameObject inventoryItemPrefab){
        HotbarManagerScript hotbarManager = hotbar.GetComponent<HotbarManagerScript>();

        // Searches if item already exists in inventory, and add to that item's stacksize if so.
        for (int i = 0; i < inventorySlots.Count; i++){
            InventorySlot inventorySlot = inventorySlots[i];
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            if (slotTransform.childCount > 0 && slotTransform.GetComponentInChildren<InventoryItem>().stackSize < 99){
                InventoryItem inventoryItem = slotTransform.GetComponentInChildren<InventoryItem>();
                if (inventoryItem.displayName.Equals(inventoryItemPrefab.GetComponent<InventoryItem>().displayName)){
                    inventoryItem.AddToStack(1);
                    linkedSellSlots[i].GetComponentInChildren<InventoryItem>().AddToStack(1);
                    hotbarManager.UpdateHotbar();
                    if (inventoryItem.linkedItemPrefab.TryGetComponent<Fruit>(out Fruit fruitScript)){
                        fruitManager.AddToFruitStack(fruitScript.fruitType);
                        hotbarManager.UpdateFruitText();
                        shopManager.UpdateFruitStockText();
                    }
                    return;
                }
            }
        }

        // If item doesn't exist in inventory or inventory stackSize is at 99, then add that item to the first empty InventorySlot and shop SellSlot.
        for (int i = 0; i < inventorySlots.Count; i++){
            InventorySlot inventorySlot = inventorySlots[i];
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            if (slotTransform.childCount == 0){
                InventoryItem newInventoryItem = inventorySlot.DrawSlot(inventoryItemPrefab, 1);
                newInventoryItem.LinkInventoryItem();
                linkedSellSlots[i].DrawSlot(inventoryItemPrefab, 1, newInventoryItem);
                linkedSellSlots[i].linkedShopItem.parentAfterDrag = linkedSellSlots[i].transform;
                linkedSellSlots[i].linkedInventoryItem.parentAfterDrag = inventorySlots[i].transform.GetChild(0).transform;
                hotbarManager.UpdateHotbar();
                if (inventoryItemPrefab.GetComponent<InventoryItem>().linkedItemPrefab.TryGetComponent<Fruit>(out Fruit fruitScript)){
                    fruitManager.AddToFruitStack(fruitScript.fruitType);
                    hotbarManager.UpdateFruitText();
                    shopManager.UpdateFruitStockText();
                }
                return;
            }
        }
    }

    // This method is called whenever loading in a save file. Populates inventory and shop sell UI's main panel with items from the save file.
    public void LoadInventory(PlayerData playerData){
        HotbarManagerScript hotbarManager = hotbar.GetComponent<HotbarManagerScript>();

        // Populates inventory and shop sell UI item by item from inventoryItemDatas
        for (int i = 0; i < playerData.inventoryItemDatas.Count; i++){
            InventorySlot inventorySlot = inventorySlots[i];
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            InventoryItemData inventoryItemData = playerData.inventoryItemDatas[i];
            if (!inventoryItemData.itemName.Equals("empty")){
                // if there is an actual inventory item at that index, then search for what prefab could match that item
                foreach (GameObject itemPrefab in possibleItemPrefabs){
                    if (inventoryItemData.itemName.Equals(itemPrefab.GetComponent<InventoryItem>().displayName)){
                        // Once we find the prefab that matches the inventory item, instantiate the prefab in the correct slots
                        InventoryItem newInventoryItem = inventorySlot.DrawSlot(itemPrefab, inventoryItemData.count);
                        newInventoryItem.LinkInventoryItem();
                        linkedSellSlots[i].DrawSlot(itemPrefab, inventoryItemData.count, newInventoryItem);
                        linkedSellSlots[i].linkedShopItem.parentAfterDrag = linkedSellSlots[i].transform;
                        linkedSellSlots[i].linkedInventoryItem.parentAfterDrag = inventorySlots[i].transform.GetChild(0).transform;
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
        shopManager.UpdateFruitStockText();
    }

    public void LoadNewInventory(){
        HotbarManagerScript hotbarManager = hotbar.GetComponent<HotbarManagerScript>();

        foreach (StartingItemSO startingItem in startingItems){
            InventorySlot inventorySlot = inventorySlots[startingItem.inventorySlotIndex];
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            InventoryItem newInventoryItem = inventorySlot.DrawSlot(startingItem.itemPrefab, startingItem.count);
            newInventoryItem.LinkInventoryItem();
            linkedSellSlots[startingItem.inventorySlotIndex].DrawSlot(startingItem.itemPrefab, startingItem.count, newInventoryItem);
            linkedSellSlots[startingItem.inventorySlotIndex].linkedShopItem.parentAfterDrag = linkedSellSlots[startingItem.inventorySlotIndex].transform;
            linkedSellSlots[startingItem.inventorySlotIndex].linkedInventoryItem.parentAfterDrag = inventorySlots[startingItem.inventorySlotIndex].transform.GetChild(0).transform;

            if (startingItem.itemName.Equals("SeafoamIcura")) fruitManager.nSeafoam = startingItem.count;
            if (startingItem.itemName.Equals("SunsetIcura")) fruitManager.nSunset = startingItem.count;
            if (startingItem.itemName.Equals("AmethystIcura")) fruitManager.nAmethyst = startingItem.count;
            if (startingItem.itemName.Equals("CrystallineIcura")) fruitManager.nCrystalline = startingItem.count;
        }
        hotbarManager.UpdateHotbar();
        hotbarManager.UpdateFruitText();
        shopManager.UpdateFruitStockText();
    }

    // Updates inventory after buying an item
    public void BuyUpdateInventory(GameObject inventoryItemPrefab, int numBought, Dictionary<string,int> totalCostDict){
        HotbarManagerScript hotbarManager = hotbar.GetComponent<HotbarManagerScript>();

        bool justDestroyed = false;

        // Deletes the amount of icura you spent from the inventory and update corresponding slot in shop sell UI.
        for (int keyIndex = 0; keyIndex < totalCostDict.Count; keyIndex++){
            string key = totalCostDict.ElementAt(keyIndex).Key.ToString();
            for (int i = 0; i < inventorySlots.Count; i++){
                if (totalCostDict[key] > 0){
                    InventorySlot inventorySlot = inventorySlots[i];
                    Transform slotTransform = inventorySlot.transform.GetChild(0); 
                    InventoryItem inventoryItem = slotTransform.GetComponentInChildren<InventoryItem>();
                    InventoryItem shopItem = linkedSellSlots[i].transform.GetComponentInChildren<InventoryItem>();
                    if (slotTransform.childCount > 0 && inventoryItem.linkedItemPrefab.TryGetComponent<Fruit>(out Fruit fruitScript)){
                        // If there is more cost than or equal to the icura amount currently in this inventory slot, use up all of the icura in this inventory slot and wait for next iteration of loop to spend remaining icura needed to make the purchase.
                        if (fruitScript.fruitType.Equals(key)){
                            Debug.Log($"total cost: {totalCostDict[key]}, num icura: {inventoryItem.stackSize}");
                            if (totalCostDict[key] >= inventoryItem.stackSize){
                                totalCostDict[key] = totalCostDict[key] - inventoryItem.stackSize;
                                inventoryItem.RemoveFromStack(inventoryItem.stackSize);
                                shopItem.RemoveFromStack(inventoryItem.stackSize);
                                Destroy(inventoryItem.gameObject);
                                Destroy(shopItem.gameObject);
                                justDestroyed = true; // need to add this boolean logic b/c Unity doesn't actually destroy an object until the end of the function call.
                            } else { // otherwise, just deduct however much icura you owe from this inventory slot and have some icura in inventory leftover.
                                inventoryItem.RemoveFromStack(totalCostDict[key]);
                                shopItem.RemoveFromStack(totalCostDict[key]);
                                totalCostDict[key] = totalCostDict[key] - inventoryItem.stackSize;
                            }
                        }
                    }
                }
            }
            hotbarManager.Invoke("UpdateHotbar",0);
            hotbarManager.Invoke("UpdateFruitText",0);
        }

        shopManager.UpdateFruitStockText();

        // Searches if item already exists in inventory, and add to that item's stacksize if so.
        for (int i = 0; i < inventorySlots.Count; i++){
            InventorySlot inventorySlot = inventorySlots[i];
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            if (slotTransform.childCount > 0 && slotTransform.GetComponentInChildren<InventoryItem>().stackSize < 99){
                InventoryItem inventoryItem = inventorySlot.transform.GetComponentInChildren<InventoryItem>();
                InventoryItem shopItem = linkedSellSlots[i].transform.GetComponentInChildren<InventoryItem>();
                if (inventoryItem.displayName == inventoryItemPrefab.GetComponent<InventoryItem>().displayName){
                    inventoryItem.AddToStack(numBought);
                    shopItem.AddToStack(numBought);
                    hotbarManager.UpdateHotbar();
                    hotbarManager.UpdateFruitText();
                    return;
                }
            }
        }

        // If item doesn't exist in inventory or inventory stackSize is at 99, then add that item to the first empty InventorySlot.
        for (int i = 0; i < inventorySlots.Count; i++){
            InventorySlot inventorySlot = inventorySlots[i];
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            if (slotTransform.childCount == 0 || justDestroyed){
                InventoryItem newInventoryItem = inventorySlot.DrawSlot(inventoryItemPrefab, numBought);
                newInventoryItem.LinkInventoryItem();
                linkedSellSlots[i].DrawSlot(inventoryItemPrefab, numBought, newInventoryItem);
                linkedSellSlots[i].linkedShopItem.parentAfterDrag = linkedSellSlots[i].transform;
                linkedSellSlots[i].linkedInventoryItem.parentAfterDrag = inventorySlots[i].transform.GetChild(0).transform;
                justDestroyed = false;
                hotbarManager.UpdateHotbar();
                hotbarManager.UpdateFruitText();
                return;
            }
        }
    }
}

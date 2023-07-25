using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellSlot : MonoBehaviour
{
    public int sellStackSize; // The The amount of this item that the player is buying.
    public ShopManager shopManager; // ShopManager script that links all the systems together.
    public InventoryItem linkedShopItem; // InventoryItem from the shop UI that is linked to this SellSlot.
    public InventoryItem linkedInventoryItem; // InventoryItem from the inventory UI that is linked to this SellSlot.
    [HideInInspector]public bool isAdding;
    [HideInInspector]public bool isRemoving;
    [HideInInspector]private bool isFiring;
    [HideInInspector]private bool stopFiring;
    [HideInInspector]public float timeElapsedSinceButtonDown;
    [HideInInspector]public ColorBlock selectedColorBlock;
    [HideInInspector]public ColorBlock unselectedColorBlock;
    [HideInInspector]public Button button;

    void Awake(){
        shopManager = GameObject.FindGameObjectWithTag("shopManager").GetComponent<ShopManager>();
        sellStackSize = 0;
        shopManager.sellStackText.text = sellStackSize.ToString();
        timeElapsedSinceButtonDown = 0.0f;
        button = GetComponent<Button>(); 
        selectedColorBlock = button.colors;
        unselectedColorBlock = button.colors;
        selectedColorBlock.normalColor = new Color(0.6f,0.6f,0.6f);
        unselectedColorBlock.normalColor = new Color(1.0f,1.0f,1.0f);
    }

    void Update(){
        if (isAdding || isRemoving){
            timeElapsedSinceButtonDown += Time.deltaTime;
        }
        if (isFiring && isAdding){
            if (sellStackSize < 99 && sellStackSize < linkedShopItem.stackSize){
                sellStackSize++;
                shopManager.sellStackText.text = sellStackSize.ToString();
                shopManager.totalSeafoamCost += linkedShopItem.linkedShopItemSO.cost[0];
                shopManager.totalSunsetCost += linkedShopItem.linkedShopItemSO.cost[1];
                shopManager.totalAmethystCost += linkedShopItem.linkedShopItemSO.cost[2];
                shopManager.totalCrystallineCost += linkedShopItem.linkedShopItemSO.cost[3];
                shopManager.updateCostText(linkedShopItem.linkedShopItemSO.cost, sellStackSize, "sell");
            }
        }
        if (isFiring && isRemoving){
            if (sellStackSize > 0){
                sellStackSize--;
                shopManager.sellStackText.text = sellStackSize.ToString();
                shopManager.totalSeafoamCost -= linkedShopItem.linkedShopItemSO.cost[0];
                shopManager.totalSunsetCost -= linkedShopItem.linkedShopItemSO.cost[1];
                shopManager.totalAmethystCost -= linkedShopItem.linkedShopItemSO.cost[2];
                shopManager.totalCrystallineCost -= linkedShopItem.linkedShopItemSO.cost[3];
                shopManager.updateCostText(linkedShopItem.linkedShopItemSO.cost, sellStackSize, "sell");
            }
        }
    }

    public void DrawSlot(GameObject inventoryItemPrefab, int stackSize, InventoryItem newInventoryItem){
        GameObject sellItemObject = Instantiate(inventoryItemPrefab, transform);
        linkedShopItem = sellItemObject.GetComponent<InventoryItem>();
        linkedShopItem.AddToStack(stackSize);
        int shopIndex = transform.GetSiblingIndex();
        linkedInventoryItem = newInventoryItem;
    }

    public void pointerDown(){
        if (!linkedShopItem.linkedItemPrefab.TryGetComponent<Fruit>(out Fruit fruitScript)){ // This if statement ensures that you cannot sell fruit in the shop.
            stopFiring = false;
            makeFireVariableTrue();
        }
    }

    private void makeFireVariableTrue(){
        isFiring = true;
        Invoke("makeFireVariableFalse", timeElapsedSinceButtonDown/100);
    }

    public void pointerUp(){
        if (!linkedShopItem.linkedItemPrefab.TryGetComponent<Fruit>(out Fruit fruitScript)){ // This if statement ensures that you cannot sell fruit in the shop.
            isFiring = false;
            stopFiring = true;
            timeElapsedSinceButtonDown = 0.0f;
        }
    }

    private void makeFireVariableFalse(){
        isFiring = false;
        if (!stopFiring){
            if (1-timeElapsedSinceButtonDown/5 > 0){
                Invoke("makeFireVariableTrue",1-timeElapsedSinceButtonDown/5);
            } else {
                makeFireVariableTrue();
            }
            
        }
    }

    // Selects item when this item is clicked in inventory
    public void Select(){
        if (!TimeManager.IsGamePaused() && transform.childCount > 0){
            // Change color of button when selected and changes the previously selected slot's color be back to the assigned unselected color.
            button.colors = selectedColorBlock;
            shopManager.currentlySelectedSellSlot.GetComponent<Button>().colors = unselectedColorBlock;

            // Makes the shop selection arrow and selections panel visible.
            shopManager.sellUIselectionArrow.SetActive(true);
            Vector2 new_selection_arrow_pos = shopManager.sellUIselectionArrow.transform.position;
            new_selection_arrow_pos.y = transform.position.y;
            shopManager.sellUIselectionArrow.transform.position = new_selection_arrow_pos;
            if (linkedShopItem.linkedItemPrefab.TryGetComponent<Fruit>(out Fruit fruitScript)){ // This if statement ensures that you cannot sell fruit in the shop.
                shopManager.sellUIcostText.text = "Cannot Sell Icura!";
                shopManager.sellUInumSelectionsPanel.SetActive(false);
                shopManager.infoText.text = "Cannot Sell Icura!";
            } else {
                shopManager.sellUInumSelectionsPanel.SetActive(true);
                Vector2 new_selections_panel_pos = shopManager.sellUInumSelectionsPanel.transform.position;
                new_selections_panel_pos.y = transform.position.y;
                shopManager.sellUInumSelectionsPanel.transform.position = new_selections_panel_pos;
                // Displays info in the info panel.
                if (linkedShopItem.linkedShopItemSO is ShopPlantSeed) { 
                    shopManager.DisplayInfo((ShopPlantSeed)linkedShopItem.linkedShopItemSO); // temporary implementation, since downcasting is bad
                } else if (linkedShopItem.linkedShopItemSO is ShopWeapon) { 
                    shopManager.DisplayInfo((ShopWeapon)linkedShopItem.linkedShopItemSO); // temporary implementation, since downcasting is bad
                }
            }

            shopManager.ownedStockText.text = linkedShopItem.stackSize.ToString(); // Updates part in money panel that states how much of this item does player already have in inventory
            shopManager.equippedText.text = "--"; // Updates part in money panel that states whether player already has this item equipped (useful for eventual clothing system)

            // Resets cost panel and internal count of total number of seafoam, sunset, amethyst, and crystalline
            shopManager.Reset();

            // Updates the currently selected slot variable in ShopManager
            shopManager.currentlySelectedSellSlot = this;
        }
    }
}

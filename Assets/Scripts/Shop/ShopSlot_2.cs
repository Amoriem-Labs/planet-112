using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot_2 : MonoBehaviour
{
    public ShopItem shopItemSO; // The scriptable object that contains fixed (non-dynamic) data about this shop item.
    public int buyStackSize; // The The amount of this item that the player is buying.
    public TextMeshProUGUI buyStackText; // The TextMeshProUGUI object that displays the amount of this item in the shopping cart.
    public TextMeshProUGUI priceText; // The TextMeshProUGUI object that displays the price of the item.
    public bool unlocked; // Tells whether the shop item is unlocked to the player yet. Initial state of whether item is locked/unlocked is determined by the state of this boolean in the Inspector
    public Image lockedImage; // Image that shows up in the shop slot if the shop item is locked.
    public ShopManager shopManager; // ShopManager script that links all the systems together.
    public bool isAdding;
    public bool isRemoving;
    private bool isFiring;
    private bool stopFiring;
    public float timeElapsedSinceButtonDown;

    void Awake(){
        buyStackSize = 0;
        buyStackText = GameObject.FindGameObjectWithTag("buyStackText").GetComponent<TextMeshProUGUI>();
        buyStackText.text = buyStackSize.ToString();
        DisplayPriceText(shopItemSO.cost);
        if (!unlocked){
            lockedImage.enabled = true;
        } else {
            lockedImage.enabled = false;
        }
        shopManager = GameObject.FindGameObjectWithTag("shopManager").GetComponent<ShopManager>();
        timeElapsedSinceButtonDown = 0.0f;
    }

    void Update(){
        if (isAdding || isRemoving){
            timeElapsedSinceButtonDown += Time.deltaTime;
        }
        if (isFiring && isAdding){
            if (unlocked && buyStackSize < 99){
            //if (unlocked && !exceededMaxCapacityOfCart() && buyStackSize < 99){
                buyStackSize++;
                buyStackText.text = buyStackSize.ToString();
                shopManager.totalSeafoamCost += shopItemSO.cost[0];
                shopManager.totalSunsetCost += shopItemSO.cost[1];
                shopManager.totalAmethystCost += shopItemSO.cost[2];
                shopManager.totalCrystallineCost += shopItemSO.cost[3];
                shopManager.updateCostText(shopItemSO.cost, buyStackSize);
            }
        }
        if (isFiring && isRemoving){
            if (buyStackSize > 0 && unlocked){
                buyStackSize--;
                buyStackText.text = buyStackSize.ToString();
                shopManager.totalSeafoamCost -= shopItemSO.cost[0];
                shopManager.totalSunsetCost -= shopItemSO.cost[1];
                shopManager.totalAmethystCost -= shopItemSO.cost[2];
                shopManager.totalCrystallineCost -= shopItemSO.cost[3];
                shopManager.updateCostText(shopItemSO.cost, buyStackSize);
            }
        }
    }

    void DisplayPriceText(int[] cost){
        string seafoamCostStr = "";
        string sunsetCostStr = "";
        string amethystCostStr = "";
        string crystallineCostStr = "";
        if (cost[0] > 0){
            seafoamCostStr = "<color=#5ADB97>" + cost[0].ToString() + " Seafoam Icura</color>\n";
        }
        if (cost[1] > 0){
            sunsetCostStr = "<color=#FF8500>" + cost[1].ToString() + " Sunset Icura</color>\n";
        }
        if (cost[2] > 0){
            amethystCostStr = "<color=#9966CC>" + cost[2].ToString() + " Amethyst Icura</color>\n";
        }
        if (cost[3] > 0){
            crystallineCostStr = "<color=#4B36F3>" + cost[3].ToString() + " Crystalline Icura</color>";
        }
        priceText.text = seafoamCostStr + sunsetCostStr + amethystCostStr + crystallineCostStr;
    }

    public void pointerDown(){
        stopFiring = false;
        makeFireVariableTrue();
    }

    private void makeFireVariableTrue(){
        isFiring = true;
        Invoke("makeFireVariableFalse", timeElapsedSinceButtonDown/100);
    }

    public void pointerUp(){
        isFiring = false;
        stopFiring = true;
        timeElapsedSinceButtonDown = 0.0f;
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

    public void unlockItem(){
        unlocked = true;
        lockedImage.enabled = false;
    }

    /* private bool exceededMaxCapacityOfCart(){
        bool exceededSeafoamCapacity = shopManager.totalSeafoamCost + shopItemSO.cost[0] > 999;
        bool exceededSunsetCapacity = shopManager.totalSunsetCost + shopItemSO.cost[1] > 999;
        bool exceededAmethystCapacity = shopManager.totalAmethystCost + shopItemSO.cost[2] > 999;
        bool exceededCrystallineCapacity = shopManager.totalCrystallineCost + shopItemSO.cost[3] > 999;
        return exceededSeafoamCapacity || exceededSunsetCapacity || exceededAmethystCapacity || exceededCrystallineCapacity;
    } */

    // Selects item when this item is clicked in inventory
    public void Select(){
        if (unlocked && !TimeManager.IsGamePaused()){
            shopManager.selectionArrow.SetActive(true);
            shopManager.numSelectionsPanel.SetActive(true);
            if (shopItemSO is ShopPlantSeed) { 
                shopManager.DisplayInfo((ShopPlantSeed)shopItemSO); // temporary implementation, since downcasting is bad
            }
            Vector2 new_selection_arrow_pos = shopManager.selectionArrow.transform.position;
            Vector2 new_selections_panel_pos = shopManager.numSelectionsPanel.transform.position;
            new_selection_arrow_pos.y = transform.position.y;
            new_selections_panel_pos.y = transform.position.y;
            shopManager.selectionArrow.transform.position = new_selection_arrow_pos;
            shopManager.numSelectionsPanel.transform.position = new_selections_panel_pos;

            shopManager.Reset();

            shopManager.currentlySelectedSlot = this;
            shopManager.UpdateOwnedText(shopItemSO);
            shopManager.UpdateEquippedText(shopItemSO);
        }
    }
}

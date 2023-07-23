using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    public ShopItem shopItemSO; // The scriptable object that contains fixed (non-dynamic) data about this shop item.
    public int buyStackSize; // The The amount of this item that the player is buying.
    public TextMeshProUGUI priceText; // The TextMeshProUGUI object that displays the price of the item.
    public bool unlocked; // Tells whether the shop item is unlocked to the player yet. Initial state of whether item is locked/unlocked is determined by the state of this boolean in the Inspector
    public Image lockedImage; // Image that shows up in the shop slot if the shop item is locked.
    public ShopManager shopManager; // ShopManager script that links all the systems together.
    [HideInInspector]public bool isAdding;
    [HideInInspector]public bool isRemoving;
    [HideInInspector]private bool isFiring;
    [HideInInspector]private bool stopFiring;
    [HideInInspector]public float timeElapsedSinceButtonDown;

    void Awake(){
        DisplayPriceText(shopItemSO.cost);
        Button button = GetComponent<Button>(); 
        if (unlocked){
            lockedImage.enabled = false;
            ColorBlock unlockedColorBlock = button.colors;
            unlockedColorBlock.selectedColor = new Color(0.7843137f,0.7843137f,0.7843137f);
            button.colors = unlockedColorBlock;
        } else {
            lockedImage.enabled = true;
            ColorBlock lockedColorBlock = button.colors;
            lockedColorBlock.selectedColor = new Color(1.0f,1.0f,1.0f);
            button.colors = lockedColorBlock;
        }
        shopManager = GameObject.FindGameObjectWithTag("shopManager").GetComponent<ShopManager>();
        buyStackSize = 0;
        shopManager.buyStackText.text = buyStackSize.ToString();
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
                shopManager.buyStackText.text = buyStackSize.ToString();
                shopManager.totalSeafoamCost += shopItemSO.cost[0];
                shopManager.totalSunsetCost += shopItemSO.cost[1];
                shopManager.totalAmethystCost += shopItemSO.cost[2];
                shopManager.totalCrystallineCost += shopItemSO.cost[3];
                shopManager.updateCostText(shopItemSO.cost, buyStackSize, "buy");
            }
        }
        if (isFiring && isRemoving){
            if (buyStackSize > 0 && unlocked){
                buyStackSize--;
                shopManager.buyStackText.text = buyStackSize.ToString();
                shopManager.totalSeafoamCost -= shopItemSO.cost[0];
                shopManager.totalSunsetCost -= shopItemSO.cost[1];
                shopManager.totalAmethystCost -= shopItemSO.cost[2];
                shopManager.totalCrystallineCost -= shopItemSO.cost[3];
                shopManager.updateCostText(shopItemSO.cost, buyStackSize, "buy");
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

    // Selects item when this item is clicked in inventory
    public void Select(){
        if (unlocked && !TimeManager.IsGamePaused()){
            // Change color of button when selected and changes the previously selected slot's color be back to the assigned unselected color.
            Button button = GetComponent<Button>(); 
            ColorBlock selectedColorBlock = button.colors;
            ColorBlock unselectedColorBlock = button.colors;
            selectedColorBlock.normalColor = new Color(0.7843137f,0.7843137f,0.7843137f);
            unselectedColorBlock.normalColor = new Color(1.0f,1.0f,1.0f);
            button.colors = selectedColorBlock;
            shopManager.currentlySelectedBuySlot.GetComponent<Button>().colors = unselectedColorBlock;

            // Makes the shop selection arrow and selections panel visible.
            shopManager.buyUIselectionArrow.SetActive(true);
            shopManager.buyUInumSelectionsPanel.SetActive(true);
            Vector2 new_selection_arrow_pos = shopManager.buyUIselectionArrow.transform.position;
            Vector2 new_selections_panel_pos = shopManager.buyUInumSelectionsPanel.transform.position;
            new_selection_arrow_pos.y = transform.position.y;
            new_selections_panel_pos.y = transform.position.y;
            shopManager.buyUIselectionArrow.transform.position = new_selection_arrow_pos;
            shopManager.buyUInumSelectionsPanel.transform.position = new_selections_panel_pos;

            // Displays info in the info panel.
            if (shopItemSO is ShopPlantSeed) { 
                shopManager.DisplayInfo((ShopPlantSeed)shopItemSO); // temporary implementation, since downcasting is bad
            } else if (shopItemSO is ShopWeapon) { 
                shopManager.DisplayInfo((ShopWeapon)shopItemSO); // temporary implementation, since downcasting is bad
            }


            shopManager.UpdateOwnedText(shopItemSO); // Updates part in money panel that states how much of this item does player already have in inventory
            shopManager.UpdateEquippedText(shopItemSO); // Updates part in money panel that states whether player already has this item equipped (useful for eventual clothing system)

            // Resets cost panel and internal count of total number of seafoam, sunset, amethyst, and crystalline
            shopManager.Reset();

            // Updates the currently selected slot variable in ShopManager
            shopManager.currentlySelectedBuySlot = this;
        }
    }
}

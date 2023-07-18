using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ShopItem shopItemSO; // The scriptable object that contains fixed (non-dynamic) data about this shop item.
    public int buyStackSize; // The The amount of this item that the player is buying.
    public TextMeshProUGUI buyStackText; // The TextMeshProUGUI object that displays the amount of item player has in cart.
    public bool unlocked; // Tells whether the shop item is unlocked to the player yet. Initial state of whether item is locked/unlocked is determined by the state of this boolean in the Inspector
    public Image lockedImage; // Image that shows up in the shop slot if the shop item is locked.
    public ShopManager shopManager; // ShopManager script that links all the systems together.
    
    void Awake(){
        buyStackSize = 0;
        buyStackText.text = buyStackSize.ToString();
        if (!unlocked){
            lockedImage.enabled = true;
        }
        shopManager = GameObject.FindGameObjectWithTag("shopManager").GetComponent<ShopManager>();
    }

    public void addToBuyStack(){
        if (unlocked && !exceededMaxCapacityOfCart() && buyStackSize < 99){
            buyStackSize++;
            buyStackText.text = buyStackSize.ToString();
            shopManager.totalSeafoamCost += shopItemSO.cost[0];
            shopManager.totalSunsetCost += shopItemSO.cost[1];
            shopManager.totalAmethystCost += shopItemSO.cost[2];
            shopManager.totalCrystallineCost += shopItemSO.cost[3];
            shopManager.updateCostText();
            // TODO: add feature where player is alerted if they exceeded max capacity of cart
        }
    }

    public void removeFromBuyStack(){
        if (buyStackSize > 0 && unlocked){
            buyStackSize--;
            buyStackText.text = buyStackSize.ToString();
            shopManager.totalSeafoamCost -= shopItemSO.cost[0];
            shopManager.totalSunsetCost -= shopItemSO.cost[1];
            shopManager.totalAmethystCost -= shopItemSO.cost[2];
            shopManager.totalCrystallineCost -= shopItemSO.cost[3];
            shopManager.updateCostText();
        }
    }

    public void unlockItem(){
        unlocked = true;
        lockedImage.enabled = false;
    }

    private bool exceededMaxCapacityOfCart(){
        bool exceededSeafoamCapacity = shopManager.totalSeafoamCost + shopItemSO.cost[0] > 999;
        bool exceededSunsetCapacity = shopManager.totalSunsetCost + shopItemSO.cost[1] > 999;
        bool exceededAmethystCapacity = shopManager.totalAmethystCost + shopItemSO.cost[2] > 999;
        bool exceededCrystallineCapacity = shopManager.totalCrystallineCost + shopItemSO.cost[3] > 999;
        return exceededSeafoamCapacity || exceededSunsetCapacity || exceededAmethystCapacity || exceededCrystallineCapacity;
    }

    // Displays info text upon pointer entering slot
    public void OnPointerEnter(PointerEventData eventData){
        if (unlocked && !TimeManager.IsGamePaused()){
            if (shopItemSO is ShopPlantSeed) { 
                shopManager.DisplayInfo((ShopPlantSeed)shopItemSO); // temporary implementation, since downcasting is bad
            }
        }
    }

    // Undisplay info text upon pointer exiting slot
    public void OnPointerExit(PointerEventData eventData){
        if (!TimeManager.IsGamePaused()){
            shopManager.UndisplayInfo();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public GameObject player;
    public float y_offset;

    public Text costText;
    public Text infoText;
    public List<ShopSlot> shopSlots;
    public int totalSeafoamCost;
    public int totalSunsetCost;
    public int totalAmethystCost;
    public int totalCrystallineCost;

    public FruitManager fruitManager;
    public InventoryManager inventoryManager;
    public AudioManager audioManager;

    void Awake(){
        totalSeafoamCost = 0;
        totalSunsetCost = 0;
        totalAmethystCost = 0;
        totalCrystallineCost = 0;
        infoText.text = "";
        updateCostText();
    }

    void Update()
    {
        if (player.GetComponent<PlayerScript>().shopIsLoaded){
            var pos = transform.position;
            pos.x = player.transform.position.x;
            pos.y = player.transform.position.y + y_offset;
            transform.position = pos;
        }
    }

    public void updateCostText(){
        string seafoamCostStr = "";
        string sunsetCostStr = "";
        string amethystCostStr = "";
        string crystallineCostStr = "";
        string color = "";
        if (totalSeafoamCost > 0){
            if (totalSeafoamCost > fruitManager.nSeafoam){ color = "#FF0000"; }
            else { color = "#5ADB97"; }
            seafoamCostStr = $"<color='{color}'>{totalSeafoamCost.ToString()} Seafoam Icura</color>\n";
        }
        if (totalSunsetCost > 0){
            if (totalSunsetCost > fruitManager.nSunset){ color = "#FF0000"; }
            else { color = "#FF8500"; }
            sunsetCostStr = $"<color='{color}'>{totalSunsetCost.ToString()} Sunset Icura</color>\n";
        }
        if (totalAmethystCost > 0){
            if (totalAmethystCost > fruitManager.nAmethyst){ color = "#FF0000"; }
            else { color = "#9966CC"; }
            amethystCostStr = $"<color='{color}'>{totalAmethystCost.ToString()} Amethyst Icura</color>\n";
        }
        if (totalCrystallineCost > 0){
            if (totalCrystallineCost > fruitManager.nCrystalline){ color = "#FF0000"; }
            else { color = "#4B36F3"; }
            crystallineCostStr = $"<color='{color}'>{totalCrystallineCost.ToString()} Crystalline Icura</color>";
        }
        costText.text = seafoamCostStr + sunsetCostStr + amethystCostStr + crystallineCostStr;
    }

    public void DisplayInfo(ShopPlantSeed shopPlantSeedSO){
        string itemName = "<color='#DBDBDB'>" + shopPlantSeedSO.itemName + "</color>\n";
        string biome = "";
        string rarity = "";
        string role = "<color='#DBDBDB'>" + shopPlantSeedSO.mainRole + "</color>\n";
        string cost = "";

        if (shopPlantSeedSO.biome.Equals("Plains")){
            biome = "<color='#3EC500'>Plains</color>\n";
        } else if (shopPlantSeedSO.biome.Equals("City")){
            biome = "<color='#FF0000'>City</color>\n";
        } else if (shopPlantSeedSO.biome.Equals("Cave")){
            biome = "<color='#580089'>Cave</color>\n";
        }

        if (shopPlantSeedSO.rarity.Equals("Common")){
            rarity = "<color='#B0B0B0'>Common</color>\n";
        } else if (shopPlantSeedSO.rarity.Equals("Rare")){
            rarity = "<color='#2889E3'>Rare</color>\n";
        } else if (shopPlantSeedSO.rarity.Equals("Epic")){
            rarity = "<color='#B467FF'>Epic</color>\n";
        } else if (shopPlantSeedSO.rarity.Equals("Legendary")){
            rarity = "<color='#FFFF00'>Legendary</color>\n";
        }

        string fruitType = "";
        for (int i = 0; i < shopPlantSeedSO.cost.Length; i++){
            if (shopPlantSeedSO.cost[i] != 0){
                if (i == 0){ fruitType = "Seafoam"; }
                if (i == 1){ fruitType = "Sunset"; }
                if (i == 2){ fruitType = "Amethyst"; }
                if (i == 3){ fruitType = "Crystalline"; }
                cost += "- " + fruitType + " " + shopPlantSeedSO.cost[i] + " icura\n";
            }
        }

        infoText.text = itemName + biome + rarity + role + "Cost\n" + cost;
    }

    public void UndisplayInfo(){
        infoText.text = "";
    }

    // need to have this itneract w the shop item texts
    public void Buy(){
        if (fruitManager.nSeafoam >= totalSeafoamCost && fruitManager.nSunset >= totalSunsetCost && fruitManager.nAmethyst >= totalAmethystCost && fruitManager.nCrystalline >= totalCrystallineCost){
            audioManager.buySFX.Play();
            
            fruitManager.nSeafoam -= totalSeafoamCost;
            fruitManager.nSunset -= totalSunsetCost;
            fruitManager.nAmethyst -= totalAmethystCost;
            fruitManager.nCrystalline -= totalCrystallineCost;

            Dictionary<string, int> totalCostDict = new Dictionary<string, int>(){
                                        {"Seafoam", totalSeafoamCost},
                                        {"Sunset", totalSunsetCost},
                                        {"Amethyst", totalAmethystCost},
                                        {"Crystalline", totalCrystallineCost}
            };

            totalSeafoamCost = 0;
            totalSunsetCost = 0;
            totalAmethystCost = 0;
            totalCrystallineCost = 0;
            updateCostText();

            foreach (ShopSlot shopSlot in shopSlots){
                if (shopSlot.buyStackSize > 0){
                    inventoryManager.BuyUpdateInventory(shopSlot.shopItemSO.inventoryItemPrefab, shopSlot.buyStackSize, totalCostDict);
                    shopSlot.buyStackSize = 0;
                    shopSlot.buyStackText.text = shopSlot.buyStackSize.ToString();
                }
            }
        } else {
            print("Not enough funds!");
        }
    }

    public void Sell(){
        audioManager.sellSFX.Play();
        //fruitManager.nSeafoam += totalSeafoamSell;
        //fruitManager.nSunset += totalSunsetSell;
        //fruitManager.nAmethyst += totalAmethystSell;
        //fruitManager.nCrystalline += totalCrystallineSell;
        //inventoryItem.Sell(numSell)
    }

    // need to have this interact w the shopItem texts
    public void Reset(){
        totalSeafoamCost = 0;
        totalSunsetCost = 0;
        totalAmethystCost = 0;
        totalCrystallineCost = 0;
        updateCostText();

        foreach (ShopSlot shopSlot in shopSlots){
            shopSlot.buyStackSize = 0;
            shopSlot.buyStackText.text = shopSlot.buyStackSize.ToString();
        }
    }
}
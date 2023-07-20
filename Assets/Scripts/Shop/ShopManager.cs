using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public GameObject player;
    public float y_offset;

    public GameObject selectionArrow;
    public GameObject numSelectionsPanel;
    public ShopSlot_2 currentlySelectedSlot;
    public TextMeshProUGUI costText;
    public GameObject buyButton;
    public TextMeshProUGUI infoText;
    public int totalSeafoamCost;
    public int totalSunsetCost;
    public int totalAmethystCost;
    public int totalCrystallineCost;

    public TextMeshProUGUI ownedStockText;
    public TextMeshProUGUI equippedText;
    public TextMeshProUGUI seafoamStockText;
    public TextMeshProUGUI sunsetStockText;
    public TextMeshProUGUI amethystStockText;
    public TextMeshProUGUI crystallineStockText;

    public FruitManager fruitManager;
    public InventoryManager inventoryManager;
    public AudioManager audioManager;

    void Awake(){
        totalSeafoamCost = 0;
        totalSunsetCost = 0;
        totalAmethystCost = 0;
        totalCrystallineCost = 0;
        infoText.text = "";
        ownedStockText.text = "--";
        equippedText.text = "--";
        costText.text = "";
    }

    void Start(){
        seafoamStockText.text = fruitManager.nSeafoam.ToString();
        sunsetStockText.text = fruitManager.nSunset.ToString();
        amethystStockText.text = fruitManager.nAmethyst.ToString();
        crystallineStockText.text = fruitManager.nCrystalline.ToString();
        selectionArrow.SetActive(false);
        numSelectionsPanel.SetActive(false);
    }

    void Update()
    {
        if (player.GetComponent<PlayerScript>().shopIsLoaded){
            var pos = transform.position;
            pos.x = player.transform.position.x;
            pos.y = player.transform.position.y + y_offset;
            transform.position = pos;
        }
        if (currentlySelectedSlot.buyStackSize > 0){
            buyButton.SetActive(true);
        } else {
            buyButton.SetActive(false);
        }
    }

    public void updateCostText(int[] cost, int buyStackSize){
        string seafoamCostStr = "";
        string sunsetCostStr = "";
        string amethystCostStr = "";
        string crystallineCostStr = "";
        string color = "";
        if (totalSeafoamCost > 0){
            if (totalSeafoamCost > fruitManager.nSeafoam){ color = "#FF0000"; }
            else { color = "#5ADB97"; }
            seafoamCostStr = $"<color={color}>{cost[0].ToString()}x{buyStackSize.ToString()}={totalSeafoamCost.ToString()}</color>\n";
        }
        if (totalSunsetCost > 0){
            if (totalSunsetCost > fruitManager.nSunset){ color = "#FF0000"; }
            else { color = "#FF8500"; }
            sunsetCostStr = $"<color={color}>{cost[1].ToString()}x{buyStackSize.ToString()}={totalSunsetCost.ToString()}</color>\n";
        }
        if (totalAmethystCost > 0){
            if (totalAmethystCost > fruitManager.nAmethyst){ color = "#FF0000"; }
            else { color = "#B383E2"; }
            amethystCostStr = $"<color={color}>{cost[2].ToString()}x{buyStackSize.ToString()}={totalAmethystCost.ToString()}</color>\n";
        }
        if (totalCrystallineCost > 0){
            if (totalCrystallineCost > fruitManager.nCrystalline){ color = "#FF0000"; }
            else { color = "#4B36F3"; }
            crystallineCostStr = $"<color={color}>{cost[3].ToString()}x{buyStackSize.ToString()}={totalCrystallineCost.ToString()}</color>\n";
        }
        costText.text = seafoamCostStr + sunsetCostStr + amethystCostStr + crystallineCostStr;
    }

    public void UpdateFruitStockText(){
        seafoamStockText.text = fruitManager.nSeafoam.ToString();
        sunsetStockText.text = fruitManager.nSunset.ToString();
        amethystStockText.text = fruitManager.nAmethyst.ToString();
        crystallineStockText.text = fruitManager.nCrystalline.ToString();
    }

    public void UpdateOwnedText(ShopItem shopItemSO){
        int totalOwned = 0;
        for (int i = 0; i < inventoryManager.inventorySlots.Count; i++){
            InventorySlot inventorySlot = inventoryManager.inventorySlots[i];
            Transform slotTransform = inventorySlot.transform.GetChild(0); 
            if (slotTransform.childCount > 0){
                InventoryItem inventoryItem = slotTransform.GetComponentInChildren<InventoryItem>();
                if (inventoryItem.displayName.Equals(shopItemSO.inventoryItemPrefab.GetComponent<InventoryItem>().displayName)){
                    totalOwned += inventoryItem.stackSize;
                }
            }
        }
        ownedStockText.text = totalOwned.ToString();
    }

    // TODO: implement this later when we actually have equipping clothing system for player
    public void UpdateEquippedText(ShopItem shopItemSO){

    }

    public void DisplayInfo(ShopPlantSeed shopPlantSeedSO){
        string itemName = "<color=#DBDBDB>" + shopPlantSeedSO.itemName + "</color>\n";
        string biome = "";
        string rarity = "";
        string role = "<color=#DBDBDB>" + shopPlantSeedSO.mainRole + "</color>\n";

        if (shopPlantSeedSO.biome.Equals("Plains")){
            biome = "<color=#3EC500>Plains</color>\n";
        } else if (shopPlantSeedSO.biome.Equals("City")){
            biome = "<color=#FF0000>City</color>\n";
        } else if (shopPlantSeedSO.biome.Equals("Cave")){
            biome = "<color=#580089>Cave</color>\n";
        }

        if (shopPlantSeedSO.rarity.Equals("Common")){
            rarity = "<color=#B0B0B0>Common</color>\n";
        } else if (shopPlantSeedSO.rarity.Equals("Rare")){
            rarity = "<color=#2889E3>Rare</color>\n";
        } else if (shopPlantSeedSO.rarity.Equals("Epic")){
            rarity = "<color=#B467FF>Epic</color>\n";
        } else if (shopPlantSeedSO.rarity.Equals("Legendary")){
            rarity = "<color=#FFFF00>Legendary</color>\n";
        }

        infoText.text = itemName + biome + rarity + role;
    }

    public void UndisplayInfo(){
        infoText.text = "";
    }

    public void pointerDownAddToBuyStack(){
        currentlySelectedSlot.isAdding = true;
        currentlySelectedSlot.pointerDown();
    }

    public void pointerUpAddToBuyStack(){
        currentlySelectedSlot.isAdding = false;
        currentlySelectedSlot.pointerUp();
    }

    public void pointerDownRemoveFromBuyStack(){
        currentlySelectedSlot.isRemoving = true;
        currentlySelectedSlot.pointerDown();
    }

    public void pointerUpRemoveFromBuyStack(){
        currentlySelectedSlot.isRemoving = false;
        currentlySelectedSlot.pointerUp();
    }

    public void Reset(){
        totalSeafoamCost = 0;
        totalSunsetCost = 0;
        totalAmethystCost = 0;
        totalCrystallineCost = 0;
        costText.text = "";
        currentlySelectedSlot.buyStackSize = 0;
        currentlySelectedSlot.buyStackText.text = "0";
    }

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

            if (currentlySelectedSlot.buyStackSize > 0){
                inventoryManager.BuyUpdateInventory(currentlySelectedSlot.shopItemSO.inventoryItemPrefab, currentlySelectedSlot.buyStackSize, totalCostDict);
                UpdateFruitStockText();
                ownedStockText.text = (int.Parse(ownedStockText.text) + currentlySelectedSlot.buyStackSize).ToString();
            }

            Reset();
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
}
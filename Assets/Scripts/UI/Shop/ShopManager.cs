using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public GameObject player;
    private PlayerScript playerScript;
    public float y_offset;

    public GameObject selectUI;
    public GameObject buyUI;
    public GameObject sellUI;

    public Button selectedShopTabButton;
    [HideInInspector] public ColorBlock selectedColorBlock; 
    [HideInInspector] public ColorBlock unselectedColorBlock;
    public GameObject plainsScrollView;
    public GameObject cityScrollView;
    public GameObject caveScrollView;
    public GameObject weaponScrollView;

    public GameObject buyUIselectionArrow;
    public float topEdgeBuyUI; // Serialized in Inspector to 3.10f
    public float bottomEdgeBuyUI; // Serialized in Inspector to 0.68f
    public GameObject buyUInumSelectionsPanel;
    public TextMeshProUGUI buyStackText;
    public ShopSlot currentlySelectedBuySlot;
    public GameObject buyButton;
    public TextMeshProUGUI buyUIcostText;

    public GameObject sellUIselectionArrow;
    public GameObject sellUInumSelectionsPanel;
    public TextMeshProUGUI sellStackText;
    public SellSlot currentlySelectedSellSlot;
    public GameObject sellButton;
    public TextMeshProUGUI sellUIcostText;

    public int totalSeafoamCost;
    public int totalSunsetCost;
    public int totalAmethystCost;
    public int totalCrystallineCost;

    public GameObject moneyPanel;
    public GameObject infoPanel;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI ownedStockText;
    public TextMeshProUGUI equippedText;
    public TextMeshProUGUI seafoamStockText;
    public TextMeshProUGUI sunsetStockText;
    public TextMeshProUGUI amethystStockText;
    public TextMeshProUGUI crystallineStockText;

    public FruitManager fruitManager;
    public InventoryManager inventoryManager;
    public AudioManager audioManager;

    public ShopSlot[] shopSlots;
    public bool isBuySlotSelected;

    void Awake(){
        shopSlots = GetComponentsInChildren<ShopSlot>(true);
        playerScript = player.GetComponent<PlayerScript>();
        selectUI.SetActive(true);
        buyUI.SetActive(false);
        sellUI.SetActive(false);
        moneyPanel.SetActive(false);
        infoPanel.SetActive(false);
        totalSeafoamCost = 0;
        totalSunsetCost = 0;
        totalAmethystCost = 0;
        totalCrystallineCost = 0;
        infoText.text = "";
        ownedStockText.text = "--";
        equippedText.text = "--";
        buyUIcostText.text = "";
        sellUIcostText.text = "";
        selectedColorBlock = selectedShopTabButton.colors;
        unselectedColorBlock = selectedShopTabButton.colors;
        selectedColorBlock.normalColor = new Color(0.7843137f, 0.7843137f, 0.7843137f);
        unselectedColorBlock.normalColor = new Color(1.0f, 1.0f, 1.0f);
        selectedShopTabButton.colors = selectedColorBlock;
        plainsScrollView.SetActive(true);
        cityScrollView.SetActive(false);
        caveScrollView.SetActive(false);
        weaponScrollView.SetActive(false);
        isBuySlotSelected = false;
    }

    void Start(){
        seafoamStockText.text = fruitManager.nSeafoam.ToString();
        sunsetStockText.text = fruitManager.nSunset.ToString();
        amethystStockText.text = fruitManager.nAmethyst.ToString();
        crystallineStockText.text = fruitManager.nCrystalline.ToString();
        buyUIselectionArrow.SetActive(false);
        buyUInumSelectionsPanel.SetActive(false);
        sellUIselectionArrow.SetActive(false);
        sellUInumSelectionsPanel.SetActive(false);
    }

    void Update()
    {
        if (playerScript.shopIsLoaded){
            var pos = transform.position;
            pos.x = player.transform.position.x;
            pos.y = player.transform.position.y + y_offset;
            transform.position = pos;

            if (isBuySlotSelected){
                Vector2 newBuyUIselectionArrowPosition = buyUIselectionArrow.transform.position;
                newBuyUIselectionArrowPosition.y = currentlySelectedBuySlot.transform.position.y;
                buyUIselectionArrow.transform.position = newBuyUIselectionArrowPosition;
                Vector2 newBuyUInumSelectionsPanelPosition = buyUInumSelectionsPanel.transform.position;
                newBuyUInumSelectionsPanelPosition.y = currentlySelectedBuySlot.transform.position.y;
                buyUInumSelectionsPanel.transform.position = newBuyUInumSelectionsPanelPosition;
                if (buyUIselectionArrow.transform.position.y > topEdgeBuyUI || buyUIselectionArrow.transform.position.y < bottomEdgeBuyUI){
                    buyUIselectionArrow.SetActive(false);
                    buyUInumSelectionsPanel.SetActive(false);
                } else {
                    buyUIselectionArrow.SetActive(true);
                    buyUInumSelectionsPanel.SetActive(true);
                }
            }
            
            if (currentlySelectedBuySlot.buyStackSize > 0){
                buyButton.SetActive(true);
            } else {
                buyButton.SetActive(false);
            }
            if (currentlySelectedSellSlot.sellStackSize > 0){
                sellButton.SetActive(true);
            } else {
                sellButton.SetActive(false);
            }
        }
    }

    public void OpenShopSelectUI(){
        Reset();
        isBuySlotSelected = false;
        currentlySelectedSellSlot.GetComponent<Button>().colors = unselectedColorBlock;
        buyUIselectionArrow.SetActive(false);
        buyUInumSelectionsPanel.SetActive(false);
        sellUIselectionArrow.SetActive(false);
        sellUInumSelectionsPanel.SetActive(false);
        moneyPanel.SetActive(false);
        infoPanel.SetActive(false);
        infoText.text = "";
        ownedStockText.text = "--";
        equippedText.text = "--";
        selectUI.SetActive(true);
        buyUI.SetActive(false);
        sellUI.SetActive(false);
    }

    public void SelectBuyUI(){
        Reset();
        isBuySlotSelected = false;
        currentlySelectedSellSlot.GetComponent<Button>().colors = unselectedColorBlock;
        currentlySelectedBuySlot.GetComponent<Button>().colors = unselectedColorBlock;
        buyUIselectionArrow.SetActive(false);
        buyUInumSelectionsPanel.SetActive(false);
        moneyPanel.SetActive(true);
        infoPanel.SetActive(true);
        infoText.text = "";
        ownedStockText.text = "--";
        equippedText.text = "--";
        selectUI.SetActive(false);
        buyUI.SetActive(true);
        sellUI.SetActive(false);
    }

    public void SelectSellUI(){
        Reset();
        isBuySlotSelected = false;
        currentlySelectedBuySlot.GetComponent<Button>().colors = unselectedColorBlock;
        sellUIselectionArrow.SetActive(false);
        sellUInumSelectionsPanel.SetActive(false);
        moneyPanel.SetActive(true);
        infoPanel.SetActive(true);
        infoText.text = "";
        ownedStockText.text = "--";
        equippedText.text = "--";
        selectUI.SetActive(false);
        buyUI.SetActive(false);
        sellUI.SetActive(true);
    }

    public void Reset(){
        if (!TimeManager.IsGamePaused()){
            totalSeafoamCost = 0;
            totalSunsetCost = 0;
            totalAmethystCost = 0;
            totalCrystallineCost = 0;
            buyUIcostText.text = "";
            sellUIcostText.text = "";
            currentlySelectedBuySlot.buyStackSize = 0;
            buyStackText.text = "0";
            currentlySelectedSellSlot.sellStackSize = 0;
            sellStackText.text = "0";
        }
    }

    public void SwitchShopTab(string newShopTabName){
        Reset();
        isBuySlotSelected = false;
        currentlySelectedBuySlot.GetComponent<Button>().colors = unselectedColorBlock;
        buyUIselectionArrow.SetActive(false);
        buyUInumSelectionsPanel.SetActive(false);
        infoText.text = "";
        ownedStockText.text = "--";
        equippedText.text = "--";
        
        if (newShopTabName.Equals("Plains")){
            plainsScrollView.SetActive(true);
            cityScrollView.SetActive(false);
            caveScrollView.SetActive(false);
            weaponScrollView.SetActive(false);
        } else if (newShopTabName.Equals("City")){
            plainsScrollView.SetActive(false);
            cityScrollView.SetActive(true);
            caveScrollView.SetActive(false);
            weaponScrollView.SetActive(false);
        } else if (newShopTabName.Equals("Cave")){
            plainsScrollView.SetActive(false);
            cityScrollView.SetActive(false);
            caveScrollView.SetActive(true);
            weaponScrollView.SetActive(false);
        } else if (newShopTabName.Equals("Weapon")){
            plainsScrollView.SetActive(false);
            cityScrollView.SetActive(false);
            caveScrollView.SetActive(false);
            weaponScrollView.SetActive(true);
        } else {
            print("Your shop tab names are not correct! Use either 'Plains', 'City', 'Cave', or 'Weapon'.");
        }
    }

    public void updateCostText(int[] cost, int stackSize, string mode){
        string seafoamCostStr = "";
        string sunsetCostStr = "";
        string amethystCostStr = "";
        string crystallineCostStr = "";
        string color = "";
        if (totalSeafoamCost > 0){
            if (totalSeafoamCost > fruitManager.nSeafoam && mode.Equals("buy")){ color = "#FF0000"; }
            else { color = "#5ADB97"; }
            seafoamCostStr = $"<color={color}>{cost[0].ToString()}x{stackSize.ToString()}={totalSeafoamCost.ToString()}</color>\n";
        }
        if (totalSunsetCost > 0){
            if (totalSunsetCost > fruitManager.nSunset && mode.Equals("buy")){ color = "#FF0000"; }
            else { color = "#FF8500"; }
            sunsetCostStr = $"<color={color}>{cost[1].ToString()}x{stackSize.ToString()}={totalSunsetCost.ToString()}</color>\n";
        }
        if (totalAmethystCost > 0){
            if (totalAmethystCost > fruitManager.nAmethyst && mode.Equals("buy")){ color = "#FF0000"; }
            else { color = "#B383E2"; }
            amethystCostStr = $"<color={color}>{cost[2].ToString()}x{stackSize.ToString()}={totalAmethystCost.ToString()}</color>\n";
        }
        if (totalCrystallineCost > 0){
            if (totalCrystallineCost > fruitManager.nCrystalline && mode.Equals("buy")){ color = "#FF0000"; }
            else { color = "#4B36F3"; }
            crystallineCostStr = $"<color={color}>{cost[3].ToString()}x{stackSize.ToString()}={totalCrystallineCost.ToString()}</color>\n";
        }
        if (buyUI.activeSelf) { buyUIcostText.text = seafoamCostStr + sunsetCostStr + amethystCostStr + crystallineCostStr; }
        if (sellUI.activeSelf) { sellUIcostText.text = seafoamCostStr + sunsetCostStr + amethystCostStr + crystallineCostStr; }
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

    public void DisplayInfo(ShopWeapon shopWeaponSO){
        string itemName = "<color=#DBDBDB>" + shopWeaponSO.itemName + "</color>\n";
        string rarity = "";
        string range = "<color=#DBDBDB>" + shopWeaponSO.range + "</color>\n";

        if (shopWeaponSO.rarity.Equals("Common")){
            rarity = "<color=#B0B0B0>Common</color>\n";
        } else if (shopWeaponSO.rarity.Equals("Rare")){
            rarity = "<color=#2889E3>Rare</color>\n";
        } else if (shopWeaponSO.rarity.Equals("Epic")){
            rarity = "<color=#B467FF>Epic</color>\n";
        } else if (shopWeaponSO.rarity.Equals("Legendary")){
            rarity = "<color=#FFFF00>Legendary</color>\n";
        }

        infoText.text = itemName + rarity + range;
    }

    public void UndisplayInfo(){
        infoText.text = "";
    }

    public void Buy(){
        if (!TimeManager.IsGamePaused()){
            if (fruitManager.nSeafoam >= totalSeafoamCost && fruitManager.nSunset >= totalSunsetCost && fruitManager.nAmethyst >= totalAmethystCost && fruitManager.nCrystalline >= totalCrystallineCost && currentlySelectedBuySlot.buyStackSize > 0){
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

                inventoryManager.BuyUpdateInventory(currentlySelectedBuySlot.shopItemSO.inventoryItemPrefab, currentlySelectedBuySlot.buyStackSize, totalCostDict);
                UpdateFruitStockText();
                ownedStockText.text = (int.Parse(ownedStockText.text) + currentlySelectedBuySlot.buyStackSize).ToString();
                
                // If the item you are buying is not stackable, then prevent player from buying the item again
                if (!currentlySelectedBuySlot.shopItemSO.inventoryItemPrefab.GetComponent<InventoryItem>().stackable){
                    currentlySelectedBuySlot.makeOutOfStock();
                }
                Reset();
            } else {
                print("Not enough funds!");
            }
        }
    }

    public void Sell(){
        if (!TimeManager.IsGamePaused()){
            audioManager.sellSFX.Play();
            fruitManager.nSeafoam += totalSeafoamCost;
            fruitManager.nSunset += totalSunsetCost;
            fruitManager.nAmethyst += totalAmethystCost;
            fruitManager.nCrystalline += totalCrystallineCost;
            if (currentlySelectedSellSlot.sellStackSize == currentlySelectedSellSlot.linkedInventoryItem.stackSize){
                sellUIselectionArrow.SetActive(false);
                sellUInumSelectionsPanel.SetActive(false);
                currentlySelectedSellSlot.button.colors = currentlySelectedSellSlot.unselectedColorBlock;
            }
            currentlySelectedSellSlot.linkedShopItem.Sell(currentlySelectedSellSlot.sellStackSize, currentlySelectedSellSlot.linkedInventoryItem.gameObject);

            UpdateFruitStockText();
            ownedStockText.text = (int.Parse(ownedStockText.text) - currentlySelectedSellSlot.sellStackSize).ToString();
            
            // If selling a non-stackable item, then the shop will receive back their item and be in stock for the item again. 
            if (!currentlySelectedSellSlot.linkedShopItem.stackable){
                foreach (ShopSlot shopSlot in shopSlots){
                    if (shopSlot.shopItemSO.inventoryItemPrefab.GetComponent<InventoryItem>().displayName.Equals(currentlySelectedSellSlot.linkedShopItem.displayName)){
                        shopSlot.makeInStock();
                        break;
                    }
                }
            }
            Reset();
        }
    }

    public void pointerDownAddToStack(){
        if (!TimeManager.IsGamePaused()){
            if (buyUI.activeSelf){
                currentlySelectedBuySlot.isAdding = true;
                currentlySelectedBuySlot.pointerDown();
            }
            if (sellUI.activeSelf){
                currentlySelectedSellSlot.isAdding = true;
                currentlySelectedSellSlot.pointerDown();
            }
        }
    }

    public void pointerUpAddToStack(){
        if (buyUI.activeSelf){
            currentlySelectedBuySlot.isAdding = false;
            currentlySelectedBuySlot.pointerUp();
        }
        if (sellUI.activeSelf){
            currentlySelectedSellSlot.isAdding = false;
            currentlySelectedSellSlot.pointerUp();
        }
    }

    public void pointerDownRemoveFromStack(){
        if (!TimeManager.IsGamePaused()){
            if (buyUI.activeSelf){
                currentlySelectedBuySlot.isRemoving = true;
                currentlySelectedBuySlot.pointerDown();
            }
            if (sellUI.activeSelf){
                currentlySelectedSellSlot.isRemoving = true;
                currentlySelectedSellSlot.pointerDown();
            }
        }
    }

    public void pointerUpRemoveFromStack(){
        if (buyUI.activeSelf){
            currentlySelectedBuySlot.isRemoving = false;
            currentlySelectedBuySlot.pointerUp();
        }
        if (sellUI.activeSelf){
                currentlySelectedSellSlot.isRemoving = false;
                currentlySelectedSellSlot.pointerUp();
            }
    }
}
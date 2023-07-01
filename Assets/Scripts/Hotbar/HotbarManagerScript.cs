using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarManagerScript : MonoBehaviour
{
    public int numHotbarSlots;
    public HotbarItem[] hotbarItems;
    public GameObject hotbarItemPrefab;
    public List<Transform> linkedSlotTransforms; // This variable is how the hotbar manager knows which inventory slots to look at that correspond with the hotbar
    public GameObject inventoryPanel;
    public TextMeshProUGUI nSeafoam;
    public TextMeshProUGUI nSunset;
    public TextMeshProUGUI nAmethyst;
    public TextMeshProUGUI nCrystalline; 
    public FruitManager fruitManager;

    #region Initializing hotbar
    void Awake(){
        hotbarItems = new HotbarItem[numHotbarSlots];
    }

    void Start(){
        LinkSlotTransforms();
        UpdateFruitText();
    }

    public void LinkSlotTransforms(){
        linkedSlotTransforms = new List<Transform>(numHotbarSlots);
        for (int i = 0; i < numHotbarSlots; i++){
            InventoryManager inventoryManager = inventoryPanel.GetComponent<InventoryManager>();
            linkedSlotTransforms.Add(inventoryPanel.transform.GetChild(i + (inventoryManager.inventorySlots.Count - numHotbarSlots)).GetChild(0));
        }
    }
    #endregion

    // This method updates hotbar and is triggered every time an item is added to inventory, dragged into hotbar slot, or dragged into trash icon.
    public void UpdateHotbar(){
        DeleteHotbar(); // First delete old hotbar, then start afresh.
        for (int i = 0; i < numHotbarSlots; i++){
            InventoryManager inventoryManager = inventoryPanel.GetComponent<InventoryManager>();
            if (linkedSlotTransforms[i].childCount > 0){
                // Instantiates hotbar item and fills in all its variables.
                GameObject newHotbarItem = Instantiate(hotbarItemPrefab, transform.GetChild(i));
                InventoryItem linkedInventoryItem = linkedSlotTransforms[i].GetComponentInChildren<InventoryItem>();
                HotbarItem newHotbarComponent = newHotbarItem.GetComponent<HotbarItem>();

                newHotbarComponent.displayName = linkedInventoryItem.displayName;
                newHotbarComponent.image = linkedInventoryItem.image;
                newHotbarComponent.stackable = linkedInventoryItem.stackable;
                newHotbarComponent.stackSize = linkedInventoryItem.stackSize;
                newHotbarComponent.stackSizeText = linkedInventoryItem.stackSizeText;
                newHotbarComponent.sprite = linkedInventoryItem.sprite;

                TextMeshProUGUI newStackSizeText = newHotbarComponent.GetComponentInChildren<TextMeshProUGUI>();
                if (linkedInventoryItem.stackable){
                    newStackSizeText.text = newHotbarComponent.stackSize.ToString();
                } else {
                    newStackSizeText.text = "";
                }
                
                newHotbarItem.GetComponent<Image>().sprite = newHotbarComponent.sprite;

                hotbarItems[i] = newHotbarComponent;
            }
        }
    }

    // Deletes the items currently in hotbar.
    public void DeleteHotbar(){
        for (int i = 0; i < numHotbarSlots; i++){
            hotbarItems[i] = null;
            if (transform.GetChild(i).childCount > 0){
                Destroy(transform.GetChild(i).GetChild(0).gameObject);
            }
        }
    }

    // Updates the text in hotbar that says how many seafoam, sunset, amethyst, and crystalline icura fruit player has in inventory.
    // Is triggered whenever player picks up a new fruit or trashes a fruit. (TODO: also will be triggered whenever player spends fruit on items at Mav's shop).
    public void UpdateFruitText(){
        nSeafoam.text = fruitManager.nSeafoam.ToString();
        nSunset.text = fruitManager.nSunset.ToString();
        nAmethyst.text = fruitManager.nAmethyst.ToString();
        nCrystalline.text = fruitManager.nCrystalline.ToString();
    }
}

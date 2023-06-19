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
    public Transform[] linkedSlotTransforms;
    public GameObject inventoryPanel;
    public TextMeshProUGUI nSeafoam;
    public TextMeshProUGUI nSunset;
    public TextMeshProUGUI nAmethyst;
    public TextMeshProUGUI nCrystalline; 
    public FruitManager fruitManager;

    void Start(){
        hotbarItems = new HotbarItem[numHotbarSlots];
        fruitManager = GameObject.FindGameObjectWithTag("fruitManager").GetComponent<FruitManager>();
        UpdateFruitText();
    }

    public void LinkSlotTransforms(){
        linkedSlotTransforms = new Transform[numHotbarSlots];
        for (int i = 0; i < numHotbarSlots; i++){
            InventoryManager inventoryManager = inventoryPanel.GetComponent<InventoryManager>();
            linkedSlotTransforms[i] = inventoryPanel.transform.GetChild(i + (inventoryManager.inventorySlots.Count - numHotbarSlots)).GetChild(0);
        }
    }

    public void UpdateHotbar(){
        DeleteHotbar();
        for (int i = 0; i < numHotbarSlots; i++){
            InventoryManager inventoryManager = inventoryPanel.GetComponent<InventoryManager>();
            if (linkedSlotTransforms[i].childCount > 0){
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

    public void DeleteHotbar(){
        for (int i = 0; i < numHotbarSlots; i++){
            hotbarItems[i] = null;
            if (transform.GetChild(i).childCount > 0){
                Destroy(transform.GetChild(i).GetChild(0).gameObject);
            }
        }
    }

    public void UpdateFruitText(){
        nSeafoam.text = fruitManager.nSeafoam.ToString();
        nSunset.text = fruitManager.nSunset.ToString();
        nAmethyst.text = fruitManager.nAmethyst.ToString();
        nCrystalline.text = fruitManager.nCrystalline.ToString();
    }
}

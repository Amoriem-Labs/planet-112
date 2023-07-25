using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopBiomeTab : MonoBehaviour
{
    public ShopManager shopManager;
    public PlayerScript playerScript;
    public string thisShopTabName; // Serialize this in Inspector

    public void SelectShopTab(){
        if (!TimeManager.IsGamePaused() && !playerScript.inventoryIsLoaded){
            // Change color of button when selected and changes the previously selected shop tab button's color be back to the assigned unselected color.
            Button button = GetComponent<Button>(); 
            button.colors = shopManager.selectedColorBlock;
            shopManager.selectedShopTabButton.colors = shopManager.unselectedColorBlock;
            shopManager.selectedShopTabButton = button;

            // Loads in different items for the new shop tab
            shopManager.SwitchShopTab(thisShopTabName);
        }
    }
}

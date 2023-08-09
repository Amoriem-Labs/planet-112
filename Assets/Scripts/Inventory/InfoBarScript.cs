using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoBarScript : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI infoText;
    private Color transparent;
    private Color full;

    // Initializing InfoBar
    void Awake(){
        infoText = GetComponentInChildren<TextMeshProUGUI>();
        infoText.text = "";
        image.sprite = null;
        transparent = image.color;
        transparent.a = 0;
        image.color = transparent;
        full = image.color;
        full.a = 1;
    }

    public void DisplayInfo(InventoryItem item){
        infoText.text = item.infoText;
        image.sprite = item.sprite;
        image.color = full;
    }

    public void UndisplayInfo(){
        infoText.text = "";
        image.sprite = null;
        image.color = transparent;
    }
}

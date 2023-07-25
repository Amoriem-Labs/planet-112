using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarItem : MonoBehaviour
{
    public string displayName;
    public Image image;
    public Sprite sprite;
    public bool stackable;
    public int stackSize;
    public TextMeshProUGUI stackSizeText;
}

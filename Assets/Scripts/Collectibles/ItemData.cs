using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public bool stackable;
    public string hoverText;
}

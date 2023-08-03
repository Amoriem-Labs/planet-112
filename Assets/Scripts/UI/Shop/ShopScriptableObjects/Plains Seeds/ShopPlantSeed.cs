using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Seed", menuName = "Plant Seed")]
// Blueprint for easily designing new shop items types
public class ShopPlantSeed : ShopItem
{
    public string biome; // biome that plant comes from
    public string mainRole; // main role of plant
}
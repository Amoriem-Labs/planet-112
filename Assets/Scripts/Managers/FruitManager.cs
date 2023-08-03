using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitManager : MonoBehaviour
{
    public int nSeafoam = 0;
    public int nSunset = 0;
    public int nAmethyst = 0;
    public int nCrystalline = 0;

    public void Reset(){
        nSeafoam = 0;
        nSunset = 0;
        nAmethyst = 0;
        nCrystalline = 0;
    }

    public void AddToFruitStack(string fruitType){
        if (fruitType.Equals("Seafoam")){
            nSeafoam += 1;
        }
        if (fruitType.Equals("Sunset")){
            nSunset += 1;
        }
        if (fruitType.Equals("Amethyst")){
            nAmethyst += 1;
        }
        if (fruitType.Equals("Crystalline")){
            nCrystalline += 1;
        }
    }

    public void RemoveFromFruitStack(string fruitType){
        if (fruitType.Equals("seafoam")){
            nSeafoam -= 1;
        }
        if (fruitType.Equals("sunset")){
            nSunset -= 1;
        }
        if (fruitType.Equals("amethyst")){
            nAmethyst -= 1;
        }
        if (fruitType.Equals("crystalline")){
            nCrystalline -= 1;
        }
    }
}

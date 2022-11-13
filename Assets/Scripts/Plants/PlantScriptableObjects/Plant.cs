using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "Plant")]
public class Plant : ScriptableObject // blue print for Plant.
{
    // member variabels that define plant properties. To set by children.
    public int maxStage; // ex 3 stages. Phases: 0-1, 1-2, 2-3. 0-3 indices per stage, but 3 intervals with ind 0 1 2. 
    public Sprite[] spriteArray; // size = maxStage + 1
    public float[] stageTimeMax; // time until growth to next stage, size = maxStage
    public float[] maxHealth; // deafult HP of the plant (or an array too? yeah why not)
    public PlantNames pName; // oxygen boost is not in here because we might want some oxygen-consuming plants even for game balancing ;D

    // delegates, can be kept here, tested.
    public delegate void OnPlantStageUpdateDelegate();
    public OnPlantStageUpdateDelegate plantStageUpdateDelegate;

    // currStageOfLife is the accessing index to everything below.
    // They don't have to all be filled! Only fill the parts that are needed for the module.
    // Variables specific to plant's productivity
    public int[] oxygenProductionLevels; // note: plant stage starts at 0, the seed! // size = maxStage + 1
    public int[] secondsPerFruitProductionLevels; // size = maxStage + 1

    // Variables specific to plant's attack ability
    public int[] attackDamageLevels; 

    // Variables specific to plant's defensiveness
    public int[] tauntRadiusLevels; // for example
}

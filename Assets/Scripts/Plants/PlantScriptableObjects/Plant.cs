using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plant", menuName = "Plant")]
// Blueprint for easily designing new plant types
public class Plant : ScriptableObject
{
    // Member variables that define plant properties. To be set by children. They don't have to all be filled! Only fill the parts that are needed for the module.
    public PlantName pName;
    public int maxStage; // TODO: have an array of stage names instead, use stages.
    public int maxAttackers; // max number of pests that can target it at a time
    public PlantTargetPriority pestAttackPriority; // enum class for the priority of pest attack
    
    // currStageOfLife is the accessing index to everything below. Stage 0 is a seed, everything builds on this.
    public Sprite[] spriteArray; // Array of sprites per each growth stage
    public float[] stageTimeMax; // Time spent in each growth stage
    public float[] maxHealth; // Max HP for each stage

    public PlantModuleEnum[] defaultModules; // default modules to this class of plants
    public MultiDimensionalArray[] relativeGridsOccupied; // additional spaces this plant will occupy, scales with different stages in life
    // DUMB IDEA.. bad visual. //public MultiDimensionalArray[] relativeGridsOfPestInterest; // spots that the pest will stay to attack, scales as well. Notice need at least 1. 
    public MultiDimensionalArray[] targetRectParameters; // first x,y is offset of bottom center from bottom center, second x,y is width and height. Diff for diff stage.
    public Vector2[] hitboxSize; // dimension of the 2D box physical collider of this plant
    public Vector2[] hitboxOffset; // offset of the 2D box physical collider of this plant from bottom center

    // Modules can subscribe to these delegates to react to changes when called. 
    public delegate void OnPlantStageUpdateDelegate();
    public OnPlantStageUpdateDelegate plantStageUpdateDelegate;

    // Module data
    // For FruitProductionModule:
    public float[] fruitProductionRate; // numSeconds for a production cycle to happen
    public int[] fruitProductionQuantity; // number of fruits per cycle of production
    public FruitType fruitType; // icura type enum
    // For OxygenProductionModule:
    public int[] oxygenProductionQuantity; // number of oxygen this plant gives to atmosphere
    // For HealingModule:
    public float[] healRate; // numSeconds for a healing cycle to happen
    public float[] healAmount; // healing amt, either flat, max hp percentage, curr hp percentage, or curr missing hp %
    public HealMode[] healMode; // one of the heal modes
    public float[] healRangeRadius; // size (radius) of the circular detection range from the center of the plant
    // For AoeDamageModule:
    public float[] aoeAttackRate;
    public float[] aoeDamageAmount;
    public float[] aoeDamageRangeRadius;
    public int[] aoeMaxPestsTargetable;
    // For TauntModule:
    public float[] tauntRangeRadius;
    // For FruitProductionBoostModule:
    public float[] fruitProductionBoostDecimal;
    public float[] fruitProductionBoostRangeRadius;
    // For OxygenProductionBoostModule:
    public float[] oxygenProductionBoostDecimal;
    public float[] oxygenProductionBoostRangeRadius;
    // For UnlockPlantability (not a module):
    public bool unlockPlantability;
}

[System.Serializable]
public class MultiDimensionalArray // for 2D array in the editor, up to 7
{
    public Vector2[] vec2Array;
}

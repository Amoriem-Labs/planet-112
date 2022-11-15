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
    
    // currStageOfLife is the accessing index to everything below. Stage 0 is a seed, everything builds on this.
    public Sprite[] spriteArray; // Array of sprites per each growth stage
    public float[] stageTimeMax; // Time spent in each growth stage
    public float[] maxHealth; // Max HP for each stage

    public PlantModules[] defaultModules; // default modules to this class of plants
    // TODO: what if two plants grow larger into conflict... should we halt growth or? Simple approach: just outline the biggest space it occupies at beginning.
    public MultiDimensionalArray[] relativeGridsOccupied; // spaces this plant will occupy, scales with different stages in life?

    // Produce:
    public int[] oxygenProductionLevels; // TODO: oxygen-consuming plants could have negative levels in certain stages?
    public float[] productionTimes; // Time to produce one unit of whatever the plant makes

    // Modules can subscribe to these delegates to react to changes when called. 
    public delegate void OnPlantStageUpdateDelegate();
    public OnPlantStageUpdateDelegate plantStageUpdateDelegate;
}

[System.Serializable]
public class MultiDimensionalArray // for 2D array in the editor, up to 7
{
    public Vector2[] vec2Array;
}

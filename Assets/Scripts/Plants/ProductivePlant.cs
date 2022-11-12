using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductivePlant : PlantScript
{
    // Values specific to this class of plants
    public int[] oxygenProductionLevels; // note: plant stage starts at 0, the seed! // size = maxStage + 1
    public int[] secondsPerFruitProductionLevels; // size = maxStage + 1
    public int oxygenProduction;
    private int secondsPerFruitProduction;

    public override void UpdatePlantStats(int currStage)
    {
        // Stage start at 0!
        oxygenProduction = oxygenProductionLevels[currStage]; 
        secondsPerFruitProduction = secondsPerFruitProductionLevels[currStage];
    }
}

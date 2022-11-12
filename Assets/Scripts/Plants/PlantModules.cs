using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ProduceOxygen : IProduce
{
    ProductivePlant plant;
    public ProduceOxygen(ProductivePlant plant) { this.plant = plant; }
    public void Produce()
    {
        PersistentData.GetLevelData(LevelManager.currentLevelID).oxygenLevel += //this would be sufficient if they are increments...
            plant.oxygenProductionLevels[plant.plantData.currStageOfLife];
    }
}

public class ProduceFruit : IProduce
{
    ProductivePlant plant;
    IEnumerator p = null;

    public ProduceFruit(ProductivePlant plant) { this.plant = plant; }
    public void Produce()
    {
        plant.plantStageUpdateDelegate += StageUpdate; // subscribes resets cycle to the update. Unsubscribe when?

        p = ProduceOneFruit(ResetsCycle);
        plant.StartCoroutine(p); // resumes from previous fruitProduceTimeLeft.
    }

    private void ResetsCycle()
    {
        plant.plantData.fruitProduceTimeLeft = plant.secondsPerFruitProductionLevels[plant.plantData.currStageOfLife];

        Debug.Log("Fruit count ++!"); // Place holder, fruit process not sure yet.

        Produce();
    }

    private void StageUpdate() // affects coroutine implicitly
    {
        if (plant.plantData.fruitProduceTimeLeft > plant.secondsPerFruitProductionLevels[plant.plantData.currStageOfLife])
            plant.plantData.fruitProduceTimeLeft = plant.secondsPerFruitProductionLevels[plant.plantData.currStageOfLife];
    }

    IEnumerator ProduceOneFruit(Action callback)
    {
        yield return new WaitForSeconds(TimeManager.timeUnit * TimeManager.gameTimeScale);

        plant.plantData.fruitProduceTimeLeft -= 1;
        Debug.Log("Seconds left on fruit production: " + plant.plantData.fruitProduceTimeLeft);

        if (plant.plantData.fruitProduceTimeLeft <= 0)
        {
            callback(); // this shows how callback structure works.
        }
        else
        {
            p = ProduceOneFruit(callback);
            plant.StartCoroutine(p);
        }
    }
}

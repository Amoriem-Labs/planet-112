using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Interfaces. Can add more functions if preferred. 
public interface IProduce
{
    void Produce();
}

public interface IAttack
{
    void Attack();
}

public interface IDefend
{
    void Defend();
}

public interface ISupport 
{
    void Support();
}

public class ProduceOxygen : IProduce
{
    PlantScript plant;
    public ProduceOxygen(PlantScript plant) { this.plant = plant; }
    public void Produce()
    {
        PersistentData.GetLevelData(LevelManager.currentLevelID).oxygenLevel += //this would be sufficient if they are increments...
            plant.plantSO.oxygenProductionLevels[plant.plantData.currStageOfLife];
    }
}

public class ProduceFruit : IProduce
{
    PlantScript plant;
    IEnumerator p = null;

    public ProduceFruit(PlantScript plant) { this.plant = plant; }
    public void Produce()
    {
        plant.plantSO.plantStageUpdateDelegate += StageUpdate; // subscribes resets cycle to the update. Unsubscribe when?

        p = ProduceOneFruit(ResetsCycle);
        plant.StartCoroutine(p); // resumes from previous fruitProduceTimeLeft.
    }

    private void ResetsCycle()
    {
        plant.plantData.fruitProduceTimeLeft = plant.plantSO.secondsPerFruitProductionLevels[plant.plantData.currStageOfLife];

        Debug.Log("Fruit count ++!"); // Place holder, fruit process not sure yet.

        Produce();
    }

    private void StageUpdate() // affects coroutine implicitly
    {
        if (plant.plantData.fruitProduceTimeLeft > plant.plantSO.secondsPerFruitProductionLevels[plant.plantData.currStageOfLife])
            plant.plantData.fruitProduceTimeLeft = plant.plantSO.secondsPerFruitProductionLevels[plant.plantData.currStageOfLife];
    }

    IEnumerator ProduceOneFruit(Action callback)
    {
        yield return new WaitForSeconds(TimeManager.timeUnit * TimeManager.gameTimeScale);

        plant.plantData.fruitProduceTimeLeft -= 1;
        //Debug.Log("Seconds left on fruit production: " + plant.plantData.fruitProduceTimeLeft);

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

public abstract class PlantScript : MonoBehaviour // by being abstract, we can't create instances of this class.
{
    // [SerializeField] allows a private var to appear visible in the inspector
    // member variabels that define plant properties. To set by children.
    public int maxStage; // ex 3 stages. Phases: 0-1, 1-2, 2-3. 0-3 indices per stage, but 3 intervals with ind 0 1 2. 
    public Sprite[] spriteArray; // size = maxStage + 1
    public float[] stageTimeMax; // time until growth to next stage, size = maxStage
    public float maxHealth; // deafult HP of the plant (or an array too?)
    public PlantNames pName; // oxygen boost is not in here because we might want some oxygen-consuming plants even for game balancing ;D

    // private variables default to all plantscript object
    protected SpriteRenderer spriteRenderer; // our plants might use animations for idle instead of sprites, so a parameter from animator would replace.
    public PlantData plantData; // contains all the dynamic data of a plant to be saved, a reference to PD
    IEnumerator g = null; // coroutine obj that controls plant growth.

    // interfaces
    public List<IProduce> productionModules = new List<IProduce>();
    public List<IAttack> attackModules;
    public List<IDefend> defendModules;

    // delegates
    public delegate void OnPlantStageUpdateDelegate();
    public OnPlantStageUpdateDelegate plantStageUpdateDelegate;

    // Variables specific to productivePlant // currStageOfLife is the accessing index.
    public int[] oxygenProductionLevels; // note: plant stage starts at 0, the seed! // size = maxStage + 1
    public int[] secondsPerFruitProductionLevels; // size = maxStage + 1

    public void TryProduce()
    {
        foreach (IProduce produce in productionModules)
        {
            produce.Produce();
        }
    }

    public void TryAttack()
    {
        foreach (IAttack attack in attackModules)
        {
            attack.Attack();
        }
    }

    public void TryDefend()
    {
        foreach (IDefend defend in defendModules)
        {
            defend.Defend();
        }
    }

    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Call this to spawn a new plant instance
    public void SpawnNewPlant(int xIndex, int yIndex) // virtual allows it to be overridden
    {
        plantData = new PlantData();
        plantData.location = new Vector2(xIndex, yIndex);
        plantData.currStageOfLife = 0;
        plantData.plantName = (int)pName;
        plantData.stageTimeLeft = stageTimeMax[plantData.currStageOfLife];
        plantData.currentHealth = maxHealth;
        PersistentData.GetLevelData(LevelManager.currentLevelID).plantDatas.Add(plantData);

        VisualizePlant();
    }

    // Call this to spawn an existing plant instance
    public void SpawnExistingPlant(PlantData plantData)
    {
        this.plantData = plantData;

        VisualizePlant();
    }

    // This step is called after spawn new / existing plant is called, after data fill-ins
    private void VisualizePlant() // for now, assume spawn function is only used in the level where player's present
    {
        Vector2 plantPosition = plantData.location; // in the future do some math to convert from X Y indices to real world coords
        gameObject.transform.SetPositionAndRotation(plantPosition, Quaternion.identity);

        // update stats and visuals
        //UpdatePlantStats(plantData.currStageOfLife);
        spriteRenderer.sprite = spriteArray[plantData.currStageOfLife];

        if (plantData.currStageOfLife != maxStage) // if they are equal then no need to keep growing.
        {
            GrowPlant(PlantStageUpdate, stageTimeMax[plantData.currStageOfLife]);
        }
    }

    //public abstract void UpdatePlantStats(int currStage); // or use virtual, which only marks override. 
    // Could be override in child class, but this method is not needed atm. 

    // gonna do a test. Does stopping g stop the corotine? 
    private void GrowPlant(Action callback, float stageTime) // if want input parameters, do Action<type, type, ...>
    {
        plantData.stageTimeLeft = stageTime;
        g = StartPlantGrowth(callback);
        StartCoroutine(g);
    }

    // Coroutine script that takes in a function and executes that function at the end of the count.
    IEnumerator StartPlantGrowth(Action callback) // assume plant data's stage time left isn't 0 at start.
    {
        yield return new WaitForSeconds(TimeManager.timeUnit * TimeManager.gameTimeScale);

        plantData.stageTimeLeft -= 1;
        if (plantData.stageTimeLeft <= 0)
        {
            callback(); // this shows how callback structure works.
        }
        else
        {
            //Debug.Log("Current time left: " + plantData.stageTimeLeft);
            g = StartPlantGrowth(callback);
            StartCoroutine(g);
        }
        // can execute a call back every iteration if want, like current % plant growth etc for growth animation if want.
        // the action can return more info to the callback, as long as parameters match!
    }

    private void PlantStageUpdate()
    {
        plantData.currStageOfLife += 1;
        // update stats and visuals
        // trigger delegates so the subscribers will be notified. Want to reduce if statements and dependency!
        plantStageUpdateDelegate();
        //UpdatePlantStats(plantData.currStageOfLife);
        spriteRenderer.sprite = spriteArray[plantData.currStageOfLife];

        if (plantData.currStageOfLife == maxStage) //if maxStage = 3, then 0-1, 1-2, 2-3, but indices are 0 1 2 3.
        {
            // plant is fully grown; do something.
            Debug.Log("Plant is fully grown!");
        }
        else
        {
            // continues growing
            GrowPlant(PlantStageUpdate, stageTimeMax[plantData.currStageOfLife]); 
        }
    }

    public void StopPlantGrowth()
    {
        if (g != null) FindObjectOfType<TimeManager>().StopCoroutine(g);
    }

    // Player interaction.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerScript>().closePlants.Add(this);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerScript>().closePlants.Remove(this);
        }
    }
}

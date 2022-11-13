using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class PlantScript : MonoBehaviour // by being abstract, we can't create instances of this class.
{
    // [SerializeField] allows a private var to appear visible in the inspector
    // The scriptable oxject that contains fixed data about this plant.
    public Plant plantSO;

    // interfaces, they don't work in scriptable because you can't change them at run time... plus this is more per-plant dynamic here.
    public List<IProduce> productionModules = new List<IProduce>();
    public List<IAttack> attackModules = new List<IAttack>();
    public List<IDefend> defenseModules = new List<IDefend>();
    public List<ISupport> supportModules = new List<ISupport>();

    // this needs to be here, because each instance has its own sprite renderer
    protected SpriteRenderer spriteRenderer; // our plants might use animations for idle instead of sprites, so a parameter from animator would replace.

    // no need to hideininspector for now. Use for demo.
    /*[HideInInspector]*/ public PlantData plantData; // contains all the dynamic data of a plant to be saved, a reference to PD 
    IEnumerator g = null; // coroutine obj that controls plant growth.

    // Everytime the below function is called, the modules will get executed. Ideally each module only needs to be called once.
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
        foreach (IDefend defend in defenseModules)
        {
            defend.Defend();
        }
    }

    public void TrySupport() // can keep support function variables and stuff within child inheritance class, cuz unique.
    {
        foreach (ISupport support in supportModules)
        {
            support.Support();
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
        plantData.plantName = (int)plantSO.pName;
        plantData.stageTimeLeft = plantSO.stageTimeMax[plantData.currStageOfLife];
        plantData.currentHealth = plantSO.maxHealth[plantData.currStageOfLife];
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

        // update visuals
        spriteRenderer.sprite = plantSO.spriteArray[plantData.currStageOfLife];

        if (plantData.currStageOfLife != plantSO.maxStage) // if they are equal then no need to keep growing.
        {
            GrowPlant(PlantStageUpdate, plantSO.stageTimeMax[plantData.currStageOfLife]);
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
        if(plantSO.plantStageUpdateDelegate != null) plantSO.plantStageUpdateDelegate();
        // update visuals
        spriteRenderer.sprite = plantSO.spriteArray[plantData.currStageOfLife];
        // current health refreshes? either leave this line or delete
        plantData.currentHealth = plantSO.maxHealth[plantData.currStageOfLife]; 

        if (plantData.currStageOfLife == plantSO.maxStage) //if maxStage = 3, then 0-1, 1-2, 2-3, but indices are 0 1 2 3.
        {
            // plant is fully grown; do something.
            Debug.Log("Plant is fully grown!");
        }
        else
        {
            // continues growing
            GrowPlant(PlantStageUpdate, plantSO.stageTimeMax[plantData.currStageOfLife]); 
        }
    }

    public void StopPlantGrowth()
    {
        if (g != null) StopCoroutine(g);
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

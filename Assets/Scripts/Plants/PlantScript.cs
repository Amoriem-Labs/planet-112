using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This is an abstract class: we can't create instances of it, but other (non-abstract) classes can inherit from this. In general, you can have specific variables to child classes (which inherit from this class).
public abstract class PlantScript : MonoBehaviour
{
    // The scriptable oxject that contains fixed (non-dynamic) data about this plant.
    public Plant plantSO;
    
    // Plant module lists. They are separated by function. They are not in the scriptable object because that can't have runtime-changeable data.
    public List<IProduce> productionModules = new List<IProduce>();
    public List<IAttack> attackModules = new List<IAttack>();
    public List<IDefend> defenseModules = new List<IDefend>();
    public List<ISupport> supportModules = new List<ISupport>();

    // this needs to be here, because each instance has its own sprite renderer
    protected SpriteRenderer spriteRenderer; // our plants might use animations for idle instead of sprites, so a parameter from animator would replace.

    // no need to hideininspector for now. Use for demo.
    /*[HideInInspector]*/ public PlantData plantData; // contains all the dynamic data of a plant to be saved, a reference to PD 
    // TODO: name this something more descriptive
    IEnumerator g = null; // coroutine obj that controls plant growth.

    // Everytime the below function is called, the modules will get executed. Ideally each module only needs to be called once.
    public void RunProduceModules()
    {
        foreach (IProduce produce in productionModules)
        {
            produce.Produce();
        }
    }

    public void RunAttackModules()
    {
        foreach (IAttack attack in attackModules)
        {
            attack.Attack();
        }
    }

    public void RunDefendModules()
    {
        foreach (IDefend defend in defenseModules)
        {
            defend.Defend();
        }
    }

    public void RunSupportModules()
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
    
    public void InitializePlantData(int x, int y) {
        plantData = new PlantData();
        plantData.location = new Vector2(x, y);
        plantData.currStageOfLife = 0;
        plantData.plantName = (int)plantSO.pName;
        plantData.stageTimeLeft = plantSO.stageTimeMax[plantData.currStageOfLife];
        plantData.currentHealth = plantSO.maxHealth[plantData.currStageOfLife];
    }

    // This step is called after plant object has been initialized. This function places the plant in the world and schedules the first growth events.
    public void VisualizePlant() // for now, assume spawn function is only used in the level where player's present
    {
        Vector3 plantPosition = new Vector3(plantData.location.x, plantData.location.y, 0); // in the future do some math to convert from X Y indices to real world coords
        gameObject.transform.SetPositionAndRotation(plantPosition, Quaternion.identity);
        Debug.Log("Set position to: " + plantPosition);

        // Set sprite
        spriteRenderer.sprite = plantSO.spriteArray[plantData.currStageOfLife];

        if (plantData.currStageOfLife != plantSO.maxStage) // if they are equal then no need to keep growing.
        {
            // TODO: call this something different to indicate that growth doesn't happen immediately
            GrowPlant(PlantStageUpdate, plantSO.stageTimeMax[plantData.currStageOfLife]);
        }
    }

    // TODO: rewrite this coroutine stuff when implementing the time system

    // TODO: make an UpdatePlantStats function?
    //public abstract void UpdatePlantStats(int currStage); // or use virtual, which only marks override. 
    // Could be override in child class, but this method is not needed atm. 

    // gonna do a test. Does stopping g stop the coroutine? 
    // TODO does this need a callback argument? If all it does is call PlantStageUpdate
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

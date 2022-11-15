using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// This is an abstract class: we can't create instances of it, but other (non-abstract) classes can inherit from this. In general, you can have specific variables to child classes (which inherit from this class).
public abstract class PlantScript : MonoBehaviour
{
    // The scriptable oxject that contains fixed (non-dynamic) data about this plant.
    public Plant plantSO;
    
    // Plant module Dict. They are separated by function. They are not in the scriptable object because that can't have runtime-changeable data.
    protected Dictionary<PlantModules, IDoStuff> plantModules = new Dictionary<PlantModules, IDoStuff>();

    // this needs to be here, because each instance has its own sprite renderer
    protected SpriteRenderer spriteRenderer; // our plants might use animations for idle instead of sprites, so a parameter from animator would replace.

    // no need to hideininspector for now. Use for demo.
    /*[HideInInspector]*/ public PlantData plantData; // contains all the dynamic data of a plant to be saved, a reference to PD 
    // TODO: name this something more descriptive
    IEnumerator g = null; // coroutine obj that controls plant growth.

    // Everytime the below function is called, the commanded modules will get executed once. 
    public void RunPlantModules(List<PlantModules> commands) 
    {
        foreach (var command in commands)
        {
            plantModules[command].DoStuff();
        }
    }

    public void AddPlantModule(PlantModules module)
    {
        if (!plantModules.ContainsKey(module))
        { // do we want multiple modules? rework if so.
            plantModules.Add(module, PlantModuleArr.GetModule(module, this));
            plantData.plantModules.Add((int)module); // dynamic module tracking
        }
    }

    public void RemovePlantModule(PlantModules module)
    {
        if (plantModules.ContainsKey(module)) // do we want multiple modules? rework if so.
        {
            plantModules.Remove(module); // user's responsibility to pause the module? or pause it here. 
            plantData.plantModules.Remove((int)module);
        }
    }

    public IDoStuff GetPlantModule(PlantModules module)
    {
        return plantModules[module];
    }

    public virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void InitializePlantData(Vector2 location) {
        plantData = new PlantData();
        plantData.location = location;
        plantData.currStageOfLife = 0;
        plantData.plantName = (int)plantSO.pName;
        plantData.stageTimeLeft = plantSO.stageTimeMax[plantData.currStageOfLife];
        plantData.currentHealth = plantSO.maxHealth[plantData.currStageOfLife];
        plantData.plantModules = new List<int>(); // size 0. Modules to be added in the child class
        PersistentData.GetLevelData(LevelManager.currentLevelID).plantDatas.Add(plantData); // add this plant into save. 
    }

    // If a plant is new, no modules. if it exists, then load em in!
    public void SpawnInModules()
    {
        // no modules, fresh plant
        if (plantData.plantModules.Count == 0)
        {
            // add in the default modules!
            foreach (PlantModules plantModule in plantSO.defaultModules)
            {
                AddPlantModule(plantModule);
            }
        }
        else // has modules, spawn in previous plant
        {
            foreach (int plantModule in plantData.plantModules)
            {
                AddPlantModule((PlantModules)plantModule);
            }
        }
    }

    // This step is called after plant object has been initialized. This function places the plant in the world and schedules the first growth events.
    public void VisualizePlant() // for now, assume spawn function is only used in the level where player's present
    {   
        // Set sprite
        spriteRenderer.sprite = plantSO.spriteArray[plantData.currStageOfLife];

        if (plantData.currStageOfLife != plantSO.maxStage) // if they are equal then no need to keep growing.
        {
            // TODO: call this something different to indicate that growth doesn't happen immediately
            GrowPlant(PlantStageUpdate, plantSO.stageTimeMax[plantData.currStageOfLife]);
        }
    }

    // This is called upon plant destruction.
    public void OnPlantDeath()
    {
        // remove this plant from save
        PersistentData.GetLevelData(LevelManager.currentLevelID).plantDatas.Remove(plantData);
        // TODO: probably need to call module terminations. Be mindful that some modules are automatically terminated when the gameObject destructs.
        // remove the gameObject from scene. Make sure to check for null in other objects! (after destruction -> null, but might still be in other lists atm)
        Destroy(gameObject);
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

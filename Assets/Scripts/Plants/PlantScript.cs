using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using MEC;

// This is an abstract class: we can't create instances of it, but other (non-abstract) classes can inherit from this. In general, you can have specific variables to child classes (which inherit from this class).
public abstract class PlantScript : MonoBehaviour
{
    // The scriptable object that contains fixed (non-dynamic) data about this plant.
    public Plant plantSO;

    // Plant ID. Is used to track how much oxygen each plant is contributing to the level. ID = 0 means this is the first plant.
    public int ID;

    // Plant module Dict. They are separated by function. They are not in the scriptable object because that can't have runtime-changeable data.
    public Dictionary<PlantModuleEnum, IPlantModule> plantModules = new Dictionary<PlantModuleEnum, IPlantModule>();

    // this needs to be here, because each instance has its own sprite renderer
    protected SpriteRenderer spriteRenderer;
    protected Animator animator; // Due to time constraints, not every plant has an animation, so that's why we have both sprite renderers and animators
    protected Slider slider; // the UI slider element that controls health bar of plant

    // no need to hideininspector for now. Use for demo.
    /*[HideInInspector]*/
    public PlantData plantData; // contains all the dynamic data of a plant to be saved, a reference to PD 
    // TODO: name this something more descriptive
    CoroutineHandle g; // coroutine obj that controls plant growth.

    public int attackers = 0; // prob need to be dynamic from save data?
    public List<PestScript> pestScripts = new List<PestScript>(); // all the attackers attacking it
    public bool inMotion = false; // a boolean for whether the plant is in motion, aka being moved or moving.
    public bool pickedUp = false; // a boolean for whether this plant have been picked up by the player

    // Setter method. Make sure this is called whenever a plant's motion state is being set
    public void SetInMotion(bool inMotion)
    {
        this.inMotion = inMotion;

        if (inMotion)
        {
            foreach (PestScript pestScript in pestScripts)
            {
                pestScript.ChaseAfterPlant();
            }
        }
    }

    // Everytime the below function is called, the commanded modules will get executed once. 
    //public void RunPlantModules(List<PlantModuleEnum> commands)
    //{
    //    foreach (var command in commands)
    //    {
    //        plantModules[command].Update();
    //    }
    //}

    public void UpdateAllModules()
    {
        foreach (var module in plantModules.Values)
        {
            module.Update();
        }
    }

    public void AddPlantModule(PlantModuleEnum module, String dataString = null)
    {
        if (!plantModules.ContainsKey(module))
        { // do we want multiple modules? rework if so.
            var moduleInstance = PlantModuleArr.GetModule(module, this);
            if (dataString != null)
            {
                moduleInstance.AssignDataFromString(dataString);
            }
            else
            {
                dataString = moduleInstance.EncodeDataToString();
            }
            plantModules.Add(module, moduleInstance);
            plantData.plantModuleData.Add(module, dataString);
            moduleInstance.OnModuleAdd();
        }
    }

    public void RemovePlantModule(PlantModuleEnum module)
    {
        if (plantModules.ContainsKey(module)) // do we want multiple modules? rework if so.
        {
            plantModules[module].OnModuleRemove();
            plantModules.Remove(module); // user's responsibility to pause the module? or pause it here. 
            plantData.plantModuleData.Remove(module);
        }
    }

    public IPlantModule GetPlantModule(PlantModuleEnum module)
    {
        return plantModules[module];
    }

    public virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        slider = GetComponent<Slider>();
        slider.maxValue = plantSO.maxHealth[plantData.currStageOfLife];
        slider.value = slider.maxValue;
        transform.GetChild(0).gameObject.SetActive(false); // hide the health bar when instantiated since seeds aren't meant to be attacked by pests
    }

    public void InitializePlantData(Vector2 location)
    {
        plantData = new PlantData();
        plantData.location = location;
        plantData.currStageOfLife = 0;
        plantData.plantName = (int)plantSO.pName;
        plantData.stageTimeLeft = plantSO.stageTimeMax[plantData.currStageOfLife];
        plantData.currentHealth = plantSO.maxHealth[plantData.currStageOfLife];
        plantData.plantModuleData = new Dictionary<PlantModuleEnum, string>(); // size 0. Modules to be added in the child class
        PersistentData.GetLevelData(LevelManager.currentLevelID).plantDatas.Add(plantData); // add this plant into save. 
    }

    /* public void LoadPlantData(PlantData plantData){

    } */

    // If a plant is new, no modules. if it exists, then load em in!
    public void SpawnInModules()
    {
        // no modules, fresh plant
        if (plantData.plantModuleData.Count == 0)
        {
            // add in the default modules!
            foreach (PlantModuleEnum module in plantSO.defaultModules)
            {
                AddPlantModule(module);
            }
        }
        else // has modules, spawn in previous plant
        {
            foreach (PlantModuleEnum module in plantData.plantModuleData.Keys)
            {
                AddPlantModule(module, plantData.plantModuleData[module]);
            }
        }
    }

    // This step is called after plant object has been initialized. This function places the plant in the world and schedules the first growth events.
    public void VisualizePlant() // for now, assume spawn function is only used in the level where player's present
    {
        // Set sprite and animations
        spriteRenderer.sprite = plantSO.spriteArray[plantData.currStageOfLife];
        animator.runtimeAnimatorController = plantSO.animatorArray[plantData.currStageOfLife];

        if (plantData.currStageOfLife != plantSO.maxStage) // if they are equal then no need to keep growing.
        {
            // TODO: call this something different to indicate that growth doesn't happen immediately
            GrowPlant(PlantStageUpdate, plantSO.stageTimeMax[plantData.currStageOfLife]);
        }
    }

    public void SetMainCollider()
    {
        gameObject.GetComponent<BoxCollider2D>().offset = plantSO.hitboxOffset[plantData.currStageOfLife];
        gameObject.GetComponent<BoxCollider2D>().size = plantSO.hitboxSize[plantData.currStageOfLife];
    }

    public void GetHealed(float healAmt, HealMode healMode)
    {
        switch (healMode)
        {
            case HealMode.flat: // flat amount, same increment
                plantData.currentHealth = Math.Min(plantData.currentHealth + healAmt, plantSO.maxHealth[plantData.currStageOfLife]);
                break;
            case HealMode.max: // max hp % amt, same increment. Garen passive.
                float maxHealth = plantSO.maxHealth[plantData.currStageOfLife];
                plantData.currentHealth = Math.Min(plantData.currentHealth + healAmt * maxHealth, maxHealth);
                break;
            case HealMode.missing: // less hp, more healing; more hp, less healing. Set passive.
                float maxHP = plantSO.maxHealth[plantData.currStageOfLife];
                float missingHealth = maxHP - plantData.currentHealth;
                plantData.currentHealth = Math.Min(plantData.currentHealth + healAmt * missingHealth, maxHP);
                break;
            case HealMode.current: // more hp, more healing; less hp, less healing.
                plantData.currentHealth = Math.Min(plantData.currentHealth + healAmt * plantData.currentHealth, plantSO.maxHealth[plantData.currStageOfLife]);
                break;
        }
        slider.value = plantData.currentHealth;
    }

    // called by the attacker upon attacking this plant. Also, notice how taking negative damage HEALS the plant!
    public void TakeDamage(int damage)
    {
        plantData.currentHealth -= damage;
        slider.value = plantData.currentHealth;
        AudioManager.GetSFX("takeDamageSFX").Play();

        // check if plant dies.
        CheckPlantHealth();

        // StartCoroutine(CheckPlantHealthInTheEndOfFrame()); // Okay nvm this feels pointless
    } // PLEASE DON'T DELETE THIS. I do this to make sure that in the same frame, if you heal a <0 plant as it's attacked, it doesn't die.
    /*IEnumerator CheckPlantHealthInTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame(); // hopefully this phrase makes sense.

        CheckPlantHealth();
    }*/
    private void CheckPlantHealth() // only used here
    {
        // TODO: different behaviors / presentation based on different stages of health (by percentage)?
        if (plantData.currentHealth <= 0)
        {
            // sadly, plant dies.
            ////Debug.Log("PLANT KILLED GG");
            GameManager.KillPlant(this);

            // If this plant is a lilypad and there's other plants on the same square, also kill the other plants
            List<PlantScript> plantsOnTopThisSquare = GridScript.GetGridSquare(transform.position).plantsOnTop;
            if (plantSO.unlockPlantability && plantsOnTopThisSquare.Count > 1){
                foreach (PlantScript plantScript in plantsOnTopThisSquare){
                    if (plantScript != this) GameManager.KillPlant(plantScript);
                }
            }

            LevelManager.UpdateOxygenLevel(ID, 0);
        }
    }

    // This is called upon plant destruction.
    public void OnPlantDeath()
    {
        // remove this plant from save
        PersistentData.GetLevelData(LevelManager.currentLevelID).plantDatas.Remove(plantData);
        // TODO: probably need to call module terminations. Be mindful that some modules are automatically terminated when the gameObject destructs.
        foreach (var module in plantModules.Values)
        {
            // We don't need to actually remove them from the data, since the gameObject will be destroyed and the plantData removed from levelData anyway. But we do want to call the OnModuleRemove() functions in case that's important.
            module.OnModuleRemove();
        }
        // remove the gameObject from scene. Make sure to check for null in other objects! (after destruction -> null, but might still be in other lists atm)
        Destroy(gameObject);
    }

    // TODO: rewrite this coroutine stuff when implementing the time system

    // TODO: make an UpdatePlantStats function? No need atm.
    //public abstract void UpdatePlantStats(int currStage); // or use virtual, which only marks override. 
    // Could be override in child class, but this method is not needed atm. 

    // TODO does this need a callback argument? If all it does is call PlantStageUpdate. Hmm lemme think about it... flexibility and frame order maybe?
    private void GrowPlant(Action callback, float stageTime) // if want input parameters, do Action<type, type, ...>
    {
        plantData.stageTimeLeft = stageTime;
        g = Timing.RunCoroutine(StartPlantGrowth(callback).CancelWith(gameObject), "plant");
    }

    // Coroutine script that takes in a function and executes that function at the end of the count.
    IEnumerator<float> StartPlantGrowth(Action callback) // assume plant data's stage time left isn't 0 at start.
    {
        yield return Timing.WaitForSeconds(TimeManager.timeUnit * TimeManager.gameTimeScale);

        plantData.stageTimeLeft -= 1;
        if (plantData.stageTimeLeft <= 0)
        {
            callback(); // this shows how callback structure works.
        }
        else
        {
            // //Debug.Log("Current time left: " + plantData.stageTimeLeft);
            g = Timing.RunCoroutine(StartPlantGrowth(callback).CancelWith(gameObject), "plant");
        }
        // can execute a call back every iteration if want, like current % plant growth etc for growth animation if want.
        // the action can return more info to the callback, as long as parameters match!
    }

    private void PlantStageUpdate()
    {
        plantData.currStageOfLife += 1;

        // First check if there's space for all the new space needed at this next stage. 
        // we can do this knowing plantstageupdate will be called with currStage at least 1
        Vector2[] newSpaceNeeded = (plantSO.relativeGridsOccupied[plantData.currStageOfLife].vec2Array).Except(
            plantSO.relativeGridsOccupied[plantData.currStageOfLife - 1].vec2Array).ToArray();
        if (!GridScript.CheckOtherTilesAvailability(plantData.location, gameObject, newSpaceNeeded)) // if the spaces are not available, pause the growth.
        {
            // TODO: what's a way to resume the growth later on?
            ////Debug.Log("Plant growth is paused. Need more space.");
            plantData.currStageOfLife -= 1; // revert
            return;
        }

        // update stats and visuals
        // trigger delegates so the subscribers will be notified. Want to reduce if statements and dependency!
        if (plantSO.plantStageUpdateDelegate != null) plantSO.plantStageUpdateDelegate();

        // update visuals
        spriteRenderer.sprite = plantSO.spriteArray[plantData.currStageOfLife];
        animator.runtimeAnimatorController = plantSO.animatorArray[plantData.currStageOfLife];
        transform.position = GridScript.GridToCoordinates(plantData.location, plantSO.offset[plantData.currStageOfLife]);

        // update hitbox
        SetMainCollider();

        // current health refreshes? either leave this line or delete
        plantData.currentHealth = plantSO.maxHealth[plantData.currStageOfLife];
        // update modules' data accordingly
        foreach (var module in plantModules.Values) module.OnPlantStageGrowth();

        // update new tiles that needed to be occupied in the grid
        GridScript.SetTileStates(plantData.location, TileState.OCCUPIED_STATE, newSpaceNeeded);
        // a1.except(a2) is anything in a1 that's not in a2. We are basically finding the spaces freed up from prev just incase it shrinks
        Vector2[] freedUpSpaceFromPrev = (plantSO.relativeGridsOccupied[plantData.currStageOfLife - 1].vec2Array).Except(
            plantSO.relativeGridsOccupied[plantData.currStageOfLife].vec2Array).ToArray();
        if (freedUpSpaceFromPrev.Length > 0) GridScript.SetTileStates(plantData.location, TileState.AVAILABLE_STATE, freedUpSpaceFromPrev);

        // plays sound for when plant grows
        AudioManager.GetSFX("growingSFX").Play();

        // updates health bar for slider
        slider.maxValue = plantSO.maxHealth[plantData.currStageOfLife];
        if (plantData.currStageOfLife == 1){
            transform.GetChild(0).gameObject.SetActive(true); // make health bar visible when plant has grown up to stage 1
            slider.value = slider.maxValue;
        }

        if (plantData.currStageOfLife == plantSO.maxStage) //if maxStage = 3, then 0-1, 1-2, 2-3, but indices are 0 1 2 3.
        {
            // plant is fully grown; do something.
            ////Debug.Log("Plant is fully grown!");
        }
        else
        {
            // continues growing
            GrowPlant(PlantStageUpdate, plantSO.stageTimeMax[plantData.currStageOfLife]);
        }
    }

    public void StopPlantGrowth()
    {
        if (Timing.IsRunning(g)) Timing.KillCoroutines(g);
    }

    public void LiftPlant(Transform handTransform)
    {
        pickedUp = true;
        // pause modules accordingly
        foreach (var module in plantModules.Values) module.OnPlantGrowthPause();
        // Free up the space
        GridScript.RemoveObjectFromGrid(plantData.location, this,
            plantSO.relativeGridsOccupied[plantData.currStageOfLife].vec2Array);
        // Pause the growth
        StopPlantGrowth(); // aware of potential bug? like coroutine generated after pausing? do we need a bool + if in coroutine?
        // In motion now
        SetInMotion(true);
        // Remove it from plantDatas and put it onto plantInHand
        PersistentData.GetLevelData(LevelManager.currentLevelID).plantDatas.Remove(plantData);
        PersistentData.GetLevelData(LevelManager.currentLevelID).plantInHand = plantData;
        // Set parent (aka can play an animation here)
        transform.SetParent(handTransform, false);
        transform.localPosition = Vector3.zero;

        //Debug.Log("Plant has been lifted, and growth paused at " + plantData.stageTimeLeft + " seconds");
        //Debug.Log(GridScript.GetTileState(GridScript.CoordinatesToGrid(transform.position)));
    }

    public bool PlacePlant(Vector2 location)
    {
        // Check grid and place
        if (GridScript.PlaceObjectAtGrid(location, gameObject, plantSO.offset[plantData.currStageOfLife], plantSO.relativeGridsOccupied[plantData.currStageOfLife].vec2Array))
        {
            pickedUp = false;
            // resume modules accordingly
            foreach (var module in plantModules.Values) module.OnPlantGrowthResume();
            // No longer plantInHand, put back
            PersistentData.GetLevelData(LevelManager.currentLevelID).plantDatas.Add(plantData);
            PersistentData.GetLevelData(LevelManager.currentLevelID).plantInHand = null;
            plantData.location = location; // also update plantdata to this new loc
            // Resume the growth
            if (plantData.currStageOfLife < plantSO.maxStage) GrowPlant(PlantStageUpdate, plantData.stageTimeLeft);
            // Set the object back to the root of the scene
            transform.parent = null;
            // No longer in motion
            SetInMotion(false);

            return true;
        }

        return false;
    }

    // Returns the bottomLeft and topRight coord for the pest target loc rect. 
    public void VisualizePlantTargetBoundary()
    {
        var offset = plantSO.targetRectParameters[plantData.currStageOfLife].vec2Array[0];
        var dim = plantSO.targetRectParameters[plantData.currStageOfLife].vec2Array[1];
        var offsetBottomCenter = new Vector2(transform.position.x + offset.x, transform.position.y + offset.y);
        Vector2 bottomLeft = new Vector2(offsetBottomCenter.x - dim.x / 2, offsetBottomCenter.y),
            topRight = new Vector2(offsetBottomCenter.x + dim.x / 2, offsetBottomCenter.y + dim.y);
        Vector2 bottomRight = new Vector2(offsetBottomCenter.x + dim.x / 2, offsetBottomCenter.y),
            topLeft = new Vector2(offsetBottomCenter.x - dim.x / 2, offsetBottomCenter.y + dim.y);
        Debug.DrawLine(topLeft, topRight, Color.red, 0.5f, false);
        Debug.DrawLine(bottomLeft, bottomRight, Color.red, 0.5f, false);
        Debug.DrawLine(topLeft, bottomLeft, Color.red, 0.5f, false);
        Debug.DrawLine(topRight, bottomRight, Color.red, 0.5f, false);
        Debug.DrawLine(offsetBottomCenter, transform.position, Color.red, 0.5f, false);
    }

    public void Update()
    {
        // Modules shouldn't work when the plants is picked up? unless...
        if (pickedUp == false) UpdateAllModules();

        /* // If this plant is lilypad and there is another plant on top of lilypad and if pest aggro is on lilypad, switches the pest aggro to the plant on top of lilypad.
        List<PlantScript> plantsInGridSquare = GridScript.GetGridSquare(GridScript.CoordinatesToGrid(transform.position)).plantsOnTop;
        if (plantsInGridSquare.Count > 1){
            foreach (PestScript pestScript in pestScripts){
                foreach (PlantScript plantScript in plantsInGridSquare){
                    if (!plantScript.plantSO.unlockPlantability){
                        pestScript.switchTargetPlant(plantScript);
                    }
                }
            }
        } */
    }
}

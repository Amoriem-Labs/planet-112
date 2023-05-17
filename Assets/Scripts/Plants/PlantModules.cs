using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PlantModuleEnum // serialized names of each of the modules
{
    // Test,
    // InstaKillPests,
    FruitProduction,
    Healing,
    AoeDamage,
}

// Plant module interfaces (can be customized to include new functions)
// Plant modules define a single behavior of a plant type.
public interface IPlantModule
{
    void Update();
    void OnModuleAdd();
    void OnModuleRemove();
    void OnPlantStageGrowth();
    void OnPlantGrowthPause();
    void OnPlantGrowthResume();
    void AssignDataFromString(String dataString);
    String EncodeDataToString();
}

public static class PlantModuleArr
{
    static Dictionary<PlantModuleEnum, Func<PlantScript, IPlantModule>> moduleConstructors = new Dictionary<PlantModuleEnum, Func<PlantScript, IPlantModule>>
    {
      // {PlantModuleEnum.Test, (plantScript) => new TestModule(plantScript)},
      // {PlantModuleEnum.InstaKillPests, (plantScript) => new InstaKillPestsModule(plantScript)},
      {PlantModuleEnum.FruitProduction, (plantScript) => new FruitProductionModule(plantScript)},
      {PlantModuleEnum.Healing, (plantScript) => new HealingModule(plantScript)},
      {PlantModuleEnum.AoeDamage, (plantScript) => new AoeDamageModule(plantScript)},
    };

    // returns a new instance of the targetted plantModule 
    public static IPlantModule GetModule(PlantModuleEnum module, PlantScript plantScript)
    {
        return moduleConstructors[module].Invoke(plantScript);
    }

    #region ModuleStructureHierarchy
    // Modules
    public abstract class StatefulPlantModule<ModuleData> : IPlantModule
    {
        public ModuleData moduleData;
        protected PlantScript plantScript;
        public virtual String EncodeDataToString()
        {
            return JsonUtility.ToJson(moduleData);
        }
        public virtual void AssignDataFromString(String dataString)
        {
            moduleData = JsonUtility.FromJson<ModuleData>(dataString);
        }
        public virtual void Update() { }
        public virtual void OnModuleAdd() { }
        public virtual void OnModuleRemove() { }
        public virtual void OnPlantStageGrowth() { }
        public virtual void OnPlantGrowthPause() { }
        public virtual void OnPlantGrowthResume() { }

    }


    /// <summary>
    /// Main Inheritable Module #1: TimerModule
    /// </summary>
    [System.Serializable]
    public class TimerModuleData
    {
        public float timePerCycle;
        public float timeInCurrentCycleSoFar; // for time tracking and data storage
    }
    public class TimerModule<T> : StatefulPlantModule<T> where T : TimerModuleData
    {
        public TimerModule(PlantScript plantScript)
        {
            this.plantScript = plantScript;
        }

        float timeAnchor = 0f;

        public override void OnModuleAdd()
        {
            timeAnchor = Time.time; // controlled by timeScale
        }

        // Going to use update over coroutine for 1) centralization 2) no reason for module pausing.
        public override void Update()
        {
            float currTime = Time.time;
            float timeElapsed = currTime - timeAnchor;
            moduleData.timeInCurrentCycleSoFar += timeElapsed;
            timeAnchor = currTime;

            if (moduleData.timeInCurrentCycleSoFar >= moduleData.timePerCycle)
            {
                if (OnCycleCompleteEvent != null) OnCycleCompleteEvent?.Invoke(); // Trigger event
                else OnCycleComplete(); // Trigger child's overwrite
                moduleData.timeInCurrentCycleSoFar = 0f; // Resets the cycle timer.
            }
        }

        public event Action OnCycleCompleteEvent = null; // event version for compositive modules
        public virtual void OnCycleComplete() { } // This is an empty method in the parent class, but can be overridden in child classes.

        public virtual void OnGrowthPause()
        {
            moduleData.timeInCurrentCycleSoFar += Time.time - timeAnchor; // add the time so far
        }

        public virtual void OnGrowthResume() // Suppose this line is called before "update" happens at resume.
        {
            timeAnchor = Time.time; // reset the anchor
        }
    }

    /// <summary>
    /// Main Inheritable Module #2: TriggerModule
    /// </summary>
    [System.Serializable]
    public class TriggerModuleData
    {

    }
    public class TriggerModule<T> : StatefulPlantModule<T> where T : TriggerModuleData
    {
        public TriggerModule(PlantScript plantScript)
        {
            this.plantScript = plantScript;
        }

        public DynamicColliderScript colliderScript;

        public override void OnModuleAdd()
        {
            GameObject childObject = new GameObject();
            childObject.transform.SetParent(plantScript.gameObject.transform);
            childObject.transform.localPosition = Vector2.zero;
            childObject.layer = LayerMask.NameToLayer("Detectors"); // no matter; trigger detectors won't trigger each other.
            colliderScript = childObject.AddComponent<DynamicColliderScript>();
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            Debug.Log("OnTriggerEnter2D called for TriggerModule. gameObject: " + collider.gameObject.name);
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            Debug.Log("OnTriggerExit2D called for TriggerModule. gameObject: " + collider.gameObject.name);
        }
    }

    /// <summary>
    /// Main Inheritable Module #3: TriggerAndTimerModule (created via composition of the previous 2)
    /// </summary>
    [System.Serializable]
    public class TriggerAndTimerModuleData
    {
        public TriggerModuleData triggerData;
        public TimerModuleData timerData;
    }
    public class TriggerAndTimerModule<T> : StatefulPlantModule<T> where T : TriggerAndTimerModuleData
    {
        protected TriggerModule<TriggerModuleData> triggerModule;
        protected TimerModule<TimerModuleData> timerModule;

        public TriggerAndTimerModule(PlantScript plantScript)
        {
            this.plantScript = plantScript;

            // Instantiate the helper modules and assign their properties
            triggerModule = new TriggerModule<TriggerModuleData>(plantScript);
            timerModule = new TimerModule<TimerModuleData>(plantScript);
            timerModule.OnCycleCompleteEvent += OnCycleComplete; // Subscribe to event
        }

        protected virtual void OnCycleComplete() { }

        public override void OnModuleAdd()
        {
            triggerModule.OnModuleAdd();
            timerModule.OnModuleAdd();
        }

        public override void Update()
        {
            triggerModule.Update();
            timerModule.Update();
        }

        // Add other override methods as needed, and delegate to the appropriate module
    }

    #endregion

    //////////////////////////////////// ACTUAL MODULE IMPLEMENTATIONS BEGINS HERE ///////////////////////////////////////
    [System.Serializable]
    public class FruitProductionModuleData : TimerModuleData
    {
        public int productionQuantity; 
        public FruitType fruitType; 
    }
    public class FruitProductionModule : TimerModule<FruitProductionModuleData>
    {
        public FruitProductionModule(PlantScript plantScript) : base(plantScript)
        {
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new FruitProductionModuleData
            {
                timePerCycle = plantScript.plantSO.productionRate[plantScript.plantData.currStageOfLife], // productionRate
                productionQuantity = plantScript.plantSO.productionQuantity[plantScript.plantData.currStageOfLife],
                fruitType = plantScript.plantSO.fruitType,
                timeInCurrentCycleSoFar = 0f
            };
        }

        public override void Update()
        {
            base.Update();
            // ... other update code
        }

        public override void OnCycleComplete()
        {
            // TODO: actual fruit production (visual + systemic)
            Debug.Log("Producing " + moduleData.productionQuantity + " of type " + moduleData.fruitType.ToString() + " fruit.");
        }

        public override void OnPlantStageGrowth()
        {
            // by now, stage should be inc'ed alrdy
            moduleData.timePerCycle = plantScript.plantSO.productionRate[plantScript.plantData.currStageOfLife];
            moduleData.productionQuantity = plantScript.plantSO.productionQuantity[plantScript.plantData.currStageOfLife];
        }
    }


    [System.Serializable]
    public class HealingModuleData : TriggerAndTimerModuleData
    {
        public float healAmount;
        public HealMode healMode;
        public float healRangeRadius; 
    }
    public class HealingModule : TriggerAndTimerModule<HealingModuleData>
    {
        public HealingModule(PlantScript plantScript) : base(plantScript)
        {
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new HealingModuleData
            {
                healAmount = plantScript.plantSO.healAmount[plantScript.plantData.currStageOfLife],
                healMode = plantScript.plantSO.healMode[plantScript.plantData.currStageOfLife],
                healRangeRadius = plantScript.plantSO.healRangeRadius[plantScript.plantData.currStageOfLife],
                timerData = new TimerModuleData
                {
                    timePerCycle = plantScript.plantSO.healRate[plantScript.plantData.currStageOfLife], // healRate
                    timeInCurrentCycleSoFar = 0f
                },
                triggerData = new TriggerModuleData()
            };
            // Don't forget to grant each module the correct datas to reference
            timerModule.moduleData = moduleData.timerData;
            triggerModule.moduleData = moduleData.triggerData;
        }

        public override void OnModuleAdd()
        {
            base.OnModuleAdd();
            triggerModule.colliderScript.gameObject.name = "HealingRange";
            triggerModule.colliderScript.SetCollider(typeof(CircleCollider2D), new Vector2(0, 0), new Vector2(), moduleData.healRangeRadius,
                OnTriggerEnter2D, OnTriggerExit2D);
        }

        List<PlantScript> plantsInRange = new List<PlantScript>();
        protected override void OnCycleComplete()
        {
            for (int i = 0; i < plantsInRange.Count; i++)
            {
                if (plantsInRange[i] == null) // potentially destroyed already
                {
                    plantsInRange.RemoveAt(i);
                    i--;
                }
                else // heal the plant.
                {
                    plantsInRange[i].GetHealed(moduleData.healAmount, moduleData.healMode);
                }
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("plant"))
            {
                plantsInRange.Add(collider.gameObject.GetComponent<PlantScript>());
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("plant"))
            {
                plantsInRange.Remove(collider.gameObject.GetComponent<PlantScript>());
            }
        }

        public override void OnPlantStageGrowth()
        {
            // by now, stage should be inc'ed alrdy
            moduleData.healAmount = plantScript.plantSO.healAmount[plantScript.plantData.currStageOfLife];
            moduleData.healMode = plantScript.plantSO.healMode[plantScript.plantData.currStageOfLife];
            moduleData.healRangeRadius = plantScript.plantSO.healRangeRadius[plantScript.plantData.currStageOfLife];
            moduleData.timerData.timePerCycle = plantScript.plantSO.healRate[plantScript.plantData.currStageOfLife];
        }
    }


    [System.Serializable]
    public class AoeDamageModuleData : TriggerAndTimerModuleData
    {
        public float damageAmount;
        public float damageRangeRadius;
    }
    public class AoeDamageModule : TriggerAndTimerModule<AoeDamageModuleData>
    {
        public AoeDamageModule(PlantScript plantScript) : base(plantScript)
        {
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new AoeDamageModuleData
            {
                damageAmount = plantScript.plantSO.aoeDamageAmount[plantScript.plantData.currStageOfLife],
                damageRangeRadius = plantScript.plantSO.aoeDamageRangeRadius[plantScript.plantData.currStageOfLife],
                timerData = new TimerModuleData
                {
                    timePerCycle = plantScript.plantSO.aoeAttackRate[plantScript.plantData.currStageOfLife], // attackRate
                    timeInCurrentCycleSoFar = 0f
                },
                triggerData = new TriggerModuleData()
            };
            // Don't forget to grant each module the correct datas to reference
            timerModule.moduleData = moduleData.timerData;
            triggerModule.moduleData = moduleData.triggerData;
        }

        public override void OnModuleAdd()
        {
            base.OnModuleAdd();
            triggerModule.colliderScript.gameObject.name = "AoeDamageRange";
            triggerModule.colliderScript.SetCollider(typeof(CircleCollider2D), new Vector2(0, 0), new Vector2(), moduleData.damageRangeRadius,
                OnTriggerEnter2D, OnTriggerExit2D);
        }

        List<PestScript> pestsInRange = new List<PestScript>();
        protected override void OnCycleComplete()
        {
            for (int i = 0; i < pestsInRange.Count; i++)
            {
                if (pestsInRange[i] == null) // potentially destroyed already
                {
                    pestsInRange.RemoveAt(i);
                    i--;
                }
                else // damage the pests. (or launch proj etc etc)
                {
                    Debug.Log("Attacking pest " + pestsInRange[i].name);
                }
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("pest"))
            {
                pestsInRange.Add(collider.gameObject.GetComponent<PestScript>());
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("pest"))
            {
                pestsInRange.Remove(collider.gameObject.GetComponent<PestScript>());
            }
        }

        public override void OnPlantStageGrowth()
        {
            // by now, stage should be inc'ed alrdy
            moduleData.damageAmount = plantScript.plantSO.aoeDamageAmount[plantScript.plantData.currStageOfLife];
            moduleData.damageRangeRadius = plantScript.plantSO.aoeDamageRangeRadius[plantScript.plantData.currStageOfLife];
            moduleData.timerData.timePerCycle = plantScript.plantSO.aoeAttackRate[plantScript.plantData.currStageOfLife];
        }
    }

    /*
    [System.Serializable]
    public class TestModuleData
    {
        public string name;
        public int age;
        public string job;
    }
    public class TestModule : StatefulPlantModule<TestModuleData>
    {
        public TestModule(PlantScript plantScript)
        {
            this.plantScript = plantScript;
            moduleData = new TestModuleData
            {
                name = "default",
                age = 0,
                job = "tesla engineer",
            };
        }

        public override void Update()
        {
            //Debug.Log("Test Module output: the plant's time left is " + plantScript.plantData.stageTimeLeft);
            Debug.Log("TEST MODULE UPDATE: " + EncodeDataToString() + moduleData.name);
        }

        public override void OnModuleAdd()
        {
            Debug.Log("OnModuleAdd was called for a TestModule");
        }

        public override void OnModuleRemove()
        {
            Debug.Log("OnModuleRemove was called for a TestModule");
        }
    }


    [System.Serializable]
    public class TriggerModuleData
    {

    }
    public class TriggerModule : StatefulPlantModule<TriggerModuleData>
    {
        DynamicColliderScript colliderScript;
        public TriggerModule(PlantScript plantScript)
        {
            this.plantScript = plantScript;
        }

        public override void OnModuleAdd()
        {
            GameObject childObject = new GameObject();
            childObject.transform.SetParent(plantScript.gameObject.transform);
            childObject.transform.localPosition = Vector2.zero;
            colliderScript = childObject.AddComponent<DynamicColliderScript>();
            colliderScript.SetCollider(typeof(BoxCollider2D), new Vector2(0, 1), new Vector2(1, 1), 0,
                OnTriggerEnter2D, OnTriggerExit2D);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            Debug.Log("OnTriggerEnter2D called for InstaKillPestsModule. gameObject: " + collider.gameObject.name);
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            Debug.Log("OnTriggerExit2D called for InstaKillPestsModule. gameObject: " + collider.gameObject.name);
        }
    }

    public class InstaKillPestsModule : TriggerModule
    {
        public InstaKillPestsModule(PlantScript plantScript) : base(plantScript)
        { }

        protected override void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("pest"))
            {
                collider.gameObject.GetComponent<PestScript>().OnDeath();
            }

        }
    }*/
}
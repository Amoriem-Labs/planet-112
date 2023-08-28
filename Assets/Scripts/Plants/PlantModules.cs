using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random=UnityEngine.Random;

public enum PlantModuleEnum // serialized names of each of the modules
{
    // Test,
    // InstaKillPests,
    FruitProduction,
    OxygenProduction,
    Healing,
    AoeDamage,
    FruitProductionBoost,
    OxygenProductionBoost,
    Taunt,
    SelfHealing,
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
      {PlantModuleEnum.OxygenProduction, (plantScript) => new OxygenProductionModule(plantScript)},
      {PlantModuleEnum.SelfHealing, (plantScript) => new HealSelfModule(plantScript)},
      {PlantModuleEnum.Healing, (plantScript) => new HealingModule(plantScript)},
      {PlantModuleEnum.AoeDamage, (plantScript) => new AoeDamageModule(plantScript)},
      {PlantModuleEnum.FruitProductionBoost, (plantScript) => new FruitProductionBoostModule(plantScript)},
      {PlantModuleEnum.OxygenProductionBoost, (plantScript) => new OxygenProductionBoostModule(plantScript)},
      {PlantModuleEnum.Taunt, (plantScript) => new TauntModule(plantScript)},
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

        public TriggerResponse colliderScript;

        public override void OnModuleAdd()
        {
            /*GameObject childObject = new GameObject();
            childObject.transform.SetParent(plantScript.gameObject.transform);
            childObject.transform.localPosition = Vector2.zero;
            childObject.layer = LayerMask.NameToLayer("Detectors"); // no matter; trigger detectors won't trigger each other.
            colliderScript = childObject.AddComponent<DynamicColliderScript>();*/
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
        public float fruitProductionRate;
        public int fruitProductionQuantity; 
        public FruitType fruitType; 
    }
    public class FruitProductionModule : TimerModule<FruitProductionModuleData>
    {
        public FruitProductionModule(PlantScript plantScript) : base(plantScript)
        {
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new FruitProductionModuleData
            {
                timePerCycle = plantScript.plantSO.fruitProductionRate[plantScript.plantData.currStageOfLife], // fruitProductionRate
                timeInCurrentCycleSoFar = 0f,
                fruitProductionQuantity = plantScript.plantSO.fruitProductionQuantity[plantScript.plantData.currStageOfLife],
                fruitType = plantScript.plantSO.fruitType,
            };
        }

        public override void OnModuleAdd()
        {
            base.OnModuleAdd();
        }

        public override void OnCycleComplete()
        {
            for (int i = 0; i < moduleData.fruitProductionQuantity; i++){
                float velocityMag = 3.0f;
                float xComp = Random.Range(-1,1);
                float yComp = (float)Math.Sqrt((velocityMag)*(velocityMag) - (xComp)*(xComp));
                Vector2 randomVelocity = new Vector2(xComp, yComp);
                UtilPrefabStorage.Instance.InstantiatePrefab(FruitManager.GetFruitPrefab(moduleData.fruitType), 
                    plantScript.transform.position, Quaternion.identity, null, randomVelocity);
            }
            Debug.Log("Producing " + moduleData.fruitProductionQuantity + " of type " + moduleData.fruitType.ToString() + " fruit.");

            // Random chance of producing 1 crystalline icura based on what type of fruit this plant mainly produces
            float randfloat = Random.Range(0,1);
            Debug.Log($"randfloat: {randfloat}, fruitType: {moduleData.fruitType}");
            if (moduleData.fruitType == FruitType.Seafoam && randfloat < 0.1f){
                float velocityMag = 3.0f;
                float xComp = Random.Range(-1,1);
                float yComp = (float)Math.Sqrt((velocityMag)*(velocityMag) - (xComp)*(xComp));
                Vector2 randomVelocity = new Vector2(xComp, yComp);
                UtilPrefabStorage.Instance.InstantiatePrefab(FruitManager.GetFruitPrefab(FruitType.Crystalline), 
                    plantScript.transform.position, Quaternion.identity, null, randomVelocity);
                Debug.Log("Producing 1 of type Crystalline fruit.");
            }
            if (moduleData.fruitType == FruitType.Sunset && randfloat < 0.2f){
                float velocityMag = 3.0f;
                float xComp = Random.Range(-1,1);
                float yComp = (float)Math.Sqrt((velocityMag)*(velocityMag) - (xComp)*(xComp));
                Vector2 randomVelocity = new Vector2(xComp, yComp);
                UtilPrefabStorage.Instance.InstantiatePrefab(FruitManager.GetFruitPrefab(FruitType.Crystalline), 
                    plantScript.transform.position, Quaternion.identity, null, randomVelocity);
                Debug.Log("Producing 1 of type Crystalline fruit.");
            }
            if (moduleData.fruitType == FruitType.Amethyst && randfloat < 0.3f){
                float velocityMag = 3.0f;
                float xComp = Random.Range(-1,1);
                float yComp = (float)Math.Sqrt((velocityMag)*(velocityMag) - (xComp)*(xComp));
                Vector2 randomVelocity = new Vector2(xComp, yComp);
                UtilPrefabStorage.Instance.InstantiatePrefab(FruitManager.GetFruitPrefab(FruitType.Crystalline), 
                    plantScript.transform.position, Quaternion.identity, null, randomVelocity);
                Debug.Log("Producing 1 of type Crystalline fruit.");
            }
        }

        public override void OnPlantStageGrowth()
        {
            // by now, stage should be inc'ed alrdy
            moduleData.timePerCycle = plantScript.plantSO.fruitProductionRate[plantScript.plantData.currStageOfLife];
            moduleData.fruitProductionQuantity = plantScript.plantSO.fruitProductionQuantity[plantScript.plantData.currStageOfLife];
        }
    }

    [System.Serializable]
    public class OxygenProductionModuleData : TimerModuleData
    {
        public int oxygenProductionQuantity; 
    }
    public class OxygenProductionModule : TimerModule<OxygenProductionModuleData>
    {
        public OxygenProductionModule(PlantScript plantScript) : base(plantScript)
        {
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new OxygenProductionModuleData
            {
                timeInCurrentCycleSoFar = 0f,
                oxygenProductionQuantity = plantScript.plantSO.oxygenProductionQuantity[plantScript.plantData.currStageOfLife],
            };
        }

        public override void OnModuleAdd()
        {
            base.OnModuleAdd();
        }

        public override void OnCycleComplete()
        {
            LevelManager.UpdateOxygenLevel(plantScript.ID, moduleData.oxygenProductionQuantity);
        }

        public override void OnPlantStageGrowth()
        {
            // by now, stage should be inc'ed alrdy
            moduleData.oxygenProductionQuantity = plantScript.plantSO.oxygenProductionQuantity[plantScript.plantData.currStageOfLife];
        }
    }

    // Heal self module
    [System.Serializable]
    public class HealSelfModuleData : TimerModuleData
    {
        public float healSelfRate;
        public float healSelfAmount; 
    }
    public class HealSelfModule : TimerModule<HealSelfModuleData>
    {
        public HealSelfModule(PlantScript plantScript) : base(plantScript)
        {
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new HealSelfModuleData
            {
                timeInCurrentCycleSoFar = 0f,
                timePerCycle = plantScript.plantSO.healSelfRate[plantScript.plantData.currStageOfLife], // healSelfRate
                healSelfAmount = plantScript.plantSO.healSelfAmount[plantScript.plantData.currStageOfLife], // healSelfAmount
            };
        }

        public override void OnModuleAdd()
        {
            base.OnModuleAdd();
        }

        public override void OnCycleComplete()
        {
            if (plantScript.plantData.currentHealth + moduleData.healSelfAmount > plantScript.plantSO.maxHealth[plantScript.plantData.currStageOfLife]){
                plantScript.plantData.currentHealth = plantScript.plantSO.maxHealth[plantScript.plantData.currStageOfLife];
            } else {
                plantScript.plantData.currentHealth += moduleData.healSelfAmount;
            }
            AudioManager.GetSFX("healSFX").Play();
        }

        public override void OnPlantStageGrowth()
        {
            // by now, stage should be inc'ed alrdy
            moduleData.healSelfRate = plantScript.plantSO.healSelfRate[plantScript.plantData.currStageOfLife];
            moduleData.healSelfAmount = plantScript.plantSO.healSelfAmount[plantScript.plantData.currStageOfLife];
        }
    }



    // Heal other plants module
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
            triggerModule.colliderScript = UtilPrefabStorage.Instance.InstantiatePrefab(UtilPrefabStorage.Instance.circleDetector, 
                Vector2.zero, Quaternion.identity, plantScript.gameObject.transform).GetComponent<TriggerResponse>();
            triggerModule.colliderScript.gameObject.transform.localPosition = Vector2.zero;
            triggerModule.colliderScript.gameObject.name = "HealingRange";
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().radius = moduleData.healRangeRadius;
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().offset = Vector2.zero;
            triggerModule.colliderScript.onTriggerEnter2D = OnTriggerEnter2D;
            triggerModule.colliderScript.onTriggerExit2D = OnTriggerExit2D;
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
            AudioManager.GetSFX("healSFX").Play();
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
            // update the collider
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().radius = moduleData.healRangeRadius;
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().offset = Vector2.zero;
        }
    }


    [System.Serializable]
    public class AoeDamageModuleData : TriggerAndTimerModuleData
    {
        public float damageAmount;
        public float damageRangeRadius;
        public int maxPestsTargetable;
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
                maxPestsTargetable = plantScript.plantSO.aoeMaxPestsTargetable[plantScript.plantData.currStageOfLife],
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
            triggerModule.colliderScript = UtilPrefabStorage.Instance.InstantiatePrefab(UtilPrefabStorage.Instance.circleDetector,
                Vector2.zero, Quaternion.identity, plantScript.gameObject.transform).GetComponent<TriggerResponse>();
            triggerModule.colliderScript.gameObject.transform.localPosition = Vector2.zero;
            triggerModule.colliderScript.gameObject.name = "AoeDamageRange";
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().radius = moduleData.damageRangeRadius;
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().offset = Vector2.zero;
            triggerModule.colliderScript.onTriggerEnter2D = OnTriggerEnter2D;
            triggerModule.colliderScript.onTriggerExit2D = OnTriggerExit2D;
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
                    if (i < moduleData.maxPestsTargetable){
                        pestsInRange[i].TakeDamage(moduleData.damageAmount);
                        Debug.Log("Attacking pest " + pestsInRange[i].name);
                    }
                    AudioManager.GetSFX("chompSFX").Play();
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
            moduleData.maxPestsTargetable = plantScript.plantSO.aoeMaxPestsTargetable[plantScript.plantData.currStageOfLife];
            moduleData.timerData.timePerCycle = plantScript.plantSO.aoeAttackRate[plantScript.plantData.currStageOfLife];
            // update the collider
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().radius = moduleData.damageRangeRadius;
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().offset = Vector2.zero;
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

    [System.Serializable]
    public class TauntModuleData : TriggerAndTimerModuleData
    {
        public float tauntRangeRadius;
        // dont forget to activate TauntModule
    }
    public class TauntModule : TriggerAndTimerModule<TauntModuleData>
    {
        /*
        Taunt module is for peach tree to attract insect aggros to itself. Insects prioritize attacking the peach tree over other plants in a specific radius tauntRangeRadius. 
        */

        public TauntModule(PlantScript plantScript) : base(plantScript)
        {
            moduleData = new TauntModuleData
            {
                tauntRangeRadius = plantScript.plantSO.tauntRangeRadius[plantScript.plantData.currStageOfLife],
                triggerData = new TriggerModuleData(),
                timerData = new TimerModuleData
                {
                    timeInCurrentCycleSoFar = 0f
                },
            };
            // Don't forget to grant each module the correct datas to reference
            timerModule.moduleData = moduleData.timerData;
            triggerModule.moduleData = moduleData.triggerData;
        }

        public override void OnModuleAdd()
        {
            base.OnModuleAdd();
            triggerModule.colliderScript = UtilPrefabStorage.Instance.InstantiatePrefab(UtilPrefabStorage.Instance.circleDetector,
                Vector2.zero, Quaternion.identity, plantScript.gameObject.transform).GetComponent<TriggerResponse>();
            triggerModule.colliderScript.gameObject.transform.localPosition = Vector2.zero;
            triggerModule.colliderScript.gameObject.name = "TauntRange";
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().radius = moduleData.tauntRangeRadius;
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().offset = Vector2.zero;
            triggerModule.colliderScript.onTriggerEnter2D = OnTriggerEnter2D;
            triggerModule.colliderScript.onTriggerExit2D = OnTriggerExit2D;
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
                    pestsInRange[i].switchTargetPlant(plantScript);
                    Debug.Log("Taunting pest " + pestsInRange[i].name);
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
            moduleData.tauntRangeRadius = plantScript.plantSO.tauntRangeRadius[plantScript.plantData.currStageOfLife];
            // update the collider
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().radius = moduleData.tauntRangeRadius;
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().offset = Vector2.zero;
        }
    }

    [System.Serializable]
    public class FruitProductionBoostModuleData : TriggerAndTimerModuleData
    {
        public float fruitProductionBoostDecimal;
        public float fruitProductionBoostRangeRadius;
    }
    public class FruitProductionBoostModule : TriggerAndTimerModule<FruitProductionBoostModuleData>
    {
        // increase a plant's fruit generated per production by % fruitProductionBoost
        public FruitProductionBoostModule(PlantScript plantScript) : base(plantScript)
        {
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new FruitProductionBoostModuleData
            {
                fruitProductionBoostDecimal = plantScript.plantSO.fruitProductionBoostDecimal[plantScript.plantData.currStageOfLife],
                fruitProductionBoostRangeRadius = plantScript.plantSO.fruitProductionBoostRangeRadius[plantScript.plantData.currStageOfLife],
                timerData = new TimerModuleData
                {
                    timeInCurrentCycleSoFar = 0f,
                },
                triggerData = new TriggerModuleData(),
            };
            timerModule.moduleData = moduleData.timerData;
            triggerModule.moduleData = moduleData.triggerData;
        }

        public override void OnModuleAdd()
        {
            base.OnModuleAdd();
            triggerModule.colliderScript = UtilPrefabStorage.Instance.InstantiatePrefab(UtilPrefabStorage.Instance.circleDetector,
                Vector2.zero, Quaternion.identity, plantScript.gameObject.transform).GetComponent<TriggerResponse>();
            triggerModule.colliderScript.gameObject.transform.localPosition = Vector2.zero;
            triggerModule.colliderScript.gameObject.name = "FruitProductionBoostRange";
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().radius = moduleData.fruitProductionBoostRangeRadius;
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().offset = Vector2.zero;
            triggerModule.colliderScript.onTriggerEnter2D = OnTriggerEnter2D;
            triggerModule.colliderScript.onTriggerExit2D = OnTriggerExit2D;
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
                else // boost fruit production for nearby plants
                {
                    if (plantsInRange[i].plantModules.ContainsKey(PlantModuleEnum.FruitProduction)){
                        FruitProductionModule fruitProductionModule = (FruitProductionModule)plantsInRange[i].plantModules[PlantModuleEnum.FruitProduction];
                        fruitProductionModule.moduleData.fruitProductionQuantity = (int)Math.Ceiling(plantsInRange[i].plantSO.fruitProductionQuantity[plantsInRange[i].plantData.currStageOfLife] * moduleData.fruitProductionBoostDecimal);
                        plantsInRange[i].plantModules[PlantModuleEnum.FruitProduction] = fruitProductionModule;
                    }
                }
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("plant"))
            {
                plantsInRange.Add(collider.gameObject.GetComponent<PlantScript>());
                Debug.Log("Boosting fruit production for plant " + collider.gameObject.GetComponent<PlantScript>().name);
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.CompareTag("plant"))
            {
                // Decrease fruit production boost for plants that are now out of range
                PlantScript exitingPlantScript = collider.gameObject.GetComponent<PlantScript>();
                if (exitingPlantScript.plantModules.ContainsKey(PlantModuleEnum.FruitProduction)){
                    FruitProductionModule fruitProductionModule = (FruitProductionModule)exitingPlantScript.plantModules[PlantModuleEnum.FruitProduction];
                    fruitProductionModule.moduleData.fruitProductionQuantity = (int)Math.Floor(fruitProductionModule.moduleData.fruitProductionQuantity / moduleData.fruitProductionBoostDecimal);
                    exitingPlantScript.plantModules[PlantModuleEnum.FruitProduction] = fruitProductionModule;
                }
                Debug.Log("Deboosting fruit production for plant " + exitingPlantScript.name);
                plantsInRange.Remove(exitingPlantScript);
            }
        }

        // find a way to increase production quantity of nearby plants until dead

        public override void OnPlantStageGrowth()
        {
            // by now, stage should be inc'ed alrdy
            moduleData.fruitProductionBoostDecimal = plantScript.plantSO.fruitProductionBoostDecimal[plantScript.plantData.currStageOfLife];
            moduleData.fruitProductionBoostRangeRadius = plantScript.plantSO.fruitProductionBoostRangeRadius[plantScript.plantData.currStageOfLife];
            // update the collider
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().radius = moduleData.fruitProductionBoostRangeRadius;
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().offset = Vector2.zero;
        }
    }

    [System.Serializable]
    public class OxygenProductionBoostModuleData : TriggerAndTimerModuleData
    {
        public float oxygenProductionBoostDecimal;
        public float oxygenProductionBoostRangeRadius;
    }
    public class OxygenProductionBoostModule : TriggerAndTimerModule<OxygenProductionBoostModuleData>
    {
        // increase a plant's fruit generated per production by % oxygenProductionBoost
        public OxygenProductionBoostModule(PlantScript plantScript) : base(plantScript)
        {
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new OxygenProductionBoostModuleData
            {
                oxygenProductionBoostDecimal = plantScript.plantSO.oxygenProductionBoostDecimal[plantScript.plantData.currStageOfLife],
                oxygenProductionBoostRangeRadius = plantScript.plantSO.oxygenProductionBoostRangeRadius[plantScript.plantData.currStageOfLife],
                triggerData = new TriggerModuleData(),
                timerData = new TimerModuleData
                {
                    timeInCurrentCycleSoFar = 0f,
                },
            };
            timerModule.moduleData = moduleData.timerData;
            triggerModule.moduleData = moduleData.triggerData;
        }

        public override void OnModuleAdd()
        {
            base.OnModuleAdd();
            triggerModule.colliderScript = UtilPrefabStorage.Instance.InstantiatePrefab(UtilPrefabStorage.Instance.circleDetector,
                Vector2.zero, Quaternion.identity, plantScript.gameObject.transform).GetComponent<TriggerResponse>();
            triggerModule.colliderScript.gameObject.transform.localPosition = Vector2.zero;
            triggerModule.colliderScript.gameObject.name = "OxygenProductionBoostRange";
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().radius = moduleData.oxygenProductionBoostRangeRadius;
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().offset = Vector2.zero;
            triggerModule.colliderScript.onTriggerEnter2D = OnTriggerEnter2D;
            triggerModule.colliderScript.onTriggerExit2D = OnTriggerExit2D;
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
                else // boost oxygen production for nearby plants
                {
                    if (plantsInRange[i].plantModules.ContainsKey(PlantModuleEnum.OxygenProduction)){
                        OxygenProductionModule oxygenProductionModule = (OxygenProductionModule)plantsInRange[i].plantModules[PlantModuleEnum.OxygenProduction];
                        int newOxygenLevel = (int)Math.Ceiling(plantsInRange[i].plantSO.oxygenProductionQuantity[plantsInRange[i].plantData.currStageOfLife] * moduleData.oxygenProductionBoostDecimal);
                        oxygenProductionModule.moduleData.oxygenProductionQuantity = newOxygenLevel;
                        plantsInRange[i].plantModules[PlantModuleEnum.OxygenProduction] = oxygenProductionModule;
                        Debug.Log("Boosting oxygen production for plant " + plantsInRange[i].name);
                    }
                }
            }
        }

        // find a way to increase production quantity of nearby plants until dead
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
                // Decrease oxygen production boost for plants that are now out of range
                PlantScript exitingPlantScript = collider.gameObject.GetComponent<PlantScript>();
                if (exitingPlantScript.plantModules.ContainsKey(PlantModuleEnum.OxygenProduction)){
                    OxygenProductionModule oxygenProductionModule = (OxygenProductionModule)exitingPlantScript.plantModules[PlantModuleEnum.OxygenProduction];
                    oxygenProductionModule.moduleData.oxygenProductionQuantity = (int)Math.Floor(oxygenProductionModule.moduleData.oxygenProductionQuantity / moduleData.oxygenProductionBoostDecimal);
                    exitingPlantScript.plantModules[PlantModuleEnum.OxygenProduction] = oxygenProductionModule;
                }
                Debug.Log("Deboosting oxygen production for plant " + exitingPlantScript.name);
                plantsInRange.Remove(exitingPlantScript);
            }
        }

        public override void OnPlantStageGrowth()
        {
            // by now, stage should be inc'ed alrdy
            moduleData.oxygenProductionBoostDecimal = plantScript.plantSO.oxygenProductionBoostDecimal[plantScript.plantData.currStageOfLife];
            moduleData.oxygenProductionBoostRangeRadius = plantScript.plantSO.oxygenProductionBoostRangeRadius[plantScript.plantData.currStageOfLife];
            // update the collider
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().radius = moduleData.oxygenProductionBoostRangeRadius;
            triggerModule.colliderScript.gameObject.GetComponent<CircleCollider2D>().offset = Vector2.zero;
        }
    }
}
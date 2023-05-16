using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.VisualScripting;

public enum PlantModuleEnum // serialized names of each of the modules
{
    Test,
    InstaKillPests,
    FruitProduction,
    Healing,
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
      {PlantModuleEnum.Test, (plantScript) => new TestModule(plantScript)},
      {PlantModuleEnum.InstaKillPests, (plantScript) => new InstaKillPestsModule(plantScript)},
      {PlantModuleEnum.FruitProduction, (plantScript) => new FruitProductionModule(plantScript)},
      {PlantModuleEnum.Healing, (plantScript) => new HealingModule(plantScript)},
    };

    // returns a new instance of the targetted plantModule 
    public static IPlantModule GetModule(PlantModuleEnum module, PlantScript plantScript)
    {
        return moduleConstructors[module].Invoke(plantScript);
    }

    // Modules

    public abstract class StatefulPlantModule<ModuleData> : IPlantModule
    {
        protected ModuleData moduleData;
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
    public class FruitProductionModuleData
    {
        public float productionRate; // numSeconds for a production cycle to happen
        public int productionQuantity; // number of fruits per cycle of production
        public FruitType fruitType; // icura type enum
        public float timeInCurrentCycleSoFar; // for time tracking and data storage
    }
    public class FruitProductionModule : StatefulPlantModule<FruitProductionModuleData>
    {
        public FruitProductionModule(PlantScript plantScript)
        {
            this.plantScript = plantScript;
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new FruitProductionModuleData
            {
                productionRate = plantScript.plantSO.productionRate[plantScript.plantData.currStageOfLife],
                productionQuantity = plantScript.plantSO.productionQuantity[plantScript.plantData.currStageOfLife],
                fruitType = plantScript.plantSO.fruitType,
                timeInCurrentCycleSoFar = 0f
            };
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

            // Debug.Log("Prate: " + moduleData.productionRate + ", Pquant: " + moduleData.productionQuantity + ", Ptype: " + moduleData.fruitType);
            if (moduleData.timeInCurrentCycleSoFar >= moduleData.productionRate)
            {
                // TODO: actual fruit production (visual + systemic)
                Debug.Log("Producing " + moduleData.productionQuantity + " of type " + moduleData.fruitType.ToString() + " fruit.");

                moduleData.timeInCurrentCycleSoFar = 0f; // Resets the cycle timer.
            }
            
        }

        public override void OnPlantStageGrowth()
        {
            // by now, stage should be inc'ed alrdy
            moduleData.productionRate = plantScript.plantSO.productionRate[plantScript.plantData.currStageOfLife];
            moduleData.productionQuantity = plantScript.plantSO.productionQuantity[plantScript.plantData.currStageOfLife];
        }

        public override void OnPlantGrowthPause()
        {
            moduleData.timeInCurrentCycleSoFar += Time.time - timeAnchor; // add the time so far
        }

        public override void OnPlantGrowthResume() // Suppose this line is called before "update" happens at resume.
        {
            timeAnchor = Time.time; // reset the anchor
        }
    }


    [System.Serializable]
    public class HealingModuleData
    {
        public float healRate; // numSeconds for a healing cycle to happen
        public float healAmount; // flat amount of healing
        public float healPercentage; // percentage of max health healing
        public int healRangeRadius; // size (radius) of the circular detection range from the center of the plant
        public float timeInCurrentCycleSoFar; // for time tracking and data storage
    }
    public class HealingModule : StatefulPlantModule<HealingModuleData>
    {
        public HealingModule(PlantScript plantScript)
        {
            this.plantScript = plantScript;
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new HealingModuleData
            {
                healRate = plantScript.plantSO.healRate[plantScript.plantData.currStageOfLife],
                healAmount = plantScript.plantSO.healAmount[plantScript.plantData.currStageOfLife],
                healPercentage = plantScript.plantSO.healPercentage[plantScript.plantData.currStageOfLife],
                healRangeRadius = plantScript.plantSO.healRangeRadius[plantScript.plantData.currStageOfLife],
                timeInCurrentCycleSoFar = 0f
            };
        }

        DynamicColliderScript colliderScript;
        public override void OnModuleAdd()
        {
            GameObject childObject = new GameObject();
            childObject.transform.SetParent(plantScript.gameObject.transform);
            childObject.transform.localPosition = Vector2.zero;
            childObject.name = "healingRange";
            colliderScript = childObject.AddComponent<DynamicColliderScript>();
            colliderScript.SetCollider(typeof(CircleCollider2D), new Vector2(0, 1), new Vector2(), moduleData.healRangeRadius,
                OnTriggerEnter2D, OnTriggerExit2D);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            Debug.Log("OnTriggerEnter2D called for HealingModule. gameObject: " + collider.gameObject.name);
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            Debug.Log("OnTriggerExit2D called for HealingModule. gameObject: " + collider.gameObject.name);
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
    }
}
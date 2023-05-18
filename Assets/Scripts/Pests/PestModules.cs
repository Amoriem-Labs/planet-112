using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum PestModuleEnum // serialized names of each of the modules
{
    SingleTargetProjectileAttack,
}

// Pest module interfaces (can be customized to include new functions)
// Pest modules define a single behavior of a pest type.
public interface IPestModule
{
    void Update();
    void OnModuleAdd();
    void OnModuleRemove();
    void PauseModule();
    void ResumeModule();
    void AssignDataFromString(String dataString);
    String EncodeDataToString();
}

public static class PestModuleArr
{
    static Dictionary<PestModuleEnum, Func<PestScript, IPestModule>> moduleConstructors = new Dictionary<PestModuleEnum, Func<PestScript, IPestModule>>
    {
      {PestModuleEnum.SingleTargetProjectileAttack, (pestScript) => new SingleTargetProjectileAttackModule(pestScript)},
    };

    // returns a new instance of the targetted pestModule 
    public static IPestModule GetModule(PestModuleEnum module, PestScript pestScript)
    {
        return moduleConstructors[module].Invoke(pestScript);
    }

    #region ModuleStructureHierarchy
    // Modules
    public abstract class StatefulPestModule<ModuleData> : IPestModule
    {
        public ModuleData moduleData;
        protected PestScript pestScript;
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
        public virtual void PauseModule() { }
        public virtual void ResumeModule() { }
    }

    // COPIED FROM PLANTMODULES!!!
    /// <summary>
    /// Main Inheritable Module #1: TimerModule
    /// </summary>
    [System.Serializable]
    public class TimerModuleData
    {
        public float timePerCycle;
        public float timeInCurrentCycleSoFar; // for time tracking and data storage
    }
    public class TimerModule<T> : StatefulPestModule<T> where T : TimerModuleData
    {
        public TimerModule(PestScript pestScript)
        {
            this.pestScript = pestScript;
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

        protected void ResetTimer()
        {
            timeAnchor = 0f;
            moduleData.timeInCurrentCycleSoFar = 0f;
        }
    }

    /// <summary>
    /// Main Inheritable Module #2: TriggerModule
    /// </summary>
    [System.Serializable]
    public class TriggerModuleData
    {

    }
    public class TriggerModule<T> : StatefulPestModule<T> where T : TriggerModuleData
    {
        public TriggerModule(PestScript pestScript)
        {
            this.pestScript = pestScript;
        }

        public TriggerResponse colliderScript;

        public override void OnModuleAdd()
        {
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
    public class TriggerAndTimerModule<T> : StatefulPestModule<T> where T : TriggerAndTimerModuleData
    {
        protected TriggerModule<TriggerModuleData> triggerModule;
        protected TimerModule<TimerModuleData> timerModule;

        public TriggerAndTimerModule(PestScript pestScript)
        {
            this.pestScript = pestScript;

            // Instantiate the helper modules and assign their properties
            triggerModule = new TriggerModule<TriggerModuleData>(pestScript);
            timerModule = new TimerModule<TimerModuleData>(pestScript);
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

    /////////////////////////////////// ACTUAL MODULE IMPLEMENTATIONS BEGINS HERE ///////////////////////////////////////
    [System.Serializable]
    public class SingleTargetProjectileAttackModuleData : TimerModuleData
    {
        public float damageAmount;
        public float damageRangeRadius;
        public float projectileSpeed;
        public bool isOn; // only shoot if the pest stops and in range. (Assume so)
    }
    public class SingleTargetProjectileAttackModule : TimerModule<SingleTargetProjectileAttackModuleData>
    {
        public SingleTargetProjectileAttackModule(PestScript pestScript) : base(pestScript)
        {
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new SingleTargetProjectileAttackModuleData
            {
                damageAmount = pestScript.pestSO.singleTargetProjectileDamageAmount[pestScript.pestData.currStageOfLife],
                damageRangeRadius = pestScript.pestSO.singleTargetProjectileAttackRangeRadius[pestScript.pestData.currStageOfLife],
                projectileSpeed = pestScript.pestSO.singleTargetProjectileSpeed[pestScript.pestData.currStageOfLife],
                timePerCycle = pestScript.pestSO.singleTargetProjectileAttackRate[pestScript.pestData.currStageOfLife], // shootRate
                timeInCurrentCycleSoFar = 0f,
                isOn = false
            };
        }

        public override void PauseModule() // turn off attack
        {
            ResetTimer(); // resets attack cd
            moduleData.isOn = false;
        }

        public override void ResumeModule() // turn on Attack
        {
            moduleData.isOn = true;
        }

        public override void Update()
        {
            if (moduleData.isOn)
            {
                base.Update(); // Timer is only updated if attacking
            }
        }

        public override void OnCycleComplete()
        {
            // Launch projectile
            Debug.Log("Projectile being generated... TODO");
        }
    }
}
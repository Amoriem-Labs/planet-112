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
    }


    /////////////////////////////////// ACTUAL MODULE IMPLEMENTATIONS BEGINS HERE ///////////////////////////////////////
    [System.Serializable]
    public class SingleTargetProjectileAttackModuleData
    {
        public string name;
        public int age;
        public string job;
    }
    public class SingleTargetProjectileAttackModule : StatefulPestModule<SingleTargetProjectileAttackModuleData>
    {
        public SingleTargetProjectileAttackModule(PestScript pestScript)
        {
            this.pestScript = pestScript;
            moduleData = new SingleTargetProjectileAttackModuleData
            {
                name = "default",
                age = 0,
                job = "tesla engineer",
            };
        }

        public override void Update()
        {
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
}
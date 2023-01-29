using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public enum PlantModuleEnum // serialized names of each of the modules
{
    Test,
    Test2
}

// Plant module interfaces (can be customized to include new functions)
// Plant modules define a single behavior of a plant type.
public interface IPlantModule
{
    void Run();
    void AssignDataFromString(String dataString);
    String EncodeDataToString();
}

public static class PlantModuleArr
{
    static Dictionary<PlantModuleEnum, Func<PlantScript, IPlantModule>> moduleConstructors = new Dictionary<PlantModuleEnum, Func<PlantScript, IPlantModule>>
    {
        {PlantModuleEnum.Test, (plantScript) => new TestModule(plantScript)},
        {PlantModuleEnum.Test2, (plantScript) => new Test2Module(plantScript)}
    };

    // returns a new instance of the targetted plantModule 
    public static IPlantModule GetModule(PlantModuleEnum module, PlantScript plantScript)
    {
        return moduleConstructors[module].Invoke(plantScript);
    }
}

public class TestModule : IPlantModule
{
    string name;
    int age;
    string job;
    PlantScript plantScript;
    public TestModule(PlantScript plantScript)
    {
        this.plantScript = plantScript;
        name = "default";
        age = 0;
        job = "tesla engineer";
    }

    public void Run()
    {
        Debug.Log("Test Module output: the plant's time left is " + plantScript.plantData.stageTimeLeft);
    }
    
    public String EncodeDataToString() {
        Dictionary<String, dynamic> map = new Dictionary<string, dynamic> {
            {"name", name},
            {"age", age},
            {"job", job}
        };
        return JsonUtility.ToJson(map);
    }
    
    public void AssignDataFromString(String dataString) {
        Dictionary<String, dynamic> map = JsonUtility.FromJson<Dictionary<String, dynamic>>(dataString);
        name = map["name"];
        age = map["age"];
        job = map["job"];
    }
}

public class Test2Module : IPlantModule
{
    int timeLeft;
    int hp;
    PlantScript plantScript;
    public Test2Module(PlantScript plantScript) {
        this.plantScript = plantScript;
        timeLeft = 0;
        hp = 10000;
    }
    public void Run() {
        Debug.Log("Test 2 running");
    }

  public String EncodeDataToString()
  {
    Dictionary<String, dynamic> map = new Dictionary<string, dynamic> {
            {"timeLeft", timeLeft},
            {"hp", hp}
    };
    return JsonUtility.ToJson(map);
  }

  public void AssignDataFromString(String dataString)
  {
    Dictionary<String, dynamic> map = JsonUtility.FromJson<Dictionary<String, dynamic>>(dataString);
    timeLeft = map["timeLeft"];
    hp = map["hp"];
  }
}
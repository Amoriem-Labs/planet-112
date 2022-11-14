using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public enum PlantModules // serialized names of each of the modules
{
    test
}

// Plant module interfaces (can be customized to include new functions)
// Plant modules define a single behavior of a plant type.
public interface IDoStuff
{
    void DoStuff();
}

public class PlantModuleArr
{
    static Func<PlantScript, IDoStuff>[] deepCopiers = new Func<PlantScript, IDoStuff>[]
    {
        TestModule.deepCopier
    };

    // returns a new instance of the targetted plantModule 
    public static IDoStuff GetModule(PlantModules module, PlantScript plantScript)
    {
        return deepCopiers[(int)module].Invoke(plantScript);
    }
}

public class TestModule : IDoStuff
{
    public static Func<PlantScript, IDoStuff> deepCopier = GenerateNewInstance;
    public static TestModule GenerateNewInstance(PlantScript plantScript) { return new TestModule(plantScript); }

    PlantScript plantScript;
    public TestModule(PlantScript plantScript) { this.plantScript = plantScript; }

    public void DoStuff()
    {
        Debug.Log("Test Module output: the plant's time left is " + plantScript.plantData.stageTimeLeft);
    }
}


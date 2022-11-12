using System.Collections;
using System.Collections.Generic;
using System;

public enum PlantNames // Actual names of the plants! 
{
    Bob //0
}

/* // screw the initializers too much work at run time. Just give each prefab a unique script, short anyway ;D
public enum PlantModules
{
    ProduceOxygen, // 0
    ProduceFruit
}

public class PlantModuleInitiator
{
    //goal: each collection of modules below refers to a plant's modules in the corresponding names.
    public static PlantModules[][] plantModules = //an array of elements, each element is an array of PlantModules elements.
    {
        new PlantModules[] { PlantModules.ProduceOxygen }, // 0
        new PlantModules[] { PlantModules.ProduceFruit }, // 1

    };

    // only if I can have an array of class pointe
}
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MrHealer : PlantScript
{
    // Unique variables to this type of plant!
    public int healAmount;
    public int healRadius;

    public MrHealer() // automatically called by Unity.
    {
        // Eg. supportModules.Add(new HealNearbyPlants(this));
    }
}

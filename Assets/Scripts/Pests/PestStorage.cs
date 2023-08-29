using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PestName // Actual names of the pests! 
{
    EvilRoach, // 0
    ArmoredSkeeto,
    FireballBee,
}

// Data is predefined and non-dynamic, but ideally this script is only spawned once at beginning and sticks to persistent managers. 
public class PestStorage : MonoBehaviour // gonna switch to scriptable object in the future, or no need? Ask Jacob. 
{
    public PestScript[] pestPrefabsInit; // declared in editor
    private static PestScript[] pestPrefabs; //MAKE SURE THE PREFAB INDEX MATCHES ENUM OF NAME! // had to do this cuz Unity hides static.

    private void Awake() // test if this can return null possibly. Test sult: nope, we good. 
    {
        pestPrefabs = pestPrefabsInit; // passed by reference I think, so run time no need to worry. Static for convenience. 
    }

    public static GameObject GetPestPrefab(PestName pestName)
    {
        return pestPrefabs[(int)pestName].gameObject; // returns the prefab blueprint
    }
}
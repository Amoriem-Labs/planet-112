using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static int currentLevelID = 0; // change when level changes.
     
    // We should prob keep the functions below in game manager. Level manager only deals with scene transitions. 
    // Spawns in a new plant
    public static GameObject SpawnPlant(PlantName plantName, Vector2 location)
    {
        GameObject plantObj = GridScript.SpawnObjectAtGrid(location, PlantStorage.GetPlantPrefab(plantName));

        if(plantObj != null)
        {
            PlantScript plantScript = plantObj.GetComponent<PlantScript>();
            plantScript.InitializePlantData(location);
            PersistentData.GetLevelData(currentLevelID).plantDatas.Add(plantScript.plantData);

            plantScript.SpawnInModules();
            plantScript.VisualizePlant();
        }
        
        return plantObj;
    }
    
    // Spawns in an existing plant
    public static GameObject SpawnPlant(PlantData plantData)
    {
        GameObject plantObj = GridScript.SpawnObjectAtGrid(plantData.location, PlantStorage.GetPlantPrefab((PlantName)plantData.plantName));
        
        if(plantObj != null)
        {
            PlantScript plantScript = plantObj.GetComponent<PlantScript>();
            plantScript.plantData = plantData;

            plantScript.SpawnInModules();
            plantScript.VisualizePlant();
        }

        return plantObj;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

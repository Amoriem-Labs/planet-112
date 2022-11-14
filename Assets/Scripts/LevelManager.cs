using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static int currentLevelID = 9; // =9 for testing
     
    // We should prob keep the functions below in game manager. Level manager only deals with scene transitions. 
    // Spawns in a new plant
    public static GameObject SpawnPlant(PlantName plantName, int x, int y)
    {
        GameObject plantObj = Instantiate(PlantStorage.GetPlantPrefab(plantName));

        PlantScript plantScript = plantObj.GetComponent<PlantScript>();
        plantScript.InitializePlantData(x, y);
        PersistentData.GetLevelData(currentLevelID).plantDatas.Add(plantScript.plantData);

        plantScript.SpawnInModules();
        plantScript.VisualizePlant();
        
        return plantObj;
    }
    
    // Spawns in an existing plant
    public static GameObject SpawnPlant(PlantData plantData)
    {
        GameObject plantObj = Instantiate(PlantStorage.GetPlantPrefab((PlantName)plantData.plantName));
        
        PlantScript plantScript = plantObj.GetComponent<PlantScript>();
        plantScript.plantData = plantData;
        // PersistentData.GetLevelData(currentLevelID).plantDatas.Add(plantScript.plantData);

        plantScript.SpawnInModules();
        plantScript.VisualizePlant();
        
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

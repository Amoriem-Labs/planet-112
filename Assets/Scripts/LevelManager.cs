using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static int currentLevelID = 9; // =9 for testing
    
    public static GameObject SpawnPlant(PlantName plantName, int x, int y)
    {
        GameObject plantObj = Instantiate(PlantStorage.GetPlantPrefab(plantName));

        PlantScript plantScript = plantObj.GetComponent<PlantScript>();
        plantScript.InitializePlantData(x, y);
        PersistentData.GetLevelData(currentLevelID).plantDatas.Add(plantScript.plantData);

        plantScript.VisualizePlant();
        
        return plantObj;
    }
    
    public static GameObject SpawnPlant(PlantName plantName, PlantData plantData)
    {
        GameObject plantObj = Instantiate(PlantStorage.GetPlantPrefab(plantName));
        
        PlantScript plantScript = plantObj.GetComponent<PlantScript>();
        plantScript.plantData = plantData;
        PersistentData.GetLevelData(currentLevelID).plantDatas.Add(plantScript.plantData);
        
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

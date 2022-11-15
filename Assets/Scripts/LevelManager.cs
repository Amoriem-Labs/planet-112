using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static int currentLevelID = 0; // change when level changes.
     
    // We should prob keep the functions below in game manager. Level manager only deals with scene transitions. 
    // Spawns in a new plant
    public static GameObject SpawnPlant(PlantName plantName, Vector2 location) // location has to be mapGrid int coords!
    {
        GameObject plantPrefab = PlantStorage.GetPlantPrefab(plantName);
        GameObject plantObj = GridScript.SpawnObjectAtGrid(location, plantPrefab, plantPrefab.GetComponent<PlantScript>().plantSO.relativeGridsOccupied);

        if(plantObj != null)
        {
            PlantScript plantScript = plantObj.GetComponent<PlantScript>();
            plantScript.InitializePlantData(location);

            plantScript.SpawnInModules();
            plantScript.VisualizePlant();
        }
        
        return plantObj;
    }
    
    // Spawns in an existing plant
    public static GameObject SpawnPlant(PlantData plantData)
    {
        GameObject plantPrefab = PlantStorage.GetPlantPrefab((PlantName)plantData.plantName);
        GameObject plantObj = GridScript.SpawnObjectAtGrid(plantData.location, plantPrefab, plantPrefab.GetComponent<PlantScript>().plantSO.relativeGridsOccupied);
        
        if(plantObj != null)
        {
            PlantScript plantScript = plantObj.GetComponent<PlantScript>();
            plantScript.plantData = plantData;

            plantScript.SpawnInModules();
            plantScript.VisualizePlant();
        }

        return plantObj;
    }

    public static void KillPlant(PlantScript plantScript) // plantScript belonging to an existing plant
    {
        // Free up the space
        GridScript.RemoveObjectFromGrid(plantScript.plantData.location, plantScript.plantSO.relativeGridsOccupied);
        // Call some internal matters plant deals with
        plantScript.OnPlantDeath();
    }
}

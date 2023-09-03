using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    public PersistentData persistentData;
    public static int plantID;
    public Settings settings;

    void Awake(){
        DontDestroyOnLoad(gameObject);
        plantID = 0; // Replace later when loading in save system.
    }

    void Start()
    {
        settings.LoadSaveFileUIs(); // This loads in the text UI for the save files in the savePanel of the Settings menu
    }

    #region SpawnPestWaveFunctions
    public static IEnumerator StartPestWaves(Level levelSO){
        for (int pestWaveIndex = 0; pestWaveIndex < levelSO.pestWavePestNames.Length; pestWaveIndex++){
            for (int i = 0; i < levelSO.pestWaveNumPests[pestWaveIndex]; i++){
                Vector2 location = new Vector2();
                if(Random.value < 0.5f){
                    location.x = 1.0f;
                } else {
                    location.x = 29.0f;
                }
                location.y = Random.Range(0.5f, 3.65f);
                SpawnPest(levelSO.pestWavePestNames[pestWaveIndex], location);
            }
            if (pestWaveIndex != levelSO.pestWavePestNames.Length - 1){
                yield return new WaitForSeconds(levelSO.pestWaveDelays[pestWaveIndex]);
            } else {
                yield return null;
            }
        }
    }

    public static GameObject SpawnPest(PestName pestName, Vector2 location){
        GameObject pestPrefab = PestStorage.GetPestPrefab(pestName);
        Debug.Log($"Spawning pest at x:{location.x} and y:{location.y}");
        GameObject pestObj = Instantiate(pestPrefab, location, pestPrefab.transform.rotation);

        /* if (plantObj != null)
        {
            AudioManager.GetSFX("plantSFX").Play();
            PlantScript plantScript = plantObj.GetComponent<PlantScript>();
            plantScript.ID = plantID;
            plantID += 1;
            plantScript.InitializePlantData(location);

            plantScript.SetMainCollider();
            plantScript.SpawnInModules();
            plantScript.VisualizePlant();
        } */

        return pestObj;
    }

    #endregion

    #region PlantFunctions
    // We should prob keep the functions below in game manager. Level manager only deals with scene transitions. 
    // Spawns in a new plant
    public static GameObject SpawnPlant(PlantName plantName, Vector2 location) // location has to be mapGrid int coords!
    {
        GameObject plantPrefab = PlantStorage.GetPlantPrefab(plantName);
        GameObject plantObj = GridScript.SpawnObjectAtGrid(location, plantPrefab, plantPrefab.GetComponent<PlantScript>().plantSO.offset[0],
            plantPrefab.GetComponent<PlantScript>().plantSO.relativeGridsOccupied[0].vec2Array); // when a new plant is spawned, currStageOfLife is 0

        if (plantObj != null)
        {
            AudioManager.GetSFX("plantSFX").Play();
            PlantScript plantScript = plantObj.GetComponent<PlantScript>();
            plantScript.ID = plantID; 
            plantID += 1;
            plantScript.InitializePlantData(location);

            plantScript.SetMainCollider();
            plantScript.SpawnInModules();
            plantScript.VisualizePlant();
        }

        return plantObj;
    }

    public static GameObject SpawnPlantRemoveLater(PlantName plantName, Vector2 location) // location has to be mapGrid int coords!
    {
        GameObject plantPrefab = PlantStorage.GetPlantPrefab(plantName);
        GameObject plantObj = GridScript.SpawnObjectAtGridRemoveLater(location, plantPrefab, plantPrefab.GetComponent<PlantScript>().plantSO.offset[0],
            plantPrefab.GetComponent<PlantScript>().plantSO.relativeGridsOccupied[0].vec2Array); // when a new plant is spawned, currStageOfLife is 0

        if (plantObj != null)
        {
            AudioManager.GetSFX("plantSFX").Play();
            PlantScript plantScript = plantObj.GetComponent<PlantScript>();
            plantScript.ID = plantID; 
            plantID += 1;
            plantScript.InitializePlantData(location);

            plantScript.SetMainCollider();
            plantScript.SpawnInModules();
            plantScript.VisualizePlant();
        }

        return plantObj;
    }

    /* public static GameObject LoadPlant(PlantData plantData) // location has to be mapGrid int coords!
    {
        GameObject plantPrefab = PlantStorage.GetPlantPrefab((PlantName)plantData.plantName);
        GameObject plantObj = GridScript.SpawnObjectAtGrid(plantData.location, plantPrefab, plantPrefab.GetComponent<PlantScript>().plantSO.offset[plantData.currStageOfLife],
            plantPrefab.GetComponent<PlantScript>().plantSO.relativeGridsOccupied[plantData.currStageOfLife].vec2Array);

        if (plantObj != null)
        {
            PlantScript plantScript = plantObj.GetComponent<PlantScript>();
            plantScript.ID = plantID; 
            plantID += 1;
            plantScript.LoadPlantData(plantData);

            plantScript.SetMainCollider();
            plantScript.SpawnInModules();
            plantScript.VisualizePlant();
        }

        return plantObj;
    } */

    // Spawns in an existing plant
    public static GameObject SpawnPlant(PlantData plantData)
    {
        GameObject plantPrefab = PlantStorage.GetPlantPrefab((PlantName)plantData.plantName);
        PlantScript plantScript = plantPrefab.GetComponent<PlantScript>();
        GameObject plantObj = GridScript.SpawnObjectAtGrid(plantData.location, plantPrefab, plantScript.plantSO.offset[plantData.currStageOfLife],
            plantScript.plantSO.relativeGridsOccupied[plantData.currStageOfLife].vec2Array);

        if (plantObj != null)
        {
            AudioManager.GetSFX("plantSFX").Play();

            plantScript.plantData = plantData;

            plantScript.SetMainCollider();
            plantScript.SpawnInModules();
            plantScript.VisualizePlant();
        }

        return plantObj;
    }

    public static void KillPlant(PlantScript plantScript) // plantScript belonging to an existing plant
    {
        if (plantScript != null) // could be null, if multiple sources kill one plant in the same frame.
        {
            // Free up the space if not picked up. If dies while in hand then no need to free since already freed when picked up
            if(!plantScript.pickedUp) GridScript.RemoveObjectFromGrid(plantScript.plantData.location, plantScript,
                plantScript.plantSO.relativeGridsOccupied[plantScript.plantData.currStageOfLife].vec2Array);
            // Call some internal matters plant deals with
            plantScript.OnPlantDeath();
        }
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        PersistentData.LoadSave(0);

        // Testing scripts
        GameObject tst = Instantiate(PlantStorage.GetPlantPrefab(PlantNames.MrHealer));
        tst.GetComponent<PlantScript>().SpawnNewPlant(5, -3);
        tst.GetComponent<PlantScript>().TrySupport(); 

        //Debug.Log("Current level oxygen is: " + PersistentData.GetLevelData(LevelManager.currentLevelID).oxygenLevel);
        //StartCoroutine(TestFunc(tst.GetComponent<ProductivePlant>()));
    }

    IEnumerator TestFunc(PlantScript plant)
    {
        yield return new WaitForSeconds(5);

        plant.TryProduce();
        Debug.Log("Test func executed");
        Debug.Log("Current level oxygen is: " + PersistentData.GetLevelData(LevelManager.currentLevelID).oxygenLevel);
    }
}

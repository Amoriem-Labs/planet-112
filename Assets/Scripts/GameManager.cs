using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        PersistentData.LoadSave(0);

        // DEMO: create a new MrHealer plant
        //GameObject tst = Instantiate(PlantStorage.GetPlantPrefab(PlantName.MrHealer));
        //tst.GetComponent<PlantScript>().SpawnNewPlant(5, -3);
        GameObject testPlant = LevelManager.SpawnPlant(PlantName.MrHealer, 5, -3);
    }
}

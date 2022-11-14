using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PersistentData.LoadSave(0);

        // DEMO: create a new MrHealer plant
        GameObject testPlant = LevelManager.SpawnPlant(PlantName.Bob, 5, -3);
        testPlant.GetComponent<PlantScript>().RunPlantModules(new List<PlantModules>() { PlantModules.test });
    }
}

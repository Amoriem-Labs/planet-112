using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ProductivePlant bob;

    // Start is called before the first frame update
    void Awake()
    {
        PersistentData.LoadSave(0);

        /*
        GameObject tst = Instantiate(bob.gameObject);
        tst.GetComponent<ProductivePlant>().SpawnNewPlant(5, -3);

        tst.GetComponent<ProductivePlant>().TryProduce(); */

        //Debug.Log("Current level oxygen is: " + PersistentData.GetLevelData(LevelManager.currentLevelID).oxygenLevel);
        //StartCoroutine(TestFunc(tst.GetComponent<ProductivePlant>()));
    }

    IEnumerator TestFunc(ProductivePlant plant)
    {
        yield return new WaitForSeconds(5);

        plant.TryProduce();
        Debug.Log("Test func executed");
        Debug.Log("Current level oxygen is: " + PersistentData.GetLevelData(LevelManager.currentLevelID).oxygenLevel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

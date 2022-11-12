using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public ProductivePlant bob;

    // Start is called before the first frame update
    void Start()
    {
        PersistentData.LoadSave(0);

        GameObject tst = Instantiate(bob.gameObject);
        tst.GetComponent<ProductivePlant>().SpawnNewPlant(5, -3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

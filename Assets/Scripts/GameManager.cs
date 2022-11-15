using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        PersistentData.CreateNewSave(0); // Now it should work ;D
        PersistentData.LoadSave(0);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
}

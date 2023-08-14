using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mav : MonoBehaviour
{
    void Awake(){
        DontDestroyOnLoad(gameObject);
    }
}

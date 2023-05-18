using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A singleton for easy prefab reference
public class UtilPrefabStorage : MonoBehaviour
{
    public static UtilPrefabStorage Instance { get; private set; }

    public GameObject boxDetector; // Assign this in the inspector
    public GameObject circleDetector;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject InstantiatePrefab(GameObject prefab, Vector2 position, Quaternion rotation, Transform parent)
    {
        return Instantiate(prefab, position, rotation, parent);
    }
}

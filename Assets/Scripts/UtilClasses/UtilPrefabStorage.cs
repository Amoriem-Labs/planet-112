using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A singleton for easy prefab reference
public class UtilPrefabStorage : MonoBehaviour
{
    public static UtilPrefabStorage Instance { get; private set; }

    public GameObject boxDetector; // Assign this in the inspector
    public GameObject circleDetector;
    public GameObject boxProjectile;
    public GameObject fireballProjectile;

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
    
    // Only use this method if you are instantiating a prefab with a RigidBody2D component (like the icura)
    public GameObject InstantiatePrefab(GameObject prefab, Vector2 position, Quaternion rotation, Transform parent, Vector2 velocity)
    {
        GameObject newPrefab = Instantiate(prefab, position, rotation, parent);
        newPrefab.GetComponent<Rigidbody2D>().velocity = velocity;
        return newPrefab;
    }
}

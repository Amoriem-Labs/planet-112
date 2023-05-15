using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DynamicColliderScript : MonoBehaviour
{
    public Action<Collider2D> onTriggerEnter2D;
    public Action<Collider2D> onTriggerExit2D;
    Collider2D myCollider;
    void Start()
    {
        myCollider = gameObject.AddComponent<BoxCollider2D>();
        myCollider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.transform != gameObject.transform.parent)
        {
            if (onTriggerEnter2D != null)
            {
                onTriggerEnter2D(collider);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.transform != gameObject.transform.parent)
        {
            if (onTriggerExit2D != null)
            {
                onTriggerExit2D(collider);
            }
        }
    }
}

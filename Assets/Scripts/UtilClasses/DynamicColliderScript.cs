using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DynamicColliderScript : MonoBehaviour
{
    public Action<Collider2D> onTriggerEnter2D;
    public Action<Collider2D> onTriggerExit2D;
    Collider2D myCollider;

    public void SetCollider(Type componentType, Vector2 offset, Vector2 boxDim, float radius,
        Action<Collider2D> onTriggerEnter2D, Action<Collider2D> onTriggerExit2D, bool isTrigger = true)
    {
        myCollider = (Collider2D) gameObject.AddComponent(componentType); // has to be a collider2D type
        if (componentType == typeof(BoxCollider2D))
        {
            BoxCollider2D boxCollider = (BoxCollider2D) myCollider;
            boxCollider.size = boxDim;
        }
        else if (componentType == typeof(CircleCollider2D))
        {
            CircleCollider2D circleCollider = (CircleCollider2D) myCollider;
            circleCollider.radius = radius;
        }
        myCollider.offset = offset;
        myCollider.isTrigger = isTrigger;
        this.onTriggerEnter2D = onTriggerEnter2D;
        this.onTriggerExit2D = onTriggerExit2D;
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

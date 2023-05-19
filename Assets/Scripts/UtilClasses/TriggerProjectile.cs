using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// DISCLAIMER: the projectile here is a trigger, not a rigidbody.
public class TriggerProjectile : MonoBehaviour
{
    private float speed = 0f;
    private Vector2 direction;
    private Action<Collider2D, TriggerProjectile> onTriggerEnter2D;
    private Action<Collider2D> onTriggerExit2D;

    public void SetProjectileStats(float speed, Vector2 direction, Action<Collider2D, TriggerProjectile> onTriggerEnter2D, Action<Collider2D> onTriggerExit2D)
    {
        this.speed = speed;
        this.direction = direction;
        this.onTriggerEnter2D = onTriggerEnter2D;
        this.onTriggerExit2D = onTriggerExit2D;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.transform != gameObject.transform.parent)
        {
            if (onTriggerEnter2D != null)
            {
                onTriggerEnter2D(collider, this);
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

    // Update is called once per frame
    void Update()
    {
        // Move the projectile in the set direction
        transform.Translate(direction.normalized * speed * Time.deltaTime);
    }
}
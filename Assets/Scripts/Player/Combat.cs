using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
        public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask pestLayers;

    // Attacks if space is pressed
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Attack();
        }
    }

    void Attack() 
    {
        print("attacking");

        // Pests in within range of attack
        Collider2D[] hitPests = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, pestLayers);

        // Damage pests
        foreach(Collider2D pest in hitPests)
        {
            print("We hit " + pest.name);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    
}

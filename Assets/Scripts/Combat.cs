using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{
  // Point of origin of attack
  public Transform attackPoint;
  public static Transform attackPointStatic;
  public LayerMask enemyLayers;
  public static LayerMask enemyLayersStatic;
  // TODO: get attack damage from weapon class to differentiate weapon specs
  public static float attackDamage = 5f;
  public static float playerAttackRange = 0.5f;

  void Awake(){
    attackPointStatic = attackPoint;
    enemyLayersStatic = enemyLayers;
  }

  public static void StickAttack()
  {
    print("attacking");

    // Pests in within range of attack
    Collider2D[] hitPests = Physics2D.OverlapCircleAll(attackPointStatic.position, playerAttackRange, enemyLayersStatic);

    // Damage pests. Only damage one at a time.
    if (hitPests.Length > 0){
      Collider2D pest = hitPests[0];
      AudioManager.GetSFX("thudSFX").Play();
      print("We hit " + pest.name);
      pest.GetComponent<PestScript>().TakeDamage(attackDamage);
    }
  }

  void OnDrawGizmosSelected()
  {
    if (attackPointStatic == null)
      return;
    Gizmos.DrawWireSphere(attackPointStatic.position, playerAttackRange);
  }
}
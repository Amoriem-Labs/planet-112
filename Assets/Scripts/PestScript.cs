using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    STATE_SEARCHING,
    STATE_MOVING,
    STATE_ATTACKING,
    STATE_RETREAT
}

public class PestScript : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float attackRange = 5f;

    [SerializeField] float attackRate = 2f;
    [SerializeField] float attackDamage = 2f;

    // remove once big timer implemented
    float nextAttackTime;

    Vector2 retreatPoint;

    State currentState;
    List<PlantScript> plantScripts = new List<PlantScript>();
    PlantScript targetPlantScript;
    const float MAX_DISTANCE = 5000f;

    private void Awake()
    {
        //currentState = State.STATE_MOVING;
        currentState = State.STATE_SEARCHING;
        //SearchForPlant();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case State.STATE_SEARCHING:
                DuringSearch();
                break;
            case State.STATE_MOVING:
                DuringMove();
                break;
            case State.STATE_ATTACKING:
                DuringAttack();
                break;
            case State.STATE_RETREAT:
                DuringRetreat();
                break;
        }
    }

    void SearchForPlant()
    {
        float closestDistance = MAX_DISTANCE;
        foreach (PlantScript plant in GameObject.FindObjectsOfType<PlantScript>())
        {
            float currentDistance = Vector3.Distance(transform.position, plant.transform.position);
            if (currentDistance < closestDistance && plant.attackers < plant.plantSO.maxAttackers)
            {
                closestDistance = currentDistance;
                targetPlantScript = plant;
            }
        }
        if(targetPlantScript != null)
        {
            var dir = targetPlantScript.transform.position - transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            currentState = State.STATE_MOVING;
        }
    }

    void DuringSearch()
    {
        SearchForPlant();
    }

    void DuringMove()
    {
        // TODO: fix this alg below. The bug could get stuck focusing on that plant.
        if (targetPlantScript.attackers >= targetPlantScript.plantSO.maxAttackers)
        {
            SearchForPlant();
        }

        transform.position = Vector2.MoveTowards(transform.position, targetPlantScript.transform.position, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPlantScript.transform.position) <= attackRange)
        {
            currentState = State.STATE_ATTACKING;
            targetPlantScript.attackers++;
            nextAttackTime = Time.time + attackRate;
        }
    }

    void DuringAttack()
    {
        // check if plant dies, if so call SearchForPlant()
        if (targetPlantScript == null) currentState = State.STATE_SEARCHING;

        // should use big timer once implemented
        if (Time.time > nextAttackTime)
        {
            Debug.Log("Attacking target plant, hp left: " + targetPlantScript.plantData.currentHealth);
            nextAttackTime = Time.time + attackRate;
            // reduce plant health
            targetPlantScript.TakeDamage((int)attackDamage);
        }

        // TODO: figure out when should enter retreat state
        // set retreatPoint to corner of camera OR when we implement level bounds, to outside level bounds
        // if setting to outside level bounds, can be done when initialized at the top of the script
    }

    void DuringRetreat()
    {
        transform.position = Vector2.MoveTowards(transform.position, retreatPoint, speed * Time.deltaTime);
    }
}

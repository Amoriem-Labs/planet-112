using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    //STATE_SEARCHING,
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
    GameObject closestPlant;
    PlantScript closestPlantScript;
    const float MAX_DISTANCE = 5000f;

    private void Awake()
    {
        currentState = State.STATE_MOVING;
        SearchForPlant();
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
            //case State.STATE_SEARCHING:
            //    DuringSearch();
            //    break;
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
            float currentDistance = Vector3.Distance(transform.position, plant.gameObject.transform.position);
            if (currentDistance < closestDistance && plant.attackers < plant.maxAttackers)
            {
                closestDistance = currentDistance;
                closestPlant = plant.gameObject;
                closestPlantScript = plant;
            }
        }
        var dir = closestPlant.transform.position - transform.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);        
    }

    void DuringSearch()
    {

    }

    void DuringMove()
    {
        if (closestPlantScript.attackers >= closestPlantScript.maxAttackers)
        {
            SearchForPlant();
        }

        transform.position = Vector2.MoveTowards(transform.position, closestPlant.transform.position, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, closestPlant.transform.position) <= attackRange)
        {
            currentState = State.STATE_ATTACKING;
            closestPlantScript.attackers++;
            nextAttackTime = Time.time + attackRate;
        }
    }

    void DuringAttack()
    {
        // should use big timer once implemented
        if (Time.time > nextAttackTime)
        {
            Debug.Log("damage: " + attackDamage);
            nextAttackTime = Time.time + attackRate;
            // reduce plant health
            // check if plant dies, if so call SearchForPlant()
        }

        // figure out when should enter retreat state
        // set retreatPoint to corner of camera OR when we implement level bounds, to outside level bounds
        // if setting to outside level bounds, can be done when initialized at the top of the script
        
        
    }

    void DuringRetreat()
    {
        transform.position = Vector2.MoveTowards(transform.position, retreatPoint, speed * Time.deltaTime);
    }
}

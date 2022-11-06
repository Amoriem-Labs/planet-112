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

    State currentState;
    List<PlantScript> plantScripts = new List<PlantScript>();
    GameObject closestPlant;
    const float MAX_DISTANCE = 2400f;


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
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestPlant = plant.gameObject;
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
        transform.position = Vector2.MoveTowards(transform.position, closestPlant.transform.position, speed * Time.deltaTime);
    }

    void DuringAttack()
    {

    }

    void DuringRetreat()
    {

    }
}

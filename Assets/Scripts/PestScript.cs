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
    // The scriptable oxject that contains fixed (non-dynamic) data about this pest.
    public Plant pestSO;

    [SerializeField] float speed = 5f;
    [SerializeField] float attackRange = 2f;

    [SerializeField] float attackRate = 2f;
    [SerializeField] float attackDamage = 2f;

    // remove once big timer implemented
    float nextAttackTime;

    Vector2 retreatPoint;

    State currentState;
    List<PlantScript> plantScripts = new List<PlantScript>();
    public PlantScript targetPlantScript;
    const float MAX_WEIGHT = 5000f;

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

    public void SetSearchingState()
    {
        currentState = State.STATE_SEARCHING;
    }

    public void SearchForPlant()
    {
        float maxWeight = 0;
        foreach (PlantScript plant in GameObject.FindObjectsOfType<PlantScript>())
        {
            float distanceToPlant = Vector3.Distance(transform.position, plant.transform.position); // might need to use seeker's path generated total distance
            float plantPriority = (float)plant.plantSO.pestAttackPriority;

            // TODO: the function below is subject to modification. Might need a better math model. 
            // current thought: less distance = more weight; more priority = more weight. Most weight plant is the target
            float totalWeight = (1000 / distanceToPlant) + plantPriority;

            if (totalWeight > maxWeight && plant.attackers < plant.plantSO.maxAttackers)
            {
                maxWeight = totalWeight;
                targetPlantScript = plant;
            }
        }

        if(targetPlantScript != null)
        {
            // TODO: find a location from that plant to target/attack'
            var offset = targetPlantScript.plantSO.targetRectParameters[targetPlantScript.plantData.currStageOfLife].vec2Array[0];
            var dim = targetPlantScript.plantSO.targetRectParameters[targetPlantScript.plantData.currStageOfLife].vec2Array[1];
            //var corners = targetPlantScript.GetPlantTargetBoundary();
            //Vector2 bottomLeft = corners[0], topRight = corners[1];

            //Compute the discrete cumulative density function(CDF) of your list-- or in simple terms the array of
            //cumulative sums of the weights. Then generate a random number in the range between 0 and the sum of
            //all weights(might be 1 in your case), do a binary search to find this random number in your discrete
            //CDF array and get the value corresponding to this entry-- this is your weighted random number.
            // no need for Binary Search, only 4 elements. Right now the Pr of each side is determined by relative length, not inspector-defined
            float perimeter = 2 * dim.x + 2 * dim.y;
            float vertWeight = dim.y / perimeter, horiWeight = dim.x / perimeter;
            float totalWeight = vertWeight * 2 + horiWeight * 2;
            float[] cdfArray = { horiWeight, vertWeight + horiWeight, horiWeight + vertWeight + horiWeight, totalWeight }; // [top, right, bottom, left]
            float randVal = Random.Range(0, totalWeight);
            int i = 0;
            for (; i < cdfArray.Length; i++)
            {
                if (randVal <= cdfArray[i]) break;
            }
            // side is based on i
            switch(i)
            {
                case 0: //top
                    GetComponent<PestMovement>().targetOffsetFromCenter =
                        new Vector2(offset.x + Random.Range(-dim.x / 2, dim.x / 2), offset.y + dim.y);
                    break;
                case 1: //right
                    GetComponent<PestMovement>().targetOffsetFromCenter =
                        new Vector2(offset.x + dim.x / 2, offset.y + Random.Range(0, dim.y));
                    break;
                case 2: //bottom
                    GetComponent<PestMovement>().targetOffsetFromCenter =
                        new Vector2(offset.x + Random.Range(-dim.x / 2, dim.x / 2), offset.y);
                    break;
                case 3: //left
                    GetComponent<PestMovement>().targetOffsetFromCenter =
                        new Vector2(offset.x - dim.x / 2, offset.y + Random.Range(0, dim.y));
                    break;
            }

            /*var dir = targetPlantScript.transform.position - transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            currentState = State.STATE_MOVING;*/

            // initiates movement script
            GetComponent<PestMovement>().targetPosition = targetPlantScript.transform;
            GetComponent<PestMovement>().enabled = true;
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
            GetComponent<PestMovement>().resetPath = true;
            GetComponent<PestMovement>().StopPathing(); // initiates pathing ending 
            //SearchForPlant(); // wait for callback from the script
        }

        /*
        transform.position = Vector2.MoveTowards(transform.position, targetPlantScript.transform.position, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPlantScript.transform.position) <= attackRange)
        {
            currentState = State.STATE_ATTACKING;
            targetPlantScript.attackers++;
            nextAttackTime = Time.time + attackRate;
        }*/
    }

    public void StartAttack() // make sure this is only called once. The initialization process
    {
        if(currentState != State.STATE_ATTACKING) // need to do this. Else plant in motion -> multiple end path calls -> perma reset
        {
            currentState = State.STATE_ATTACKING;
            targetPlantScript.attackers++;
            targetPlantScript.pestScripts.Add(this);
            nextAttackTime = Time.time + attackRate;
        }
    }

    // this is to deal with the case where a staionary plant already being attacked is being moved
    public void ChaseAfterPlant()
    {
        if(currentState == State.STATE_ATTACKING)
        {
            GetComponent<PestMovement>().enabled = true;
        }
    }

    void DuringAttack()
    {
        // check if plant dies, if so call SearchForPlant()
        if (targetPlantScript == null)
        {
            currentState = State.STATE_SEARCHING;
            return;
        }

        // should use big timer once implemented
        // Thought here:
        // As long as pest's AA off cd, it will attack the plant even if you hold it and run past it in range.
        // so this adds a bit of mecahnics yay
        if (Time.time > nextAttackTime) // the attack is ready
        {
            // reduce plant health if in attack range. Otherwise no.
            if (TargetPlantInAttackRange())
            {
                // TODO: play attacking animation here
                Debug.Log("Attack animation played");

                targetPlantScript.TakeDamage((int)attackDamage);
                //Debug.Log("Attacking target plant, hp left: " + targetPlantScript.plantData.currentHealth);

                nextAttackTime = Time.time + attackRate; // reset aa timer
            }
        }

        // TODO: figure out when should enter retreat state
        // set retreatPoint to corner of camera OR when we implement level bounds, to outside level bounds
        // if setting to outside level bounds, can be done when initialized at the top of the script
    }

    public bool TargetPlantInAttackRange()
    {
        if(targetPlantScript == null) return false; // destroyed during check

        return Vector3.Distance(transform.position, targetPlantScript.transform.position) <= attackRange;
    }

    void DuringRetreat()
    {
        transform.position = Vector2.MoveTowards(transform.position, retreatPoint, speed * Time.deltaTime);
    }
}

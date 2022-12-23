using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;

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
    public Pest pestSO;

    [SerializeField] float speed = 5f;
    [SerializeField] float attackRange = 2f;

    [SerializeField] float attackRate = 2f;
    [SerializeField] float attackDamage = 2f;

    // remove once big timer implemented
    float nextAttackTime;

    Vector2 retreatPoint;

    public State currentState; // put it back to private later. Public for now to debug. 
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

    private void OnPathsComplete(Path p)
    {
        if (p.error)
        {
            Debug.Log("The multipather has an error." + p.errorLog);
            return;
        }

        MultiTargetPath mp = p as MultiTargetPath;
        if(mp == null)
        {
            Debug.LogError("The path was not a multi-target path");
            return;
        }

        // All Paths
        List<Vector3>[] paths = mp.vectorPaths;
        float maxWeight = 0;
        for (int i = 0; i < paths.Length; i++)
        {
            List<Vector3> path = paths[i];

            if(path == null || currentPlantCache[i] == null)
            {
                Debug.Log("Path number " + i + " could not be found. Prehaps the plant is already destroyed.");
                continue;
            }

            float distanceToPlant = 0;
            for(int j = 0; j < path.Count-1; j++)
            {
                distanceToPlant += Vector2.Distance(path[j], path[j+1]);
            }
            float plantPriority = (float)currentPlantCache[i].plantSO.pestAttackPriority;

            // TODO: the function below is subject to modification. Might need a better math model. 
            // current thought: less distance = more weight; more priority = more weight. Most weight plant is the target
            float totalWeight = (1000 / distanceToPlant) + plantPriority;

            if (totalWeight > maxWeight && currentPlantCache[i].attackers < currentPlantCache[i].plantSO.maxAttackers)
            {
                maxWeight = totalWeight;
                targetPlantScript = currentPlantCache[i];
            }
        }

        queryFinished = true;
    }

    List<PlantScript> currentPlantCache;
    bool queryStarted = false, queryFinished = false;
    public void SearchForPlant()
    {
        if(!queryStarted) // need to make sure a query is finished first
        {
            targetPlantScript = null;

            // two methods. 1. since all paths are returned in the order the targets are passed in, we can just do a 
            // multi-pathing query and wait over some frames. Upside: more efficient, downside: can't detect new plant added in between. 
            // 2. we can use blockuntilcalculated to get a path immediately instead of spreading it out over multiple
            // frames, so we can run a for loop with indiv. path dist and weight calculation in one frame. Upside: 
            // more new plant coverage, downside: much slower. Gonna go with approach 1 for now.
            currentPlantCache = FindObjectsOfType<PlantScript>().ToList(); // replace with direct grabbing from plant storage later

            // Searches all paths. Since boolean is set to true, not just returning the shortest one.
            if (currentPlantCache.Count != 0)
            {
                GetComponent<Seeker>().StartMultiTargetPath(transform.position,
                    currentPlantCache.Select(p => p.transform.position).ToArray(),
                    true, OnPathsComplete);
                queryStarted = true;
            }
        }

        /* Method 2
        float maxWeight = 0;
        foreach (PlantScript plant in GameObject.FindObjectsOfType<PlantScript>())
        {
            //Path p = GetComponent<Seeker>().StartPath(transform.position, plant.transform.position);
            //p.BlockUntilCalculated();
            //float distanceToPlant = p.GetTotalLength();
            //float directDistance = Vector3.Distance(transform.position, plant.transform.position);
            //Debug.Log("Total length of the path is " + distanceToPlant + " and direct distance is " + directDistance);
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
        } */

        if(queryFinished && targetPlantScript != null)
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
            GetComponent<PestMovement>().coreOffsetCache = GetComponent<PestMovement>().targetOffsetFromCenter; // store the data

            // treat this like a point RELATIVE to the offset, recalculate if in motion. different from main offset
            float castAngle; // in radian
            Vector3 castVector; // the direction vector
            if(GetComponent<PestMovement>().targetOffsetFromCenter.x >= offset.x) // right side
            {
                castAngle = Random.Range(0, 90) * (Mathf.PI / 180);
                castVector = RotateVector(Vector3.right, castAngle);
            }
            else // left side
            {
                castAngle = Random.Range(-90, 0) * (Mathf.PI / 180);
                castVector = RotateVector(-Vector3.right, castAngle);
            }
            // Okay this is really weird. I used degree for rotate in Bezier but why is it radian here? I mean here it only works with radian
            // is my Bezier wrong? try with radian later?
            //Debug.DrawLine(targetPlantScript.transform.position + GetComponent<PestMovement>().targetOffsetFromCenter,
            //    targetPlantScript.transform.position + GetComponent<PestMovement>().targetOffsetFromCenter + castVector,
            //    Color.red, 100, false);
            float baseDetectionRange = 1 ; // TODO: make this generalizable over the longest side of the "?" instead of hard-coded
            float radius = dim.x / 2; // make this size generalizable over what...
            int maxDetectionRange = (int)(baseDetectionRange + attackRange);
            var info = Physics2D.CircleCast(targetPlantScript.transform.position + GetComponent<PestMovement>().targetOffsetFromCenter,
                radius,
                castVector,
                maxDetectionRange,
                (1<<LayerMask.NameToLayer("NonGroundObstacle")) // need to do this, because ground is also an obstacle, and we don't want that. Plus might be more in future.
                // tip for above, can do (1<<i) | (1<<j) | ...; for multiple layers. 
                );
            //Debug.Log("Was there a collision: " + (info.collider != null));
            //Debug.Log("Name of the collider is: " + info.collider.gameObject.name);
            //Debug.DrawLine(targetPlantScript.transform.position + GetComponent<PestMovement>().targetOffsetFromCenter, info.point, Color.magenta, 100, false);

            Vector2 decoyTarget;
            if(info.collider == null) // no collision on its way
            {
                decoyTarget = PickRandomPointInCircle(targetPlantScript.transform.position + 
                    GetComponent<PestMovement>().targetOffsetFromCenter + castVector * maxDetectionRange, // no need to normalize dir. Already 1. 
                    radius);
            }
            else // hit something, info parameters came to life
            {
                decoyTarget = PickRandomPointInCircle(info.centroid, radius);
            }
            //Debug.DrawLine(targetPlantScript.transform.position + GetComponent<PestMovement>().targetOffsetFromCenter, decoyTarget, Color.magenta, 100, false);
            // Finally, do something with decoyTarget!
            var decoyTargetOffsetFromCenter = (Vector3)decoyTarget - targetPlantScript.transform.position;
            GetComponent<PestMovement>().decoyState = true;
            GetComponent<PestMovement>().targetOffsetFromCenter = decoyTargetOffsetFromCenter; // can do this, for example. Use boolean "beentodecoyyet"

            /*var dir = targetPlantScript.transform.position - transform.position;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            currentState = State.STATE_MOVING;*/

            // initiates movement script
            GetComponent<PestMovement>().targetPosition = targetPlantScript.transform;
            GetComponent<PestMovement>().enabled = true;
            currentState = State.STATE_MOVING;

            queryStarted = false;
            queryFinished = false;
        }
    }

    void DuringSearch()
    {
        SearchForPlant();
    }

    void DuringMove()
    {
        // if max pest or target plant dies
        if (targetPlantScript.attackers >= targetPlantScript.plantSO.maxAttackers || targetPlantScript == null)
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
        Debug.Log("CHASE AFTER PLANT ACTIVATED");
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

    Vector2 RotateVector(Vector2 v, float theta)
    {
        return new Vector2(
            v.x * Mathf.Cos(theta) - v.y * Mathf.Sin(theta),
            v.x * Mathf.Sin(theta) + v.y * Mathf.Cos(theta)
            );
    }

    Vector2 PickRandomPointInCircle(Vector2 center, float radius)
    {
        var r = radius * Mathf.Sqrt(Random.Range(0f, 1f));
        var theta = Random.Range(0f, 1f) * 2 * Mathf.PI;
        var x = center.x + r * Mathf.Cos(theta);
        var y = center.y + r * Mathf.Sin(theta);
        return new Vector2(x, y);
    }
}

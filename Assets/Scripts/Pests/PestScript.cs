using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;
using System;

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

    // Pest module Dict. They are separated by function. They are not in the scriptable object because that can't have runtime-changeable data.
    protected Dictionary<PestModuleEnum, IPestModule> pestModules = new Dictionary<PestModuleEnum, IPestModule>();

    public PestData pestData; // contains all the dynamic data of a pest to be saved, a reference to PD 

    [SerializeField] float speed = 5f;
    public float attackRange = 2f;

    [SerializeField] float attackRate = 2f;
    [SerializeField] float attackDamage = 2f;

    // remove once big timer implemented
    float nextAttackTime;

    Vector2 retreatPoint;

    public State currentState; // put it back to private later. Public for now to debug. 
    public PlantScript targetPlantScript;

    private void Awake()
    {
        currentState = State.STATE_SEARCHING;
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

        UpdateAllModules();
    }

    #region StateMachine
    public void SetSearchingState()
    {
        currentState = State.STATE_SEARCHING;
    }

    public int queryCount = 0;
    public int expectedQueryCount = 69;
    float maxWeight = 0;
    IEnumerator Perform36PtQueryPerPlant(Vector3[] points, Vector3 center, Vector3[] pointOffsets, List<Vector3> path, int i, float dimY)
    {
        MultiTargetPath possPaths = GetComponent<Seeker>().StartMultiTargetPath(transform.position, points, true);
        yield return StartCoroutine(possPaths.WaitForPath());
        // The path is calculated now
        List<Vector2> availablePosOffsets = new List<Vector2>();
        for (int j = 0; j < 36; j++) // could i get less than 36 paths?
        {
            var pathToPossPoint = possPaths.vectorPaths[j];
            //Debug.Log("Length of pathToPossPoint is " + pathToPossPoint.Count);
            if (Vector2.Distance(pathToPossPoint[pathToPossPoint.Count - 1], center + pointOffsets[j]) <= attackRange)
            {
                // this point is reacheable
                availablePosOffsets.Add(pointOffsets[j]);
                //Debug.DrawLine(center, center + pointOffsets[j], Color.gray, 100, false);
                //Debug.DrawLine(pathToPossPoint[pathToPossPoint.Count - 1], center + pointOffsets[j], Color.gray, 100, false);
            }
        }

        //yield break; // leave the corou
        if (availablePosOffsets.Count != 0) // if == 0, plant is unreacheable and thus ignored
        {
            // we could use a shortest path from one of the reacheable point as the distance, but it's not needed
            // because the point picking process is randomized anyway. So let's just uniformaly count from center.
            float distanceToPlant = 0; // to plant's center
            for (int j = 0; j < path.Count - 1; j++)
            {
                distanceToPlant += Vector2.Distance(path[j], path[j + 1]);
            }
            float plantPriority = (float)currentPlantCache[i].plantSO.pestAttackPriority;

            // TODO: the function below is subject to modification. Might need a better math model. 
            // current thought: less distance = more weight; more priority = more weight. Most weight plant is the target
            float totalWeight = (1000 / distanceToPlant) + plantPriority;

            if (totalWeight > maxWeight && currentPlantCache[i].attackers < currentPlantCache[i].plantSO.maxAttackers)
            {
                maxWeight = totalWeight;
                targetPlantScript = currentPlantCache[i];
                // fix the two lines below to make line 1 equal to line 2
                availablePosOffsetsOfTarget = availablePosOffsets.Select(o => o + Vector2.up * dimY / 2).ToList(); // convert from center to center bottom.
                                                                                                                   //for(int j=0; j<availablePosOffsets.Count; j++) Debug.DrawLine(center - Vector3.up * dim.y / 2, (center - Vector3.up * dim.y / 2) + ((center + (Vector3)pointOffsets[j]) - (center - Vector3.up * dim.y / 2)), Color.gray, 100, false);
            }
        }

        queryCount++; // curr query finished, move onto next one in queue
        // if queue not empty, then next. This ensures no pather overlap.
        if (coroutineQueue.Count != 0) StartCoroutine(coroutineQueue.Dequeue());
    }

    Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    private void OnPathsComplete(Path p)
    {
        if (p.error)
        {
            Debug.Log("The multipather has an error." + p.errorLog);
            return;
        }

        MultiTargetPath mp = p as MultiTargetPath;
        if (mp == null)
        {
            Debug.LogError("The path was not a multi-target path");
            return;
        }

        // All Paths
        List<Vector3>[] paths = mp.vectorPaths;
        //float maxWeight = 0;

        queryCount = 0;
        expectedQueryCount = paths.Length; // aka # of plants 
        maxWeight = 0;
        for (int i = 0; i < paths.Length; i++)
        {
            List<Vector3> path = paths[i];

            if (path == null || currentPlantCache[i] == null)
            {
                Debug.Log("Path number " + i + " could not be found. Prehaps the plant is already destroyed.");
                queryCount++; // no path, no need to query
                continue;
            }

            // tracking to the offset with the idea that if it can reach the plant regardless of the side...?
            var offset = currentPlantCache[i].plantSO.targetRectParameters[currentPlantCache[i].plantData.currStageOfLife].vec2Array[0];
            var dim = currentPlantCache[i].plantSO.targetRectParameters[currentPlantCache[i].plantData.currStageOfLife].vec2Array[1];
            var center = currentPlantCache[i].transform.position + (Vector3)offset + Vector3.up * dim.y / 2;
            Vector3[] points = new Vector3[36], pointOffsets = new Vector3[36];
            for (int deg = 0; deg < 36; deg++)
            {
                var pointOffsetOnTargetBox = GetThePointOnRectParamByDegree(Vector2.zero, dim, deg * 10); // from center
                var pointOnTargetBox = center + (Vector3)pointOffsetOnTargetBox;
                //Debug.DrawLine(center, pointOnTargetBox, Color.green, 100, false); 
                points[deg] = pointOnTargetBox;
                pointOffsets[deg] = pointOffsetOnTargetBox;
            }

            coroutineQueue.Enqueue(Perform36PtQueryPerPlant(points, center, pointOffsets, path, i, dim.y));

            /*
            // somehow the call below alters points, so I have to use another array (offsets) to track OG data
            MultiTargetPath possPaths = GetComponent<Seeker>().StartMultiTargetPath(transform.position, points, true);
            possPaths.BlockUntilCalculated(); // no callback, but a bit slower, but 36 shouldn't be that much tbh...
            //Debug.Log("possPaths have " + possPaths.vectorPaths.Length);
            List<Vector2> availablePosOffsets = new List<Vector2>();
            for(int j = 0; j < 36; j++)
            {
                var pathToPossPoint = possPaths.vectorPaths[j];
                if (Vector2.Distance(pathToPossPoint[pathToPossPoint.Count - 1], center + pointOffsets[j]) <= attackRange)
                {
                    // this point is reacheable
                    availablePosOffsets.Add(pointOffsets[j]);
                    //Debug.DrawLine(center, center + pointOffsets[j], Color.gray, 100, false);
                    //Debug.DrawLine(pathToPossPoint[pathToPossPoint.Count - 1], center + pointOffsets[j], Color.gray, 100, false);
                }
            }
            if (availablePosOffsets.Count == 0) continue; // plant is unreacheable and thus ignored

            // we could use a shortest path from one of the reacheable point as the distance, but it's not needed
            // because the point picking process is randomized anyway. So let's just uniformaly count from center.
            float distanceToPlant = 0; // to plant's center
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
                // fix the two lines below to make line 1 equal to line 2
                availablePosOffsetsOfTarget = availablePosOffsets.Select(o => o + Vector2.up * dim.y / 2).ToList(); // convert from center to center bottom.
                //for(int j=0; j<availablePosOffsets.Count; j++) Debug.DrawLine(center - Vector3.up * dim.y / 2, (center - Vector3.up * dim.y / 2) + ((center + (Vector3)pointOffsets[j]) - (center - Vector3.up * dim.y / 2)), Color.gray, 100, false);
            } */
        }

        if (coroutineQueue.Count != 0) StartCoroutine(coroutineQueue.Dequeue()); // start the first one

        /*
        if(targetPlantScript == null) // query found nothing suitable, redo
        {
            queryFinished = false;
            queryStarted = false;
        }
        else
        {
            queryFinished = true;
        }*/

    }

    List<PlantScript> currentPlantCache;
    public bool queryStarted = false, queryFinished = false;
    List<Vector2> availablePosOffsetsOfTarget;
    public void SearchForPlant()
    {
        if (!queryStarted) // need to make sure a query is finished first
        {
            targetPlantScript = null;
            availablePosOffsetsOfTarget = null;

            // two methods. 1. since all paths are returned in the order the targets are passed in, we can just do a 
            // multi-pathing query and wait over some frames. Upside: more efficient, downside: can't detect new plant added in between. 
            // 2. we can use blockuntilcalculated to get a path immediately instead of spreading it out over multiple
            // frames, so we can run a for loop with indiv. path dist and weight calculation in one frame. Upside: 
            // more new plant coverage, downside: much slower. Gonna go with approach 1 for now.
            currentPlantCache = FindObjectsOfType<PlantScript>().ToList(); // replace with direct grabbing from plant storage later

            // Searches all paths. Since boolean is set to true, not just returning the shortest one.
            if (currentPlantCache.Count != 0)
            {
                StopAllCoroutines(); // cancel all prev corou
                coroutineQueue.Clear();
                queryCount = 0;
                expectedQueryCount = 69; // set all the searching states back to default.
                GetComponent<Seeker>().StartMultiTargetPath(transform.position,
                    currentPlantCache.Select(p => p.transform.position).ToArray(),
                    true, OnPathsComplete);
                queryStarted = true;
            }
        }

        if (queryCount < expectedQueryCount) // queries unfinished
        {
            //Debug.Log("Dancin'~~~ (idle animation-ing)"); // could play idle animation? might no need.
            return;
        }
        else // all queries finished
        {
            if (targetPlantScript == null) // query found nothing suitable, redo
            {
                queryFinished = false;
                queryStarted = false;
            }
            else
            {
                queryFinished = true;
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

        if (queryFinished && targetPlantScript != null)
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
            /*float perimeter = 2 * dim.x + 2 * dim.y;
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
            }*/
            // Or just grab a point out of all the availble positions. The above one has a flaw, in the drawing notes.
            // because sometimes the point randomly chosen can be an unreacheable area in a passing range. 
            var randCoord = availablePosOffsetsOfTarget[UnityEngine.Random.Range(0, availablePosOffsetsOfTarget.Count)];
            GetComponent<PestMovement>().targetOffsetFromCenter = new Vector2(offset.x + randCoord.x, offset.y + randCoord.y);
            GetComponent<PestMovement>().coreOffsetCache = GetComponent<PestMovement>().targetOffsetFromCenter; // store the data
            //Debug.DrawLine(targetPlantScript.transform.position, targetPlantScript.transform.position + GetComponent<PestMovement>().coreOffsetCache, Color.magenta, 100, false);

            // treat this like a point RELATIVE to the offset, recalculate if in motion. different from main offset
            float castAngle; // in radian
            Vector3 castVector; // the direction vector
            if (GetComponent<PestMovement>().targetOffsetFromCenter.x >= offset.x) // right side
            {
                castAngle = UnityEngine.Random.Range(0, 90) * (Mathf.PI / 180);
                castVector = RotateVector(Vector3.right, castAngle);
            }
            else // left side
            {
                castAngle = UnityEngine.Random.Range(-90, 0) * (Mathf.PI / 180);
                castVector = RotateVector(-Vector3.right, castAngle);
            }
            // Okay this is really weird. I used degree for rotate in Bezier but why is it radian here? I mean here it only works with radian
            // is my Bezier wrong? try with radian later?
            //Debug.DrawLine(targetPlantScript.transform.position + GetComponent<PestMovement>().targetOffsetFromCenter,
            //    targetPlantScript.transform.position + GetComponent<PestMovement>().targetOffsetFromCenter + castVector,
            //    Color.red, 100, false);
            float baseDetectionRange = 1; // TODO: make this generalizable over the longest side of the "?" instead of hard-coded
            float radius = dim.x / 2; // make this size generalizable over what...
            int maxDetectionRange = (int)(baseDetectionRange + attackRange);
            var info = Physics2D.CircleCast(targetPlantScript.transform.position + GetComponent<PestMovement>().targetOffsetFromCenter,
                radius,
                castVector,
                maxDetectionRange,
                (1 << LayerMask.NameToLayer("NonGroundObstacle")) // need to do this, because ground is also an obstacle, and we don't want that. Plus might be more in future.
                                                                  // tip for above, can do (1<<i) | (1<<j) | ...; for multiple layers. 
                );
            //Debug.Log("Was there a collision: " + (info.collider != null));
            //Debug.Log("Name of the collider is: " + info.collider.gameObject.name);
            //Debug.DrawLine(targetPlantScript.transform.position + GetComponent<PestMovement>().targetOffsetFromCenter, info.point, Color.magenta, 100, false);

            Vector2 decoyTarget;
            if (info.collider == null) // no collision on its way
            {
                decoyTarget = PickRandomPointInCircle(targetPlantScript.transform.position +
                    GetComponent<PestMovement>().targetOffsetFromCenter + castVector * maxDetectionRange, // no need to normalize dir. Already 1. 
                    radius);
            }
            else // hit something, info parameters came to life
            {
                decoyTarget = PickRandomPointInCircle(info.centroid, radius);
            }
            //Debug.DrawLine(targetPlantScript.transform.position + GetComponent<PestMovement>().coreOffsetCache, decoyTarget, Color.magenta, 100, false);
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
        if (targetPlantScript == null || targetPlantScript.attackers >= targetPlantScript.plantSO.maxAttackers)
        {
            // handle a weird case where the pest is in state moving and movement script disabled, when the target is destroyed/missing.
            if (GetComponent<PestMovement>().enabled != false)
            {
                GetComponent<PestMovement>().resetPath = true;
                GetComponent<PestMovement>().StopPathing(); // initiates pathing ending 
            }
            else
            {
                SetSearchingState();
            }

            //SearchForPlant(); // wait for callback from the script
        }
    }

    public void StartAttack() // make sure this is only called once. The initialization process
    {
        if (currentState != State.STATE_ATTACKING) // need to do this. Else plant in motion -> multiple end path calls -> perma reset
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
        //Debug.Log("CHASE AFTER PLANT ACTIVATED");
        if (currentState == State.STATE_ATTACKING)
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
                //Debug.Log("Attack animation played");

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
        if (targetPlantScript == null) return false; // destroyed during check

        return Vector3.Distance(transform.position, targetPlantScript.transform.position) <= attackRange;
    }

    void DuringRetreat()
    {
        transform.position = Vector2.MoveTowards(transform.position, retreatPoint, speed * Time.deltaTime);
    }
    #endregion

    public void InitializePestData(Vector2 location)
    {
        pestData = new PestData();
        pestData.location = location;
        pestData.currStageOfLife = 0;
        pestData.pestName = (int)pestSO.pName;
        pestData.currentHealth = pestSO.maxHealth[pestData.currStageOfLife];
        pestData.pestModuleData = new Dictionary<PestModuleEnum, string>(); // size 0. Modules to be added in the child class
        PersistentData.GetLevelData(LevelManager.currentLevelID).pestDatas.Add(pestData); // add this pest into save. 
    }

    private void Start()
    {
        InitializePestData(transform.position); // FOR NOW, no load from save YET unlike the ready plant.
        SpawnInModules(); // FOR NOW.
    }

    public void UpdateAllModules()
    {
        foreach (var module in pestModules.Values)
        {
            module.Update();
        }
    }

    public void AddPestModule(PestModuleEnum module, String dataString = null)
    {
        if (!pestModules.ContainsKey(module))
        { // do we want multiple modules? rework if so.
            var moduleInstance = PestModuleArr.GetModule(module, this);
            if (dataString != null)
            {
                moduleInstance.AssignDataFromString(dataString);
            }
            else
            {
                dataString = moduleInstance.EncodeDataToString();
            }
            pestModules.Add(module, moduleInstance);
            pestData.pestModuleData.Add(module, dataString);
            moduleInstance.OnModuleAdd();
        }
    }

    public void RemovePestModule(PestModuleEnum module)
    {
        if (pestModules.ContainsKey(module)) // do we want multiple modules? rework if so.
        {
            pestModules[module].OnModuleRemove();
            pestModules.Remove(module); // user's responsibility to pause the module? or pause it here. 
            pestData.pestModuleData.Remove(module);
        }
    }

    public void ResumePestModule(PestModuleEnum module)
    {
        pestModules[module].ResumeModule();
    }

    public IPestModule GetPestModule(PestModuleEnum module)
    {
        return pestModules[module];
    }

    // If a pest is new, no modules. if it exists, then load em in!
    public void SpawnInModules()
    {
        // no modules, fresh pest
        if (pestData.pestModuleData.Count == 0)
        {
            // add in the default modules!
            foreach (PestModuleEnum module in pestSO.defaultModules)
            {
                AddPestModule(module);
            }
        }
        else // has modules, spawn in previous pest
        {
            foreach (PestModuleEnum module in pestData.pestModuleData.Keys)
            {
                AddPestModule(module, pestData.pestModuleData[module]);
            }
        }
    }

    public void OnDeath()
    {
        // TODO: properly destroy the whole pest
        Debug.Log("Called OnDeath for PestScript: " + gameObject.name);
        Destroy(gameObject);
    }

    #region MathyStuff
    Vector2 RotateVector(Vector2 v, float theta)
    {
        return new Vector2(
            v.x * Mathf.Cos(theta) - v.y * Mathf.Sin(theta),
            v.x * Mathf.Sin(theta) + v.y * Mathf.Cos(theta)
            );
    }

    Vector2 PickRandomPointInCircle(Vector2 center, float radius)
    {
        var r = radius * Mathf.Sqrt(UnityEngine.Random.Range(0f, 1f));
        var theta = UnityEngine.Random.Range(0f, 1f) * 2 * Mathf.PI;
        var x = center.x + r * Mathf.Cos(theta);
        var y = center.y + r * Mathf.Sin(theta);
        return new Vector2(x, y);
    }

    // input deg is in degree, rect.x is width, rect.y is height
    Vector2 GetThePointOnRectParamByDegree(Vector2 center, Vector2 rect, float deg)
    {
        var twoPI = Mathf.PI * 2;
        var theta = deg * Mathf.PI / 180;

        while (theta < -Mathf.PI)
        {
            theta += twoPI;
        }

        while (theta > Mathf.PI)
        {
            theta -= twoPI;
        }

        var rectAtan = Mathf.Atan2(rect.y, rect.x);
        var tanTheta = Mathf.Tan(theta);
        int region;

        if ((theta > -rectAtan) && (theta <= rectAtan))
        {
            region = 1;
        }
        else if ((theta > rectAtan) && (theta <= (Mathf.PI - rectAtan)))
        {
            region = 2;
        }
        else if ((theta > (Mathf.PI - rectAtan)) || (theta <= -(Mathf.PI - rectAtan)))
        {
            region = 3;
        }
        else
        {
            region = 4;
        }

        Vector2 edgePoint = center; // new Vector2(rect.x / 2, rect.y / 2); //for 0,0
        var xFactor = 1;
        var yFactor = 1;

        switch (region)
        {
            case 1: yFactor = -1; break;
            case 2: yFactor = -1; break;
            case 3: xFactor = -1; break;
            case 4: xFactor = -1; break;
        }

        if ((region == 1) || (region == 3))
        {
            edgePoint.x += xFactor * (rect.x / 2);                                     // "Z0"
            edgePoint.y += yFactor * (rect.x / 2) * tanTheta;
        }
        else
        {
            edgePoint.x += xFactor * (rect.y / (2 * tanTheta));                        // "Z1"
            edgePoint.y += yFactor * (rect.y / 2);
        }

        return edgePoint;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    // The slider that controls the pest's health bar.
    private Slider slider;

    // Pest module Dict. They are separated by function. They are not in the scriptable object because that can't have runtime-changeable data.
    protected Dictionary<PestModuleEnum, IPestModule> pestModules = new Dictionary<PestModuleEnum, IPestModule>();

    public PestModuleArr.IMovementModule currMovementModule;
    public PestModuleEnum currAttackModule;

    public PestData pestData; // contains all the dynamic data of a pest to be saved, a reference to PD 

    public float attackRange = 2f; // 2f for meele-ish // can't sync... trying to create melee but finding ranged on the way effect

    // remove once big timer implemented
    float nextAttackTime;

    Vector2 retreatPoint;

    public State currentState; // put it back to private later. Public for now to debug. 
    public PlantScript targetPlantScript;

    private void Awake()
    {
        currentState = State.STATE_SEARCHING;
        slider = GetComponent<Slider>();
        slider.maxValue = pestSO.maxHealth[pestData.currStageOfLife];
        slider.value = slider.maxValue;
        transform.GetChild(0).GetComponent<Canvas>().worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>(); // Sets the Event Camera for the Canvas component in the Healthbar object so that the healthbar can show up for pests.
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
        if (pestData.currentHealth <= 0){
            OnDeath();
        }
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
            ////Debug.Log("Length of pathToPossPoint is " + pathToPossPoint.Count);
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

    public void switchTargetPlant(PlantScript plantScript){
        if (plantScript.plantData.currStageOfLife != 0){
            targetPlantScript = plantScript;
            ResumePestModule(currAttackModule);
        }
    }

    public void TakeDamage(float damageAmount){
        // update internal health
        pestData.currentHealth -= damageAmount;

        // update health bar for slider
        slider.value = pestData.currentHealth;
    }

    Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    private void OnPathsComplete(Path p)
    {
        if (p.error)
        {
            ////Debug.Log("The multipather has an error." + p.errorLog);
            return;
        }

        MultiTargetPath mp = p as MultiTargetPath;
        if (mp == null)
        {
            ////Debug.LogError("The path was not a multi-target path");
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
                ////Debug.Log("Path number " + i + " could not be found. Prehaps the plant is already destroyed.");
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
        }

        if (coroutineQueue.Count != 0) StartCoroutine(coroutineQueue.Dequeue()); // start the first one
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
            currentPlantCache = FindObjectsOfType<PlantScript>().ToList(); // TODO: replace with direct grabbing from plant storage later!!!

            // Searches all paths. Since boolean is set to true, not just returning the shortest one.
            if (currentPlantCache.Count != 0)
            {
                StopAllCoroutines(); // cancel all prev corou. WARNING: does this cancel other modules too?
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
            ////Debug.Log("Dancin'~~~ (idle animation-ing)"); // could play idle animation? might no need.
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

        if (queryFinished && targetPlantScript != null)
        {
            // TODO: find a location from that plant to target/attack'
            var offset = targetPlantScript.plantSO.targetRectParameters[targetPlantScript.plantData.currStageOfLife].vec2Array[0];
            var dim = targetPlantScript.plantSO.targetRectParameters[targetPlantScript.plantData.currStageOfLife].vec2Array[1];
            // Or just grab a point out of all the availble positions. The above one has a flaw, in the drawing notes.
            // because sometimes the point randomly chosen can be an unreacheable area in a passing range. 
            var randCoord = availablePosOffsetsOfTarget[UnityEngine.Random.Range(0, availablePosOffsetsOfTarget.Count)];
            currMovementModule.targetOffsetFromCenter = new Vector2(offset.x + randCoord.x, offset.y + randCoord.y);
            currMovementModule.coreOffsetCache = currMovementModule.targetOffsetFromCenter; // store the data
            //Debug.DrawLine(targetPlantScript.transform.position, targetPlantScript.transform.position + GetComponent<PestMovement>().coreOffsetCache, Color.magenta, 100, false);

            // treat this like a point RELATIVE to the offset, recalculate if in motion. different from main offset
            float castAngle; // in radian
            Vector3 castVector; // the direction vector
            if (currMovementModule.targetOffsetFromCenter.x >= offset.x) // right side
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
            var info = Physics2D.CircleCast(targetPlantScript.transform.position + currMovementModule.targetOffsetFromCenter,
                radius,
                castVector,
                maxDetectionRange,
                (1 << LayerMask.NameToLayer("Obstacle")) // need to do this, because ground is also an obstacle, and we don't want that. Plus might be more in future.
                                                                  // tip for above, can do (1<<i) | (1<<j) | ...; for multiple layers. 
                );

            Vector2 decoyTarget;
            if (info.collider == null) // no collision on its way
            {
                decoyTarget = PickRandomPointInCircle(targetPlantScript.transform.position +
                    currMovementModule.targetOffsetFromCenter + castVector * maxDetectionRange, // no need to normalize dir. Already 1. 
                    radius);
            }
            else // hit something, info parameters came to life
            {
                decoyTarget = PickRandomPointInCircle(info.centroid, radius);
            }
            //Debug.DrawLine(targetPlantScript.transform.position + GetComponent<PestMovement>().coreOffsetCache, decoyTarget, Color.magenta, 100, false);
            // Finally, do something with decoyTarget!
            var decoyTargetOffsetFromCenter = (Vector3)decoyTarget - targetPlantScript.transform.position;
            currMovementModule.decoyState = true;
            currMovementModule.targetOffsetFromCenter = decoyTargetOffsetFromCenter; // can do this, for example. Use boolean "beentodecoyyet"

            // initiates movement script
            currMovementModule.targetPosition = targetPlantScript.transform;
            // GetComponent<PestMovement>().enabled = true;
            ((IPestModule)currMovementModule).ResumeModule();
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
        // if target plant dies or max pest or the plant is still a seed
        if (targetPlantScript == null || targetPlantScript.attackers >= targetPlantScript.plantSO.maxAttackers || targetPlantScript.plantData.currStageOfLife == 0)
        {
            // handle a weird case where the pest is in state moving and movement script disabled, when the target is destroyed/missing.
            // if (GetComponent<PestMovement>().enabled != false)
            if (currMovementModule.keepPathing == true) // ????????? hmmmmmmmMMMMMMMMM
            {
                currMovementModule.resetPath = true;
                currMovementModule.keepPathing = false; // initiates pathing ending 
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
            // nextAttackTime = Time.time + attackRate;

            ResumePestModule(currAttackModule);
        }
    }

    // this is to deal with the case where a stationary plant already being attacked is being moved
    public void ChaseAfterPlant()
    {
        ////Debug.Log("CHASE AFTER PLANT ACTIVATED");
        if (currentState == State.STATE_ATTACKING)
        {
            // GetComponent<PestMovement>().enabled = true;
            ((IPestModule)currMovementModule).ResumeModule();
        }
    }

    void DuringAttack()
    {
        // check if plant dies, if so call SearchForPlant()
        if (targetPlantScript == null)
        {
            currentState = State.STATE_SEARCHING;
            PausePestModule(currAttackModule);
            return;
        }

        // should use big timer once implemented
        // Thought here:
        // As long as pest's AA off cd, it will attack the plant even if you hold it and run past it in range.
        // so this adds a bit of mecahnics yay
        /*if (Time.time > nextAttackTime) // the attack is ready
        {
            // reduce plant health if in attack range. Otherwise no.
            if (TargetPlantInAttackRange())
            {
                // TODO: play attacking animation here
                ////Debug.Log("Attack animation played");

                targetPlantScript.TakeDamage((int)attackDamage);
                ////Debug.Log("Attacking target plant, hp left: " + targetPlantScript.plantData.currentHealth);

                nextAttackTime = Time.time + attackRate; // reset aa timer
            }
        }*/

        // TODO: figure out when should enter retreat state
        // set retreatPoint to corner of camera OR when we implement level bounds, to outside level bounds
        // if setting to outside level bounds, can be done when initialized at the top of the script
    }

    public bool TargetPlantInAttackRange()
    {
        if (targetPlantScript == null) return false; // destroyed during check

        // Some pretty smart "line of sight" attempt. TODO: IT WOULD WORK! TRY LATER AFTER FINISHING PROJ
        int obstacleLayer = 1 << LayerMask.NameToLayer("Obstacle"); // move them out. BitSHift is expensive. TODO...
        int groundLayer = 1 << LayerMask.NameToLayer("Ground");
        int plantLayer = 1 << LayerMask.NameToLayer("Plant");
        int combinedLayerMask = obstacleLayer | plantLayer | groundLayer;
        Vector2 rayDirection = (targetPlantScript.transform.position + currMovementModule.coreOffsetCache) - transform.position;
        // RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, rayDirection, 1000, combinedLayerMask);
        // Cast a circle from the current position in the direction of the target, ignoring all layers except Obstacle and Plant
        float radius = 0.2f; // width of your "ray"... makeShift. TODO: projectile size comm?
        float attackRange = 100f; // makeShift.. TODO: try to find a way to use SO data.
        // Single cast has problem: another plant blocking the ray from reaching the target plant. So has to track all
        /*RaycastHit2D hit = Physics2D.CircleCast(transform.position, radius, rayDirection, attackRange, combinedLayerMask);
        // //Debug.Log(hit.collider.gameObject);
        if (hit.collider != null)
        {
            if (hit.collider.gameObject == targetPlantScript.gameObject)
            {
                ////Debug.Log("Found target plant in sight!!!");
                return true;
            }
        }*/
        // So we need to check everything.
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, radius, rayDirection, attackRange, combinedLayerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject == targetPlantScript.gameObject)
            {
                ////Debug.Log("Found target plant in sight!!!");
                return true;
            }
            // If the hit is an obstacle, then the target plant is not in sight. Objects are returned in order of contact.
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||
                    hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                break;
            }
        }

        return false;

        // return Vector3.Distance(transform.position, targetPlantScript.transform.position) <= attackRange;
    }

    void DuringRetreat()
    {
        var retreatSpeed = 5;
        transform.position = Vector2.MoveTowards(transform.position, retreatPoint, retreatSpeed * Time.deltaTime);
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

    public void PausePestModule(PestModuleEnum module)
    {
        pestModules[module].PauseModule();
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

    public void DestroyForYou(GameObject gameObject)
    {
        Destroy(gameObject);
    }

    public void OnDeath()
    {
        // TODO: properly destroy the whole pest
        ////Debug.Log("Called OnDeath for PestScript: " + gameObject.name);
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

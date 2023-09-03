using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Pathfinding;

public enum PestModuleEnum // serialized names of each of the modules
{
    SingleTargetProjectileAttack,
    BezierMovement
}

// Pest module interfaces (can be customized to include new functions)
// Pest modules define a single behavior of a pest type.
public interface IPestModule
{
    void Update();
    void OnModuleAdd();
    void OnModuleRemove();
    void PauseModule();
    void ResumeModule();
    void AssignDataFromString(String dataString);
    String EncodeDataToString();
}

public static class PestModuleArr
{
    static Dictionary<PestModuleEnum, Func<PestScript, IPestModule>> moduleConstructors = new Dictionary<PestModuleEnum, Func<PestScript, IPestModule>>
    {
      {PestModuleEnum.SingleTargetProjectileAttack, (pestScript) => new SingleTargetProjectileAttackModule(pestScript)},
      {PestModuleEnum.BezierMovement, (pestScript) => new BezierMovementModule(pestScript)},
    };

    // returns a new instance of the targetted pestModule 
    public static IPestModule GetModule(PestModuleEnum module, PestScript pestScript)
    {
        return moduleConstructors[module].Invoke(pestScript);
    }

    #region ModuleStructureHierarchy
    // Modules
    public abstract class StatefulPestModule<ModuleData> : IPestModule
    {
        public ModuleData moduleData;
        protected PestScript pestScript;
        public virtual String EncodeDataToString()
        {
            return JsonUtility.ToJson(moduleData);
        }
        public virtual void AssignDataFromString(String dataString)
        {
            moduleData = JsonUtility.FromJson<ModuleData>(dataString);
        }
        public virtual void Update() { }
        public virtual void OnModuleAdd() { }
        public virtual void OnModuleRemove() { }
        public virtual void PauseModule() { }
        public virtual void ResumeModule() { }
    }

    // COPIED FROM PLANTMODULES!!!
    /// <summary>
    /// Main Inheritable Module #1: TimerModule
    /// </summary>
    [System.Serializable]
    public class TimerModuleData
    {
        public float timePerCycle;
        public float timeInCurrentCycleSoFar; // for time tracking and data storage
    }
    public class TimerModule<T> : StatefulPestModule<T> where T : TimerModuleData
    {
        public TimerModule(PestScript pestScript)
        {
            this.pestScript = pestScript;
        }

        float timeAnchor = 0f;

        public override void OnModuleAdd()
        {
            timeAnchor = Time.time; // controlled by timeScale
        }

        // Going to use update over coroutine for 1) centralization 2) no reason for module pausing.
        public override void Update()
        {
            float currTime = Time.time;
            float timeElapsed = currTime - timeAnchor;
            moduleData.timeInCurrentCycleSoFar += timeElapsed;
            timeAnchor = currTime;

            if (moduleData.timeInCurrentCycleSoFar >= moduleData.timePerCycle)
            {
                if (OnCycleCompleteEvent != null) OnCycleCompleteEvent?.Invoke(); // Trigger event
                else OnCycleComplete(); // Trigger child's overwrite
                moduleData.timeInCurrentCycleSoFar = 0f; // Resets the cycle timer.
            }
        }

        public event Action OnCycleCompleteEvent = null; // event version for compositive modules
        public virtual void OnCycleComplete() { } // This is an empty method in the parent class, but can be overridden in child classes.

        public virtual void OnGrowthPause()
        {
            moduleData.timeInCurrentCycleSoFar += Time.time - timeAnchor; // add the time so far
        }

        public virtual void OnGrowthResume() // Suppose this line is called before "update" happens at resume.
        {
            timeAnchor = Time.time; // reset the anchor
        }

        protected void ResetTimer()
        {
            timeAnchor = 0f;
            moduleData.timeInCurrentCycleSoFar = 0f;
        }
    }

    /// <summary>
    /// Main Inheritable Module #2: TriggerModule
    /// </summary>
    [System.Serializable]
    public class TriggerModuleData
    {

    }
    public class TriggerModule<T> : StatefulPestModule<T> where T : TriggerModuleData
    {
        public TriggerModule(PestScript pestScript)
        {
            this.pestScript = pestScript;
        }

        public TriggerResponse colliderScript;

        public override void OnModuleAdd()
        {
        }

        protected virtual void OnTriggerEnter2D(Collider2D collider)
        {
            Debug.Log("OnTriggerEnter2D called for TriggerModule. gameObject: " + collider.gameObject.name);
        }

        protected virtual void OnTriggerExit2D(Collider2D collider)
        {
            Debug.Log("OnTriggerExit2D called for TriggerModule. gameObject: " + collider.gameObject.name);
        }
    }

    /// <summary>
    /// Main Inheritable Module #3: TriggerAndTimerModule (created via composition of the previous 2)
    /// </summary>
    [System.Serializable]
    public class TriggerAndTimerModuleData
    {
        public TriggerModuleData triggerData;
        public TimerModuleData timerData;
    }
    public class TriggerAndTimerModule<T> : StatefulPestModule<T> where T : TriggerAndTimerModuleData
    {
        protected TriggerModule<TriggerModuleData> triggerModule;
        protected TimerModule<TimerModuleData> timerModule;

        public TriggerAndTimerModule(PestScript pestScript)
        {
            this.pestScript = pestScript;

            // Instantiate the helper modules and assign their properties
            triggerModule = new TriggerModule<TriggerModuleData>(pestScript);
            timerModule = new TimerModule<TimerModuleData>(pestScript);
            timerModule.OnCycleCompleteEvent += OnCycleComplete; // Subscribe to event
        }

        protected virtual void OnCycleComplete() { }

        public override void OnModuleAdd()
        {
            triggerModule.OnModuleAdd();
            timerModule.OnModuleAdd();
        }

        public override void Update()
        {
            triggerModule.Update();
            timerModule.Update();
        }

        // Add other override methods as needed, and delegate to the appropriate module
    }

    /// <summary>
    /// Main Inheritable Module #4: Movement
    /// </summary>
    // Integrated from PestMovement. Read it for the commentaries
    [System.Serializable]
    public class MovementModuleData
    {
        public float speed;
    }
    public interface IMovementModule // a hack that allows parent var-style access
    {
        Vector3 targetOffsetFromCenter { get; set; }
        Vector3 coreOffsetCache { get; set; }
        bool decoyState { get; set; }
        Transform targetPosition { get; set; }
        bool resetPath { get; set; }
        bool keepPathing { get; set; }
    }
    public class MovementModule<T> : StatefulPestModule<T>, IMovementModule where T : MovementModuleData
    {
        public Transform targetPosition { get; set; }

        public Vector3 targetOffsetFromCenter { get; set; } // stores the current offset from center of the spot pest is going to (used for decoy and actual)

        public Vector3 coreOffsetCache { get; set; } // stores the position off from center of the spot on plant the pest gonna attack

        protected Seeker seeker;

        protected Path path;

        protected Transform enemyGraphics;

        protected float nextWaypointDistance = 1; // smaller the more accurate; the distance that decides where you have reached a point

        protected int currentWaypoint;

        // protected bool reachedEndOfPath;

        public bool keepPathing { get; set; } // true to activate perma pathing, false to keep the path one-time

        public bool resetPath { get; set; }

        public bool decoyState { get; set; } // the pest is going to a secondary place first, then go to the plant. For diversity purposes.

        protected int consecUnreacheableCounter = 0;

        protected int minAttemptUnreacheable = 10; // basically how many times the pest will try before giving up

        public MovementModule(PestScript pestScript)
        {
            this.pestScript = pestScript;
        }

        public override void OnModuleAdd()
        {
            seeker = pestScript.gameObject.GetComponent<Seeker>();
            enemyGraphics = pestScript.GetComponentInChildren<SpriteRenderer>().transform; // TODO: USE DIRECT ASSIGNMENT; NOT EFFICIENT
            PauseModule(); // starts off paused
        }

        public override void PauseModule() // TODO: might not be enough...
        {
            path = null;
            keepPathing = false; // hmmmMMMMMMMMMm
        }

        public override void ResumeModule()
        {
            currentWaypoint = 0;
            keepPathing = true; // if this is set to false, then pest won't move at start until it's true.
            resetPath = false;
            // reachedEndOfPath = false;
            UpdatePath();
        }

        protected void UpdatePath()
        {
            if (seeker.IsDone() && targetPosition != null)
                seeker.StartPath(pestScript.transform.position, (targetPosition.position + targetOffsetFromCenter), OnPathComplete);
        }

        private void OnPathComplete(Path p)
        {
            if (targetPosition != null) targetPosition.GetComponent<PlantScript>().VisualizePlantTargetBoundary(); // for debugging. Comment out later
            if (!p.error)
            {
                path = p;
                // Reset the waypoint counter so that we start to move towards the first point in the path
                currentWaypoint = 0;
            }
        }

        protected void EndPathing(bool turnOffPathing)
        {
            if (resetPath) // pathing ended because it needs a reset
            {
                pestScript.SetSearchingState();
            }
            else if (decoyState) // pathing has finished the decoy part, time for real part
            {
                decoyState = false;
                targetOffsetFromCenter = coreOffsetCache;
            }
            else // pathing ended because acutally reached the target; 
            {
                pestScript.StartAttack();
            }

            if (turnOffPathing)
            {
                // fix the orientation once in the end
                if (targetPosition != null)
                {
                    if (pestScript.transform.position.x < targetPosition.position.x) enemyGraphics.localScale = new Vector3(1f, 1f, 1f);
                    else if (pestScript.transform.position.x > targetPosition.position.x) enemyGraphics.localScale = new Vector3(-1f, 1f, 1f);
                }
                // else if want to rotate whenever pass plant x mid line, remove offset to relativePosition part in movements?

                PauseModule(); // turns off the movement script (itself)
            }
        }

        // Getters and setters for parent to comm.
        
    }
    #endregion

    /////////////////////////////////// ACTUAL MODULE IMPLEMENTATIONS BEGINS HERE ///////////////////////////////////////
    // Integrated from BezierPattern. Read it for the commentaries
    public class BezierMovementModuleData : MovementModuleData
    {

    }
    public class BezierMovementModule : MovementModule<BezierMovementModuleData>
    {
        // Bezier Experimental
        private Vector2 p0, p1, p2, p3;

        private float t = 1.1f;

        private Queue<Vector2> pathDivisions;

        private float minSegDist = 0.5f; // width of each curve

        private float curveExtrusionFactor = 0.5f; // outward extrudedness of each curve

        private int uniformalBezDegree = 90; // degree of uniformal inward / outward extrusion

        private int alternatingFactor = 1;

        private float relativePosition;

        private float slowdownDetectionRange = 3; // range, in radius, of slowdown activation. Set to 0 for no slowdown.

        public BezierMovementModule(PestScript pestScript) : base(pestScript)
        {
            moduleData = new BezierMovementModuleData()
            {
                speed = pestScript.pestSO.bezierMoveSpeed[pestScript.pestData.currStageOfLife],
            };
        }

        public override void OnModuleAdd()
        {
            pestScript.currMovementModule = this;
            base.OnModuleAdd();
        }

        public override void ResumeModule()
        {
            pestScript.currMovementModule = this;

            pathDivisions = new Queue<Vector2>();
            relativePosition = Mathf.Sign(pestScript.transform.position.x - (targetPosition.transform.position.x + targetOffsetFromCenter.x)); // + targetOffsetFromCenter.x to make it follow through dir first

            base.ResumeModule();
        }

        public override void Update()
        {
            if (path == null)
            {
                // We have no path to follow yet, so don't do anything
                return;
            }

            if (t > 1 && pathDivisions.Count == 0 && keepPathing) // not during a movement pattern or sub pathing
            {
                // Check in a loop if we are close enough to the current waypoint to switch to the next one.
                // We do this in a loop because many waypoints might be close to each other and we may reach
                // several of them in the same frame.
                float distanceToWaypoint;
                // reachedEndOfPath = false;
                // The distance to the next waypoint in the path
                while (true)
                {
                    // If you want maximum performance you can check the squared distance instead to get rid of a
                    // square root calculation. But that is outside the scope of this tutorial.
                    distanceToWaypoint = Vector3.Distance(pestScript.transform.position, path.vectorPath[currentWaypoint]);
                    if (distanceToWaypoint < nextWaypointDistance)
                    {
                        // Check if there is another waypoint or if we have reached the end of the path
                        if (currentWaypoint + 1 < path.vectorPath.Count)
                        {
                            currentWaypoint++;
                        }
                        else
                        {
                            // Set a status variable to indicate that the agent has reached the end of the path.
                            // You can use this to trigger some special code if your game requires that.
                            // reachedEndOfPath = true;

                            // are you trapped trying to reach the unreacheable?
                            if (targetPosition != null &&
                                Vector2.Distance(path.vectorPath[currentWaypoint], targetPosition.position + targetOffsetFromCenter)
                                > pestScript.attackRange)
                            {
                                consecUnreacheableCounter++;
                            }
                            else
                            {
                                consecUnreacheableCounter = 0;
                            }

                            // if target is stationary and no more movement etc, then keepPathing = false.
                            // else the pathing should continue
                            // Here we assume that the target is reached within attack range, so...
                            if (pestScript.TargetPlantInAttackRange()) // need to make sure this path is in range one
                            {
                                decoyState = false; // Mandatory overwrite. Exit decoy state if found good angle.
                                // targetOffsetFromCenter = coreOffsetCache; // Don't forget to find the target spot
                                if (!pestScript.targetPlantScript.inMotion) keepPathing = false; // naturally into EndPathing(true) later.
                                else EndPathing(false); // pest is still pathing / aka chasing the plant, but also attacking.
                            }
                            else if (targetPosition == null || consecUnreacheableCounter >= minAttemptUnreacheable) // new target time
                            {
                                resetPath = true;
                                keepPathing = false;
                                Debug.Log("Potentially idleling");
                                //enabled = false; // target destroyed. Better to set to idle behavior here while calculating/waiting new target
                            }
                            else if (decoyState)
                            {
                                EndPathing(false);
                            }

                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                //Debug.Log("After while loop: current waypoint: " + currentWaypoint + ", and total waypoints are: " + path.vectorPath.Count);
            }

            if (t <= 1) // movement if during movement pattern phase
            {
                Vector3 newLoc = CalculateCubicBezierPoint(t, p0, p1, p2, p3);
                Vector3 dir = (newLoc - pestScript.transform.position).normalized;
                if (Vector3.Distance(newLoc, pestScript.transform.position) <= 0.01)
                {
                    t += 0.1f;
                }
                else
                {
                    // Slow down smoothly upon approaching the end of the path
                    // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
                    // Optional line. 
                    var speedFactor = 1f; // hopefully doesn't cause weird speed behaviors
                    if (targetPosition != null) // just in case a target is destroyed during movement
                    {
                        float distToTarget = Vector3.Distance(pestScript.transform.position, (targetPosition.position + targetOffsetFromCenter));
                        if (distToTarget <= slowdownDetectionRange && !decoyState)
                        {
                            //Debug.Log("Dist to target is: " + distToTarget);
                            speedFactor = Mathf.Sqrt(distToTarget / slowdownDetectionRange);
                        }
                    }

                    pestScript.transform.position += dir * moduleData.speed * Time.deltaTime * speedFactor;
                }
            }
            else // if t > 1 and keepPathing (usually true, only false when the target is stationary, set at reaching. 
            {
                if (!keepPathing) // remember this can also be set manually
                {
                    // This basically means you've reached a set destination to the target.
                    // Call other functions etc
                    // if KeepPathing stays true, then the ai following target continues as target moves. 
                    //Debug.Log("I shall stop HERE.");
                    EndPathing(true);
                    return;
                }

                UpdatePath(); // update the path at the end of every path division.

                //Debug.Log("t > 1. Check: pathDivisionsQueueCount: " + pathDivisions.Count + " and not reachedEndOfPath: " + !reachedEndOfPath);
                if (pathDivisions.Count == 0) // generate a new path division
                {
                    // Problem with this is that the AI cuts through curves based on the alg since direct lerping.
                    // which makes it weird since the AI passes through obstacles.
                    // I don't think this is the problem. It's a problem with currWayPoint that it jumps to the end.
                    p0 = pestScript.transform.position;
                    p3 = path.vectorPath[currentWaypoint];
                    float lerpRatio = Vector3.Distance(p0, p3) / minSegDist;
                    int numDivisions = (lerpRatio <= 1) ? 1 : (int)lerpRatio;
                    for (int i = 1; i <= numDivisions; i++) // ignore 0 for smoothness
                    {
                        // Divides a segment into multiple instances similar in length and store them. 
                        pathDivisions.Enqueue(Vector2.Lerp(p0, p3, i / (float)numDivisions)); // 0, 1/n, 2/n, ..., 1
                    }

                    float direction = Mathf.Sign(p3.x - p0.x);
                    if (direction >= 0.01f) // velocity enemy desires to travel with based on the current path
                    {
                        enemyGraphics.localScale = new Vector3(1f, 1f, 1f); // if do velocity.x, then has delay because acts with velocity
                    }
                    else if (direction <= -0.01f)
                    {
                        enemyGraphics.localScale = new Vector3(-1f, 1f, 1f);
                    }

                    // this is the make sure sign smoothness
                    // we know x can't be negative. if target is left of obj, then ...
                    // Place this at a reachable spot
                    if (targetPosition != null) // just in case target is destroyed during this process
                    {
                        float newRelativePosition = Mathf.Sign(pestScript.transform.position.x - (targetPosition.transform.position.x + targetOffsetFromCenter.x));
                        //Debug.Log("ARE THE RELATIVE POSITIONS DIFFERENT: " + (newRelativePosition != relativePosition));
                        if (newRelativePosition != relativePosition)
                        {
                            relativePosition = newRelativePosition;
                            alternatingFactor *= -1;
                        }
                    }
                }

                // go to the next pt in the path division. 
                t = 0;
                p0 = pestScript.transform.position;
                p3 = pathDivisions.Dequeue();
                Vector2 rot0 = (p0 - p3).normalized / (1 / curveExtrusionFactor); //minSegDist); // / by int to change length
                Vector2 rot3 = (p3 - p0).normalized / (1 / curveExtrusionFactor);
                p1 = p0 + RotateVector(rot0, uniformalBezDegree * alternatingFactor);
                p2 = p3 + RotateVector(rot3, -uniformalBezDegree * alternatingFactor);
                alternatingFactor *= -1;
            }
        }

        // t is [0, 1], position on the curve, interpolating from p0 to p3
        // p0 is start, p3 is end.
        // p1 is normalized vector rotated in some direction,
        // same as p2, towards same direction as p3.
        Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }

        Vector2 RotateVector(Vector2 v, float theta)
        {
            return new Vector2(
                v.x * Mathf.Cos(theta) - v.y * Mathf.Sin(theta),
                v.x * Mathf.Sin(theta) + v.y * Mathf.Cos(theta)
                );
        }
    }


    [System.Serializable]
    public class SingleTargetProjectileAttackModuleData : TimerModuleData
    {
        public float damageAmount;
        public float damageRangeRadius;
        public float projectileSpeed;
        public bool isOn; // only shoot if the pest stops and in range. (Assume so)
    }
    public class SingleTargetProjectileAttackModule : TimerModule<SingleTargetProjectileAttackModuleData>
    {
        public SingleTargetProjectileAttackModule(PestScript pestScript) : base(pestScript)
        {
            // load from default, presumably assume that this happens before retrieving from data.
            moduleData = new SingleTargetProjectileAttackModuleData
            {
                damageAmount = pestScript.pestSO.singleTargetProjectileDamageAmount[pestScript.pestData.currStageOfLife],
                damageRangeRadius = pestScript.pestSO.singleTargetProjectileAttackRangeRadius[pestScript.pestData.currStageOfLife],
                projectileSpeed = pestScript.pestSO.singleTargetProjectileSpeed[pestScript.pestData.currStageOfLife],
                timePerCycle = pestScript.pestSO.singleTargetProjectileAttackRate[pestScript.pestData.currStageOfLife], // shootRate
                timeInCurrentCycleSoFar = 0f,
                isOn = false
            };
        }

        public override void OnModuleAdd()
        {
            base.OnModuleAdd();

            pestScript.currAttackModule = PestModuleEnum.SingleTargetProjectileAttack;
        }

        public override void PauseModule() // turn off attack
        {
            ResetTimer(); // resets attack cd
            moduleData.isOn = false;
        }

        public override void ResumeModule() // turn on Attack
        {
            moduleData.isOn = true;
        }

        public override void Update()
        {
            if (moduleData.isOn)
            {
                base.Update(); // Timer is only updated if attacking
            }
        }

        public override void OnCycleComplete()
        {
            // Launch projectile
            if (pestScript.targetPlantScript != null && pestScript.TargetPlantInAttackRange())
            {
                Debug.Log("Projectile being generated...");
                if (pestScript.pestSO.pName == PestName.EvilRoach || pestScript.pestSO.pName == PestName.ArmoredSkeeto){
                    AudioManager.GetSFX("bugProjectileSFX").Play();
                    TriggerProjectile bullet = UtilPrefabStorage.Instance.InstantiatePrefab(UtilPrefabStorage.Instance.boxProjectile,
                        pestScript.transform.position, Quaternion.identity, null).GetComponent<TriggerProjectile>();
                    bullet.gameObject.name = "bullet";
                    var direction = (pestScript.targetPlantScript.transform.position + pestScript.currMovementModule.coreOffsetCache) - pestScript.transform.position;
                    bullet.SetProjectileStats(2, direction, OnBulletHit2D, OnBulletExit2D);
                }
                if (pestScript.pestSO.pName == PestName.FireballBee){
                    AudioManager.GetSFX("fireballSFX").Play();
                    TriggerProjectile fireball = UtilPrefabStorage.Instance.InstantiatePrefab(UtilPrefabStorage.Instance.fireballProjectile,
                        pestScript.transform.position, Quaternion.identity, null).GetComponent<TriggerProjectile>();
                    fireball.gameObject.name = "fireball";
                    var direction = (pestScript.targetPlantScript.transform.position + pestScript.currMovementModule.coreOffsetCache) - pestScript.transform.position;
                    fireball.SetProjectileStats(2, direction, OnBulletHit2D, OnBulletExit2D);
                }
            }
            // else
            // {
            //    Debug.Log(pestScript.name + " Not in Range: " + pestScript.targetPlantScript.transform.position);
            // }
        }

        void OnBulletHit2D(Collider2D collider, TriggerProjectile bullet)
        {
            Debug.Log(collider.gameObject.name);
            if (collider.gameObject != null)
            {
                if (pestScript.targetPlantScript != null && collider.gameObject == pestScript.targetPlantScript.gameObject)
                {
                    pestScript.targetPlantScript.TakeDamage(50);
                    pestScript.DestroyForYou(bullet.gameObject);
                }
                else if (collider.gameObject.tag == "Obstacle" || collider.gameObject.tag == "Ground")
                {
                    pestScript.DestroyForYou(bullet.gameObject);
                }
            }
        }

        void OnBulletExit2D(Collider2D collider)
        {

        }
    }

}
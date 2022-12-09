using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    // rigidBody2D is optional. can use forced movements. 

    /*
    public Transform target;

    public float speed = 200f;
    public float nextWayPointDistance = 3f;

    public Transform enemyGraphics;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("UpdatePath", 0f, 0.5f); //insta call it then repeat every 0.5s
    }

    void UpdatePath()
    {
        if (seeker.IsDone())
            seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (path == null) {
            return;
        }

        if(currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        } 
        else
        {
            reachedEndOfPath = false;
        }

        // Vector from our position to the current waypoint, normalize it to set length to 1
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;
        
        rb.AddForce(force); // add linear drag / air resistence to slow it down

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        
        if (distance < nextWayPointDistance)
        {
            currentWaypoint++;
        }

        if (force.x >= 0.01f) // velocity enemy desires to travel with based on the current path
        {
            enemyGraphics.localScale = new Vector3(1f, 1f, 1f); // if do velocity.x, then has delay because acts with velocity
        }
        else if (force.x <= -0.01f)
        {
            enemyGraphics.localScale = new Vector3(-1f, 1f, 1f);
        }
    }*/

    // These values are in the inspector! and they overwrite the ones set in here!

    public Transform targetPosition;

    private Seeker seeker;

    public Path path;

    public Transform enemyGraphics;

    public float speed;

    public float nextWaypointDistance; // smaller the more accurate

    private int currentWaypoint = 0;

    private bool reachedEndOfPath;

    // Bezier Experimental
    private Vector2 p0, p1, p2, p3;
    private float t = 1.1f;
    private Queue<Vector2> pathDivisions;
    public float minSegDist = 0.5f; // 1f
    public int uniformalBezDegree = 90;
    private int alternatingFactor = 1;
    private float relativePosition;
    public bool keepPathing = true;

    //public GameObject testPrefab;

    public void Start()
    {
        seeker = GetComponent<Seeker>();
        pathDivisions = new Queue<Vector2>();
        relativePosition = Mathf.Sign(transform.position.x - targetPosition.transform.position.x);

        UpdatePath();
    }

    void UpdatePath()
    {
        // Create a new path object, the last parameter is a callback function
        // but it will be used internally by the seeker, so we will set it to null here
        // Paths are created using the static Construct call because then it can use
        // pooled paths instead of creating a new path object all the time
        // which is a nice way to avoid frequent GC spikes.
        //var p = ABPath.Construct(transform.position, transform.position + transform.forward * 10, null);
        //var p = RandomPath.Construct(transform.position, transform.position + transform.forward * 10, null);

        // By default, a search for the closest walkable nodes to the start and end nodes will be carried out
        // but for example in a turn based game, you might not want it to search for the closest walkable node, but return an error if the target point
        // was at an unwalkable node. Setting the NNConstraint to None will disable the nearest walkable node search
        //p.nnConstraint = NNConstraint.None;

        if (seeker.IsDone())
            // Start a new path to the targetPosition, call the the OnPathComplete function
            // when the path has been calculated (which may take a few frames depending on the complexity)
            seeker.StartPath(transform.position, targetPosition.position, OnPathComplete);
    }

    public void OnPathComplete(Path p)
    {
        Debug.Log("A path was calculated. Did it fail with an error? " + p.error);

        if (!p.error)
        {
            path = p;
            // Reset the waypoint counter so that we start to move towards the first point in the path
            currentWaypoint = 0;

            // this is used to visualize vector points
            /*for(int i=0; i<path.vectorPath.Count; i++)
            {
                var obj = Instantiate(testPrefab, path.vectorPath[i], Quaternion.identity);
                obj.transform.localScale = Vector3.one / 10;
                obj.GetComponent<SpriteRenderer>().color = Color.green;
            }*/
        }
    }

    public void Update()
    {
        if (path == null)
        {
            // We have no path to follow yet, so don't do anything
            Debug.Log("NO PATH");
            return;
        }

        // Don't place the seg here to avoid multiple calls during bezier curve execution. 
        if (t > 1 && pathDivisions.Count == 0 && keepPathing) // not during a movement pattern or sub pathing
        {
            // Check in a loop if we are close enough to the current waypoint to switch to the next one.
            // We do this in a loop because many waypoints might be close to each other and we may reach
            // several of them in the same frame.
            float distanceToWaypoint;
            reachedEndOfPath = false;
            Debug.Log("Before while loop: current waypoint: " + currentWaypoint + ", and total waypoints are: " + path.vectorPath.Count);
            // The distance to the next waypoint in the path
            while (true)
            {
                // If you want maximum performance you can check the squared distance instead to get rid of a
                // square root calculation. But that is outside the scope of this tutorial.
                distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
                if (distanceToWaypoint < nextWaypointDistance) // && t >= 1)
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
                        reachedEndOfPath = true;

                        Debug.Log("Distance to waypoint is: " + distanceToWaypoint);
                        Debug.Log("END OF PATH REACHED");

                        // if target is stationary and no more movement etc, then keepPathing = false.
                        // else call UpdatePath() and keep var true. 
                        //keepPathing = false;

                        // The problem with calling UpdatePath here, aka at the end of every seg, is that
                        // there could be a frame-delay between two different paths so the pest spins a
                        // circle again on the previous n-1 waypoint path. So it's better to call invoker
                        // to update seeker path continuously.
                        //UpdatePath(); // comment this out to make it path once only. 

                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            Debug.Log("After while loop: current waypoint: " + currentWaypoint + ", and total waypoints are: " + path.vectorPath.Count);
        }

        if (t <= 1) // movement if during movement pattern phase
        {
            Vector3 newLoc = CalculateCubicBezierPoint(t, p0, p1, p2, p3);
            Vector3 dir = (newLoc - transform.position).normalized;
            //Debug.Log(newLoc);
            if (Vector3.Distance(newLoc, transform.position) <= 0.01)
            {
                t += 0.1f;
                //Debug.Log("Small pt reached");
            }
            else
            {
                // Slow down smoothly upon approaching the end of the path
                // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
                // Optional line. 
                var speedFactor = 1f;
                /*if(reachedEndOfPath)
                {
                    Debug.Log("REACHED END OF PATH TRIGGERED");
                    var distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
                    speedFactor = Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance);
                }*/
                //var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

                transform.position += dir * speed * Time.deltaTime * speedFactor;
            }
            //Debug.Log("Called");
        }
        else // if t > 1 and keepPathing (usually true, only false when the target is stationary, set at reaching. 
        { // *can remove the && !reachedEndOfPath and the path = null condition if hit bug. Suspicious.
            if(!keepPathing)
            {
                // This basically means you've reached a set destination to the target.
                // Call other functions etc
                // if KeepPathing stays true, then the ai following target continues as target moves. 
                Debug.Log("I shall stop HERE.");
                return;
            }

            UpdatePath(); // update the path at the end of every path division.

            Debug.Log("t > 1. Check: pathDivisionsQueueCount: " + pathDivisions.Count + " and not reachedEndOfPath: " + !reachedEndOfPath);
            if (pathDivisions.Count == 0) // generate a new path division
            {
                Debug.Log("NEW PATH DIV GENERATED! Current waypoint is " + currentWaypoint + ", and total waypoints are: " + path.vectorPath.Count);

                // Problem with this is that the AI cuts through curves based on the alg since direct lerping.
                // which makes it weird since the AI passes through obstacles.
                // I don't think this is the problem. It's a problem with currWayPoint that it jumps to the end.
                p0 = transform.position;
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
                float newRelativePosition = Mathf.Sign(transform.position.x - targetPosition.transform.position.x);
                //Debug.Log("ARE THE RELATIVE POSITIONS DIFFERENT: " + (newRelativePosition != relativePosition));
                if (newRelativePosition != relativePosition)
                {
                    Debug.Log("Sign Flipped");
                    relativePosition = newRelativePosition;
                    alternatingFactor *= -1;
                }
            }

            // go to the next pt in the path division. 
            t = 0;
            p0 = transform.position;
            p3 = pathDivisions.Dequeue();
            Vector2 rot0 = (p0 - p3).normalized / (1 / minSegDist); // / by int to change length
            Vector2 rot3 = (p3 - p0).normalized / (1 / minSegDist);
            p1 = p0 + RotateVector(rot0, uniformalBezDegree * alternatingFactor);
            p2 = p3 + RotateVector(rot3, -uniformalBezDegree * alternatingFactor);
            Debug.Log("Sign Flipped");
            alternatingFactor *= -1;

            Debug.Log("Bez: " + p0 + " " + p1 + " " + p2 + " " + p3);
            //Debug.Log("Character trans: " + FindObjectOfType<PlayerScript>().transform.position);
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

    float Angle(Vector3 v)
    {
        // normalize the vector: this makes the x and y components numerically
        // equal to the sine and cosine of the angle:
        v.Normalize();
        // get the basic angle:
        var ang = Mathf.Asin(v.x) * Mathf.Rad2Deg;
        // fix the angle for 2nd and 3rd quadrants:
        if (v.y< 0){
            ang = 180 - ang;
        }
        else // fix the angle for 4th quadrant:
        if (v.x < 0)
        {
            ang = 360 + ang;
        }
        return ang;
     }
}

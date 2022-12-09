using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BezierPattern : PestMovement
{
    // Bezier Experimental
    private Vector2 p0, p1, p2, p3;

    private float t = 1.1f;

    private Queue<Vector2> pathDivisions;

    public float minSegDist = 0.5f; // width of each curve

    public float curveExtrusionFactor = 0.5f; // outward extrudedness of each curve

    public int uniformalBezDegree = 90; // degree of uniformal inward / outward extrusion

    private int alternatingFactor = 1;

    private float relativePosition;

    public float slowdownDetectionRange = 2; // range, in radius, of slowdown activation. Set to 0 for no slowdown.


    public override void Start()
    {
        pathDivisions = new Queue<Vector2>();
        relativePosition = Mathf.Sign(transform.position.x - targetPosition.transform.position.x);

        base.Start();
    }


    public void Update()
    {
        if (path == null)
        {
            // We have no path to follow yet, so don't do anything
            //Debug.Log("NO PATH");
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
            //Debug.Log("Before while loop: current waypoint: " + currentWaypoint + ", and total waypoints are: " + path.vectorPath.Count);
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

                        //Debug.Log("Distance to waypoint is: " + distanceToWaypoint);
                        Debug.Log("END OF PATH REACHED. Execute an Action here.");

                        // if target is stationary and no more movement etc, then keepPathing = false.
                        // else call UpdatePath() and keep var true. Nevermind outdated thought.
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
            //Debug.Log("After while loop: current waypoint: " + currentWaypoint + ", and total waypoints are: " + path.vectorPath.Count);
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
                float distToTarget = Vector3.Distance(transform.position, targetPosition.position);
                if(distToTarget <= slowdownDetectionRange)
                {
                    //Debug.Log("Dist to target is: " + distToTarget);
                    speedFactor = Mathf.Sqrt(distToTarget / slowdownDetectionRange);
                }
                /*if(reachedEndOfPath)
                {
                    Debug.LogError("REACHED END OF PATH TRIGGERED");
                    var distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);
                    speedFactor = Mathf.Sqrt(distanceToTarget / nextWaypointDistance);
                }*/
                //var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

                // Run these two lines separately everytime creates a cool pausing effect.
                //var distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
                //speedFactor = Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance);

                transform.position += dir * speed * Time.deltaTime * speedFactor;
            }
            //Debug.Log("Called");
        }
        else // if t > 1 and keepPathing (usually true, only false when the target is stationary, set at reaching. 
        { 
            if(!keepPathing)
            {
                // This basically means you've reached a set destination to the target.
                // Call other functions etc
                // if KeepPathing stays true, then the ai following target continues as target moves. 
                Debug.Log("I shall stop HERE.");
                return;
            }

            UpdatePath(); // update the path at the end of every path division.

            //Debug.Log("t > 1. Check: pathDivisionsQueueCount: " + pathDivisions.Count + " and not reachedEndOfPath: " + !reachedEndOfPath);
            if (pathDivisions.Count == 0) // generate a new path division
            {
                //Debug.Log("NEW PATH DIV GENERATED! Current waypoint is " + currentWaypoint + ", and total waypoints are: " + path.vectorPath.Count);

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
                    //Debug.Log("Sign Flipped");
                    relativePosition = newRelativePosition;
                    alternatingFactor *= -1;
                }
            }

            // go to the next pt in the path division. 
            t = 0;
            p0 = transform.position;
            p3 = pathDivisions.Dequeue();
            Vector2 rot0 = (p0 - p3).normalized / (1 / curveExtrusionFactor); //minSegDist); // / by int to change length
            Vector2 rot3 = (p3 - p0).normalized / (1 / curveExtrusionFactor);
            p1 = p0 + RotateVector(rot0, uniformalBezDegree * alternatingFactor);
            p2 = p3 + RotateVector(rot3, -uniformalBezDegree * alternatingFactor);
            //Debug.Log("Sign Flipped");
            alternatingFactor *= -1;

            //Debug.Log("Bez: " + p0 + " " + p1 + " " + p2 + " " + p3);
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

    /*
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
     }*/
}

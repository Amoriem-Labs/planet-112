using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePattern : PestMovement
{
    // Start is called before the first frame update
    public override void OnEnable()
    {
        base.OnEnable();
        //InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    public override void StartPathing()
    {
        base.StartPathing();
        //InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    public override void StopPathing()
    {
        base.StopPathing();
        //CancelInvoke();
    }


    // If want to use physics, apply a rigid body, fixed update, and add force accordingly.
    public void Update()
    {
        if (path == null)
        {
            // We have no path to follow yet, so don't do anything
            return;
        }

        if (!keepPathing)
        {
            // This basically means you've reached a set destination to the target.
            // Call other functions etc
            // if KeepPathing stays true, then the ai following target continues as target moves. 
            ////Debug.Log("I shall stop HERE.");
            EndPathing(true);
            return;
        }

        // Check in a loop if we are close enough to the current waypoint to switch to the next one.
        // We do this in a loop because many waypoints might be close to each other and we may reach
        // several of them in the same frame.
        reachedEndOfPath = false;
        // The distance to the next waypoint in the path
        float distanceToWaypoint;
        while (true)
        {
            // If you want maximum performance you can check the squared distance instead to get rid of a
            // square root calculation. But that is outside the scope of this tutorial.
            distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
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
                    reachedEndOfPath = true;

                    ////Debug.Log("END OF PATH REACHED. Execute an Action here.");

                    // are you trapped trying to reach the unreacheable?
                    if (targetPosition != null &&
                        Vector2.Distance(path.vectorPath[currentWaypoint], (targetPosition.position + targetOffsetFromCenter))
                        > GetComponent<PestScript>().attackRange)
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
                    if (GetComponent<PestScript>().TargetPlantInAttackRange()) // need to make sure this path is in range one
                    {
                        if (!GetComponent<PestScript>().targetPlantScript.inMotion) keepPathing = false; // naturally 
                        else EndPathing(false); // pest is still pathing / aka chasing the plant, but also attacking.
                    }
                    else if (targetPosition == null || consecUnreacheableCounter >= minAttemptUnreacheable) // new target time
                    {
                        resetPath = true;
                        keepPathing = false;
                        ////Debug.Log("Potentially idleling");
                        //enabled = false; // target destroyed. Better to set to idle behavior here while calculating/waiting new target
                    }
                    else if (decoyState)
                    {
                        EndPathing(false);
                    }

                    UpdatePath();

                    break;
                }
            }
            else
            {
                break;
            }
        }

        // Slow down smoothly upon approaching the end of the path
        // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
        var speedFactor = reachedEndOfPath && !decoyState ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

        // Direction to the next waypoint
        // Normalize it so that it has a length of 1 world unit
        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        // Multiply the direction by our desired speed to get a velocity
        Vector3 velocity = dir * speed * speedFactor;

        // If you are writing a 2D game you should remove the CharacterController code above and instead move the transform directly by uncommenting the next line
        transform.position += velocity * Time.deltaTime;

        if (velocity.x >= 0.01f) // velocity enemy desires to travel with based on the current path
        {
            enemyGraphics.localScale = new Vector3(1f, 1f, 1f); // if do velocity.x, then has delay because acts with velocity
        }
        else if (velocity.x <= -0.01f)
        {
            enemyGraphics.localScale = new Vector3(-1f, 1f, 1f);
        }
    }
}

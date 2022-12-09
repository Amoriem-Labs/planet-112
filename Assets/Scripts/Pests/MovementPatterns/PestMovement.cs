using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

// This is the parent class to all the movement patterns!

public class PestMovement : MonoBehaviour
{
    // These values are in the inspector! and they overwrite the ones set in here!

    public Transform targetPosition;

    protected Seeker seeker;

    public Path path;

    public Transform enemyGraphics;

    public float speed;

    public float nextWaypointDistance; // smaller the more accurate

    protected int currentWaypoint;

    protected bool reachedEndOfPath;

    public bool keepPathing; // true to activate perma pathing, false to keep the path one-time

    //public GameObject testPrefab;

    public virtual void Start()
    {
        currentWaypoint = 0;
        keepPathing = true; // if this is set to false, then pest won't move at start until it's true.
        reachedEndOfPath = false;

        seeker = GetComponent<Seeker>();

        UpdatePath();
    }

    // start/resume statement for the pest; pause statement for the pest
    public virtual void StartPathing() { keepPathing = true; }
    public virtual void StopPathing() { keepPathing = false; }

    protected void UpdatePath()
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


    private void OnPathComplete(Path p)
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
}

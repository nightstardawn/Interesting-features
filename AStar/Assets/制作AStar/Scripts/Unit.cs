using System;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform target;
    public float Speed;
    public float turnDst = 5;
    public float turnSpeed = 10;
    private Path path;
    private void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    private void OnPathFound(Vector3[] wayPoint, bool success)
    {
        if (success)
        {
            path = new Path(wayPoint,transform.position,turnDst);
            StopCoroutine("");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(path.lookPoints[pathIndex]);
        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D))
            {
                if (pathIndex == path.finishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                    pathIndex++;
            }

            if (followingPath)
            {
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                transform.Translate(Vector3.forward * Time.deltaTime * Speed,Space.Self);
            }
            yield return null;
        }
    }

    public void OnDrawGizmos() 
    {
        if (path != null)
        {
            path.DrawWithGizmos();
        }
    } 
}

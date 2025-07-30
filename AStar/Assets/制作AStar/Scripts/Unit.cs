using System;
using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    const float minPathUpdateTime = 0.2f;
    const float pathUpdateMoveThreshold = 0.5f;
    public Transform target;
    public float Speed;
    public float turnDst = 5;
    public float turnSpeed = 10;
    public float stoppingDst = 10;
    private Path path;

    private void Start()
    {
        StartCoroutine(UpadatePath());
    }

    private void OnPathFound(Vector3[] wayPoint, bool success)
    {
        if (success)
        {
            path = new Path(wayPoint,transform.position,turnDst,stoppingDst);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator UpadatePath()
    {
        if (Time.timeSinceLevelLoad > .3f)
        {
            yield return new WaitForSeconds(0.3f);
        }
        PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
        float sqrMoveThreshold = pathUpdateMoveThreshold * pathUpdateMoveThreshold;
        Vector3 targetOldPos = target.position;
        while (true)
        {
            yield return new WaitForSeconds(minPathUpdateTime);
            if ((target.position - targetOldPos).sqrMagnitude > sqrMoveThreshold)
            {
                PathRequestManager.RequestPath(new PathRequest(transform.position, target.position, OnPathFound));
                targetOldPos = target.position;
            }
        }
    }
    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        float speedPercent = 1;
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
                if(pathIndex == path.slowDownIndex && stoppingDst > 0)
                {
                    speedPercent = Mathf.Clamp01(path.turnBoundaries[path.finishLineIndex].DistanceFormPoint(pos2D) /
                                                 stoppingDst);
                    if(speedPercent < 0.01f)
                        followingPath = false;

                }
                
                Quaternion targetRotation = Quaternion.LookRotation(path.lookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
                transform.Translate(Vector3.forward * Time.deltaTime * Speed * speedPercent,Space.Self);
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

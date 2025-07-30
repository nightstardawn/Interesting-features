using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathResult> results = new Queue<PathResult>();
    static PathRequestManager instance;
    Pathfinding pathfinding;
    bool isProcessingPath;
    private void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    private void Update()
    {
        if (results.Count > 0)
        {
            int count = results.Count;
            lock (results)
            {
                for (int i = 0; i < count; i++)
                {
                    PathResult result = results.Dequeue();
                    result.callback(result.path, result.success);
                }
            }
        }
    }

    public static void RequestPath(PathRequest request)
    {
        ThreadStart threadStart = delegate
        {
            instance.pathfinding.FindPath(request, instance.FinishProcessingPath);
        };
        threadStart.Invoke();
    }

    public void FinishProcessingPath(PathResult result)
    {
        lock (results)
        {
            results.Enqueue(result);
        }
    }
}
public struct PathResult
{
    public Vector3[] path;
    public bool success;
    public Action<Vector3[], bool> callback;
    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
    {
        this.path = path;
        this.success = success;
        this.callback = callback;
    }
}
public struct PathRequest
{
    public Vector3 pathStart;
    public Vector3 pathEnd;
    public Action<Vector3[], bool> callback;

    public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        this.pathStart = pathStart;
        this.pathEnd = pathEnd;
        this.callback = callback;
    }
}

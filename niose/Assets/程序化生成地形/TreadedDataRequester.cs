using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
public class TreadedDataRequester : MonoBehaviour
{
    static TreadedDataRequester instance;

    private void Awake()
    {
        instance = FindObjectOfType<TreadedDataRequester>();
    }
    Queue<TreadInfo> dataQueue = new Queue<TreadInfo>();
    
    public static void RequestData(Func<object> generateData, Action<object> callback)
    {
        ThreadStart threadStart = delegate
        {
            instance.DataTread(generateData,callback);
        };
        new Thread(threadStart).Start();
    }
    
    void DataTread(Func<object> generateData,Action<object> callback)
    {
        object data = generateData();
        lock (dataQueue)
            dataQueue.Enqueue(new TreadInfo(callback, data));
    }
    private void Update()
    {
        if (dataQueue.Count > 0)
        {
            for (int i = 0; i < dataQueue.Count; i++)
            {
                TreadInfo threadInfo = dataQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }
}
struct TreadInfo
{
    public readonly Action<object> callback;
    public readonly  object parameter;
    public TreadInfo(Action<object> callback, object parameter)
    {
        this.callback = callback;
        this.parameter = parameter;
    }
}
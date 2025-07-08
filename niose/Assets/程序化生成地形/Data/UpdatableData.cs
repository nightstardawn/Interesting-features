using System;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event Action OnValuesUpdated;
    public bool autoUpdate;
    
    protected virtual void OnValidate()
    {
#if UNITY_EDITOR
        if (autoUpdate)
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
#endif
    }
    public void NotifyOfUpdatedValues()
    {
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
#endif
    }
}

using System;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
    public event Action OnValuesUpdated;
    public bool autoUpdate;
    
#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (autoUpdate)
            UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
    }
    public void NotifyOfUpdatedValues()
    {
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
    }
#endif
}

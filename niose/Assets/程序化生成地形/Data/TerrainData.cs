using UnityEngine;


[CreateAssetMenu()]
public class TerrainData : UpdatableData
{
    public float uniformScale = 2f;
    
    public bool useFlatShading;
    public bool useFalloffMap;
    
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    
    public float minHeight => uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
    public float maxHeight => uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
}

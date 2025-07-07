using System;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData
{
    public Noise.NormalizationMode normalizationMode;
    public float noiseScale;
    
    public int octaves;
    public float lacunarity;
    [Range(0,1)] public float persistence;
    public int seed;
    public Vector2 offset;

    protected override void OnValidate()
    {
        if(lacunarity < 1)
            lacunarity = 1;
        if(octaves < 0)
            octaves = 0;
        base.OnValidate();
    }
}

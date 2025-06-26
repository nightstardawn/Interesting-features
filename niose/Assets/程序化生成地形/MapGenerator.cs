using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    
    public int octaves;
    public float lacunarity;
    [Range(0,1)]
    public float persistence;
    public int seed;
    public Vector2 offset;
    public bool autoUpdate;

    public void GenerateMap() {
        float[,] noiseMap = Noise.GenerateNoiseMap (mapWidth, mapHeight, seed,noiseScale,
                                                    octaves,  lacunarity, persistence,offset);


        MapDisplay display = FindObjectOfType<MapDisplay> ();
        display.DrawNoiseMap(noiseMap);
    }

    private void OnValidate()
    {
        if(mapWidth <1)
            mapWidth = 1;
        if (mapHeight < 1)
            mapHeight = 1;
        if(lacunarity < 1)
            lacunarity = 1;
        if(octaves < 0)
            octaves = 0;
    }
}

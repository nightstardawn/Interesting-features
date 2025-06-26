using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh,
    }
    public DrawMode drawMode = DrawMode.NoiseMap;
    public const int mapMeshChunksize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;
    
    public int octaves;
    public float lacunarity;
    [Range(0,1)]
    public float persistence;
    public int seed;
    public Vector2 offset;
    
    public TerrainType[] regions;
    
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public bool autoUpdate;

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();
        MapDisplay display = FindObjectOfType<MapDisplay> ();
        if(drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromNoiseMap(mapData.heightMap));
        else if(drawMode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFormColorMap(mapData.colorMap, mapMeshChunksize, mapMeshChunksize));
        else if (drawMode == DrawMode.Mesh)
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap,meshHeightMultiplier,meshHeightCurve,levelOfDetail),TextureGenerator.TextureFormColorMap(mapData.colorMap, mapMeshChunksize, mapMeshChunksize));
    }
    private MapData GenerateMapData() {
        float[,] noiseMap = Noise.GenerateNoiseMap (mapMeshChunksize, mapMeshChunksize, seed,noiseScale,
                                                    octaves,  lacunarity, persistence,offset);
        Color[] colorMap = new Color[mapMeshChunksize * mapMeshChunksize];
        for (int y = 0; y < mapMeshChunksize; y++)
        {
            for (int x = 0; x < mapMeshChunksize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight< regions[i].height)
                    {
                        colorMap[y * mapMeshChunksize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        return new MapData(noiseMap, colorMap);
    }

    private void OnValidate()
    {
        if(lacunarity < 1)
            lacunarity = 1;
        if(octaves < 0)
            octaves = 0;
    }
}
[Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;

    public TerrainType(string name, float height, Color color)
    {
        this.name = name;
        this.height = height;
        this.color = color;
    }
}

public struct MapData
{
    public float[,] heightMap;
    public Color[] colorMap;
    
    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}

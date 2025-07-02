using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh,
        FalloffMap,
    }
    public DrawMode drawMode = DrawMode.NoiseMap;
    public Noise.NormalizationMode normalizationMode;
    public const int mapMeshChunksize = 239;
    [Range(0,6)]
    public int editorPreviewLOD;
    public float noiseScale;
    
    public int octaves;
    public float lacunarity;
    [Range(0,1)]
    public float persistence;
    public int seed;
    public Vector2 offset;
    
    public bool useFalloffMap; 
        
    public TerrainType[] regions;
    
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    
    float[,] falloffMap;
    
    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    public bool autoUpdate;

    private void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFalloff(mapMeshChunksize);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay> ();
        if(drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromNoiseMap(mapData.heightMap));
        else if(drawMode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFormColorMap(mapData.colorMap, mapMeshChunksize, mapMeshChunksize));
        else if (drawMode == DrawMode.Mesh)
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap,meshHeightMultiplier,meshHeightCurve,editorPreviewLOD),TextureGenerator.TextureFormColorMap(mapData.colorMap, mapMeshChunksize, mapMeshChunksize));
        else if (drawMode == DrawMode.FalloffMap)
            display.DrawTexture(TextureGenerator.TextureFromNoiseMap(FalloffGenerator.GenerateFalloff(mapMeshChunksize)));
            
        
    }
    
    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center,callback);
        };
        new Thread(threadStart).Start();
    }
    
    void MapDataThread(Vector2 center,Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
    }
    public void RequestMeshData(MapData mapData,int lod,Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData,lod,callback);
        };
        new Thread(threadStart).Start();
    }
    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap,meshHeightMultiplier,meshHeightCurve,lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    private MapData GenerateMapData(Vector2 center) {
        float[,] noiseMap = Noise.GenerateNoiseMap (mapMeshChunksize + 2, mapMeshChunksize + 2, seed,noiseScale,
                                                    octaves,  lacunarity, persistence,center + offset,normalizationMode);
        Color[] colorMap = new Color[mapMeshChunksize * mapMeshChunksize];
        for (int y = 0; y < mapMeshChunksize; y++)
        {
            for (int x = 0; x < mapMeshChunksize; x++)
            {
                if (useFalloffMap) 
                    noiseMap[x,y] = Mathf.Clamp01(noiseMap[x,y] - falloffMap[x,y]);
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight >= regions[i].height)
                        colorMap[y * mapMeshChunksize + x] = regions[i].color;
                    else
                        break;
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
        falloffMap = FalloffGenerator.GenerateFalloff(mapMeshChunksize);
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly  T parameter;
        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
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
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;
    
    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}

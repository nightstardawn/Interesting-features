
using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    
    const float viewMoveThresholdForChunkUpdate = 25f;
    const float sqrViewMoveThresholdForChunkUpdate = viewMoveThresholdForChunkUpdate * viewMoveThresholdForChunkUpdate;
    
    public int colliderLODIndex;
    public LODInfo[] detailLevels;
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;
    public Transform viewer;
    public Material mapMaterial;
    
    Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    float meshWorldSize;
    int chunckVisibleInViewDst;
    
    Dictionary<Vector2,TerrainChunk> terrainChunksDictionary = new Dictionary<Vector2,TerrainChunk>();
    List<TerrainChunk> visiableTerrainChunks = new List<TerrainChunk>();
    private void Start()
    {
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial,heightMapSettings.minHeight,heightMapSettings.maxHeight);
        
        float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunckVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);
        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if (viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk chunk in visiableTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }
        
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewMoveThresholdForChunkUpdate)
        { 
            UpdateVisibleChunks();
            viewerPositionOld = viewerPosition;
        }
    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunksCoords = new HashSet<Vector2>();
        for (int i = visiableTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunksCoords.Add(visiableTerrainChunks[i].coord);
            visiableTerrainChunks[i].UpdateTerrainChunk();
        }
        
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunckVisibleInViewDst; yOffset <= chunckVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunckVisibleInViewDst; xOffset <= chunckVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunksCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunksDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunksDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord,heightMapSettings,meshSettings,
                                                                 detailLevels,colliderLODIndex,transform,viewer,mapMaterial);
                        terrainChunksDictionary.Add(viewedChunkCoord,newChunk);
                        newChunk.onVisiavbleChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }
            }
        }

        void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
        {
            if (isVisible)
            {
                visiableTerrainChunks.Add(chunk);
            }
            else
            {
                visiableTerrainChunks.Remove(chunk);
            }
        }
    }
}
[System.Serializable]
public struct LODInfo
{
    [Range(0,MeshSettings.numSupportedLODs - 1)] public int lod;
    public float visibleDstThreshold;

    public float SqrvisibleDstThreshold => visibleDstThreshold * visibleDstThreshold;
}
class LODMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    private int lod;
    public event Action updataCallback;

    public LODMesh(int lod)
    {
        this.lod = lod; 
    }

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;
        updataCallback();
    }

    public void RequestMesh(HeightMap heightMap,MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        TreadedDataRequester.RequestData(()=>MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod),OnMeshDataReceived);
    }
}
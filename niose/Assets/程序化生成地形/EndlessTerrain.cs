
using System;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float viewMoveThresholdForChunkUpdate = 25f;
    const float sqrViewMoveThresholdForChunkUpdate = viewMoveThresholdForChunkUpdate * viewMoveThresholdForChunkUpdate;
    
    public LODInfo[] detailLevels;
    public static float maxViewDst;
    public Transform viewer;
    public Material mapMaterial;
    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    private int chunkSize;
    int chunckVisibleInViewDst;
    
    Dictionary<Vector2,TerrainChunk> terrainChunksDictionary = new Dictionary<Vector2,TerrainChunk>();
    List<TerrainChunk> terrainChunksVisiableLastUpdate = new List<TerrainChunk>();
    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = MapGenerator.mapMeshChunksize - 1;
        chunckVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewMoveThresholdForChunkUpdate)
        { 
            UpdateVisibleChunks();
            viewerPositionOld = viewerPosition;
        }
    }

    void UpdateVisibleChunks()
    {
        for (int i = 0; i < terrainChunksVisiableLastUpdate.Count; i++)
        {
            terrainChunksVisiableLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisiableLastUpdate.Clear();
        
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunckVisibleInViewDst; yOffset <= chunckVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunckVisibleInViewDst; xOffset <= chunckVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunksDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunksDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunksDictionary[viewedChunkCoord].IsVisible())
                        terrainChunksVisiableLastUpdate.Add(terrainChunksDictionary[viewedChunkCoord]);
                }
                else
                {
                    terrainChunksDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord,chunkSize,detailLevels,transform,mapMaterial));
                }
                
            }
        }
        
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        
        
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        
        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        public TerrainChunk(Vector2 coord, int size,LODInfo[] detailLevels,Transform parent,Material material)
        {
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            
            meshObject = new GameObject("Terrain Chunk " + coord);
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod,UpdateTerrainChunk);
            }
            mapGenerator.RequestMapData(position,OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;
            Texture2D texture = TextureGenerator.TextureFormColorMap(mapData.colorMap, MapGenerator.mapMeshChunksize, MapGenerator.mapMeshChunksize);
            meshRenderer.material.mainTexture = texture;
            UpdateTerrainChunk();
        }
        
        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewDstFormNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewDstFormNearestEdge <= maxViewDst;
                if (visible)
                {
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewDstFormNearestEdge > detailLevels[i].visibleDstThreshold)
                            lodIndex = i + 1;
                        else
                            break;
                    }

                    if (previousLODIndex != lodIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if(!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }
                SetVisible(visible);
            }
        }
        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }
        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;
        private Action updataCallback;

        public LODMesh(int lod, Action updataCallback)
        {
            this.lod = lod; 
            this.updataCallback = updataCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updataCallback();
        }

        public void RequestMesh(MapData mapdata)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapdata,lod,OnMeshDataReceived);
        }
    }
    [Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
    }
}

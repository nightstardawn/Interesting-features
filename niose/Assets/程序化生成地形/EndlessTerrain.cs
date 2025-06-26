
using System;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDst = 450f;
    public Transform viewer;
    
    public static Vector2 viewerPosition;
    private int chunkSize;
    int chunckVisibleInViewDst;
    
    Dictionary<Vector2,TerrainChunk> terrainChunksDictionary = new Dictionary<Vector2,TerrainChunk>();
    List<TerrainChunk> terrainChunksVisiableLastUpdate = new List<TerrainChunk>();
    private void Start()
    {
        chunkSize = MapGenerator.mapMeshChunksize - 1;
        chunckVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
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
                    terrainChunksDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord,chunkSize,transform));
                }
                
            }
        }
        
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        private Bounds bounds;
        public TerrainChunk(Vector2 coord, int size,Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false);
        }

        public void UpdateTerrainChunk()
        {
            float viewDstFormNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewDstFormNearestEdge <= maxViewDst;
            SetVisible(visible);
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
}

using UnityEngine;

public class TerrainChunk
    {
        const float collidsionGenerationDistanceThreshold = 5f;
        public event System.Action<TerrainChunk,bool> onVisiavbleChanged;
        public Vector2 coord; 
        GameObject meshObject;
        Vector2 sampleCenter;
        Bounds bounds;
        
        
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        LODMesh collisionLODMesh;
        int colliderLODIndex;
        
        HeightMap heightMap;
        bool heightMapReceived;
        int previousLODIndex = -1;
        bool hasSetCollider;
        float maxViewDst;

        HeightMapSettings heightMapSettings;
        MeshSettings meshSettings;
        Transform viewer;
        public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings,MeshSettings meshSettings,
                            LODInfo[] detailLevels,int colliderLODIndex,
                            Transform parent,Transform viewer, Material material)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;
            this.heightMapSettings = heightMapSettings;
            this.meshSettings = meshSettings;
            this.viewer = viewer;
            
            sampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
            Vector2 position = coord * meshSettings.meshWorldSize;
            bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);
            
            meshObject = new GameObject("Terrain Chunk " + coord);
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            
            meshRenderer.material = material;
            meshObject.transform.position = new Vector3(position.x,0,position.y);
            meshObject.transform.parent = parent;
            
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                lodMeshes[i].updataCallback += UpdateTerrainChunk;
                if (i == colliderLODIndex)
                {
                    lodMeshes[i].updataCallback += UpdateCollisionMesh;
                }
            }

            maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        }

        public void Load()
        {
            TreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine,
                                              heightMapSettings, sampleCenter)
                                            ,OnHeightMapReceived);
        }
        void OnHeightMapReceived(object heightMapObject)
        {
            this.heightMap = (HeightMap)heightMapObject;
            heightMapReceived = true;
            
            UpdateTerrainChunk();
        }
        
        Vector2 viewerPosition => new Vector2(viewer.position.x, viewer.position.z);
        
        public void UpdateTerrainChunk()
        {
            if (heightMapReceived)
            {
                float viewDstFormNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool wasVisible = IsVisible();
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

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if(!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(heightMap,meshSettings);
                        }
                    }
                }

                if (wasVisible != visible)
                {
                    SetVisible(visible);
                    if (onVisiavbleChanged != null)
                    {
                        onVisiavbleChanged(this, visible);
                    }
                }
            }
        }
        public void UpdateCollisionMesh()
        {
            if (!hasSetCollider)
            {
                float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

                if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].SqrvisibleDstThreshold)
                {
                    if (!lodMeshes [colliderLODIndex].hasRequestedMesh) 
                    {
                        lodMeshes [colliderLODIndex].RequestMesh (heightMap, meshSettings);
                    }
                }
            
                if (sqrDstFromViewerToEdge <= collidsionGenerationDistanceThreshold * collidsionGenerationDistanceThreshold)
                {
                    if (lodMeshes [colliderLODIndex].hasMesh) {
                        meshCollider.sharedMesh = lodMeshes [colliderLODIndex].mesh;
                        hasSetCollider = true;
                    }
                }   
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



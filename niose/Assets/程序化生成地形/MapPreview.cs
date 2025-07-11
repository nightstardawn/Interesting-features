using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public bool autoUpdate;
    
    public enum DrawMode
    {
        NoiseMap,
        Mesh,
        FalloffMap,
    }
    public DrawMode drawMode = DrawMode.NoiseMap;
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;
    
    public Material terrainMaterial;

    
    [Range(0,MeshSettings.numSupportedLODs - 1)] public int editorPreviewLOD;
    public void DrawMapInEditor()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial,heightMapSettings.minHeight,heightMapSettings.maxHeight);
        HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine,
                                                                   heightMapSettings,Vector2.zero);
        if(drawMode == DrawMode.NoiseMap)
            DrawTexture(TextureGenerator.TextureFromNoiseMap(heightMap));
        else if (drawMode == DrawMode.Mesh)
            DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings,editorPreviewLOD));
        else if (drawMode == DrawMode.FalloffMap)
            DrawTexture(TextureGenerator.TextureFromNoiseMap(new HeightMap(FalloffGenerator.GenerateFalloff(meshSettings.numVertsPerLine),0,1)));
    }
    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.SetTexture("_MainTex", texture);
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        
        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }
    void OnValueUpdated()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
            textureData.ApplyToMaterial(terrainMaterial);
        }
    }
    private void OnValidate()
    {
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValueUpdated;
            meshSettings.OnValuesUpdated += OnValueUpdated;
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValueUpdated;
            heightMapSettings.OnValuesUpdated += OnValueUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnValueUpdated;
            textureData.OnValuesUpdated += OnValueUpdated;
        }

    }
}

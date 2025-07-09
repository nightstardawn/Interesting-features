
using System.Linq;
using UnityEngine;

[CreateAssetMenu()]
public class TextureData:UpdatableData
{
    private const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;
    public Layer[] layers;
    private float savedMinHeight;
    private float savedMaxHeight;
    public void ApplyToMaterial(Material material)
    {
        // 确保这些名称与着色器中的变量名匹配
        material.SetInt("_LayerCount", layers.Length);
        material.SetColorArray("_BaseColors", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("_BaseStartHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("_BaseBlends", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("_BaseColourStrength", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("_BaseTextureScales", layers.Select(x => x.textureScale).ToArray());
        Texture2DArray texture2DArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        material.SetTexture("_BaseTextures", texture2DArray);
        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;
        
        material.SetFloat("_MinHeight", minHeight);
        material.SetFloat("_MaxHeight", maxHeight);
    }

    Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        Texture2DArray texture2DArray
            = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
        for (int i = 0; i < textures.Length; i++)
        {
            texture2DArray.SetPixels(textures[i].GetPixels(), i);
        }
        texture2DArray.Apply();
        return texture2DArray;
    }
    
    [System.Serializable]
    public class Layer
    {
        public Texture2D texture;
        public Color tint;
        
        [Range(0,1)] public float tintStrength;
        [Range(0,1)] public float startHeight;
        [Range(0,1)] public float blendStrength;
        public float textureScale;

    }
}

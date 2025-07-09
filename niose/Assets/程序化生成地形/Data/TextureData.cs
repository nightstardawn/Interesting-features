
using UnityEngine;

[CreateAssetMenu()]
public class TextureData:UpdatableData
{
    public Color[] baseColors;
    [Range(0,1)]public float[] baseStartHeights;
    [Range(0,1)]public float[] baseBlends;
    private float savedMinHeight;
    private float savedMaxHeight;
    public void ApplyToMaterial(Material material)
    {
        // 确保这些名称与着色器中的变量名匹配
        material.SetInt("_BaseColorCount", baseColors.Length);
        material.SetColorArray("_BaseColors", baseColors);
        material.SetFloatArray("_BaseStartHeights", baseStartHeights);
        material.SetFloatArray("_BaseBlends", baseBlends);
        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;
        
        material.SetFloat("_MinHeight", minHeight);
        material.SetFloat("_MaxHeight", maxHeight);
    }
}

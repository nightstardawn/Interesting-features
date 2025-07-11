using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu()]
public class MeshSettings : UpdatableData
{
    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numsSupportedFlatshadedSizes = 3;
    public static readonly int[] supportedChunkSizes = {48,72,96,120,144,168,192,216,240};
    
    public float meshScale = 2f;
    public bool useFlatShading;
    
    [Range(0,numSupportedChunkSizes - 1)] public int chunkSizeIndex;
    [Range(0,numsSupportedFlatshadedSizes - 1)] public int flatShadedChunkSizeIndex;
    
    // 在 LOD = 0 时渲染的每行网格的顶点数,包含最终网格中用于计算法时排除的 2 个额外顶点
    public int numVertsPerLine => supportedChunkSizes[(useFlatShading) ? flatShadedChunkSizeIndex : chunkSizeIndex] - 1 + 2;

    public float meshWorldSize => (numVertsPerLine - 1 - 2) * meshScale;// 排除 2 个额外顶点
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise 
{
    public enum NormalizationMode 
    {
        Local,
        Global
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight,int seed, float scale, 
                                           int octaves,  float lacunarity ,float persistence, Vector2 offset,NormalizationMode normalizationMode) 
    {
        // 创建二维数组存储噪声图，尺寸为指定的宽度和高度
        float[,] noiseMap = new float[mapWidth, mapHeight];
    
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;
        
        
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }
        // 防止除零错误：确保缩放比例不会为零或负数
        if (scale <= 0) 
            scale = 0.0001f;
        
        // 初始化变量用于记录整个噪声图中的最小和最大值
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;
        
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        
        // 遍历噪声图的每个像素
        for (int y = 0; y < mapHeight; y++) 
        {
            for (int x = 0; x < mapWidth; x++) 
            {
                // 初始化振幅和频率（用于分形噪声叠加）
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0; // 当前像素的总噪声值
                
                // 分形噪声：叠加多个不同频率的噪声层（octaves）
                for (int i = 0; i < octaves; i++)
                {
                    // 计算当前频率下的采样坐标
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;
                    
                    // 生成Perlin噪声值（范围-1到1）
                    // Unity的PerlinNoise返回0-1，我们转换为-1到1以便负值地形
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    
                    // 累加当前层的噪声值（乘以振幅权重）
                    noiseHeight += perlinValue * amplitude;
                    
                    // 更新下一层的参数：振幅递减，频率增加
                    // persistence: 振幅衰减率（0-1），值越小表示高层影响越小
                    // lacunarity: 频率增长率（>1），值越大表示高层细节越密集
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                
                // 更新整个噪声图的最小/最大值（用于后续归一化）
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                // 存储当前像素的原始噪声值
                noiseMap[x,y] = noiseHeight;
            }
        }
        
        // 第二遍遍历：将所有值归一化到[0,1]范围
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizationMode == NormalizationMode.Local) 
                    noiseMap[x,y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x,y]);
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + maxPossibleHeight) / (2 * maxPossibleHeight);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight,0,int.MaxValue);
                }
            }
        }
        
        return noiseMap;
    }

}

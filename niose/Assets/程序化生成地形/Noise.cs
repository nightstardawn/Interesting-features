using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public static class Noise 
{
    public enum NormalizationMode 
    {
        Local,
        Global
    }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight,NoiseSettings settings,Vector2 sampleCenter)
    {
        // 创建二维数组存储噪声图，尺寸为指定的宽度和高度
        float[,] noiseMap = new float[mapWidth, mapHeight];
    
        System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;
        
        
        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCenter.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCenter.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
            maxPossibleHeight += amplitude;
            amplitude *= settings.persistence;
        }
        
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
                for (int i = 0; i < settings.octaves; i++)
                {
                    // 计算当前频率下的采样坐标
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;
                    
                    // 生成Perlin噪声值（范围-1到1）
                    // Unity的PerlinNoise返回0-1，我们转换为-1到1以便负值地形
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    
                    // 累加当前层的噪声值（乘以振幅权重）
                    noiseHeight += perlinValue * amplitude;
                    
                    // 更新下一层的参数：振幅递减，频率增加
                    // persistence: 振幅衰减率（0-1），值越小表示高层影响越小
                    // lacunarity: 频率增长率（>1），值越大表示高层细节越密集
                    amplitude *= settings.persistence;
                    frequency *= settings.lacunarity;
                }
                
                // 更新整个噪声图的最小/最大值（用于后续归一化）
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                // 存储当前像素的原始噪声值
                noiseMap[x,y] = noiseHeight;

                if(settings.normalizationMode == NormalizationMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }
        
        // 第二遍遍历：将所有值归一化到[0,1]范围
        if (settings.normalizationMode == NormalizationMode.Local)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x,y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x,y]);
                }
            }
        }
        return noiseMap;
    }

}

[System.Serializable]
public class NoiseSettings
{
    public Noise.NormalizationMode normalizationMode;
    [FormerlySerializedAs("noiseScale")] public float scale = 50;
    
    public int octaves = 6;
    public float lacunarity = 2;
    [Range(0,1)] public float persistence = 0.6f;
    public int seed;
    public Vector2 offset;

    public void ValidateValues()
    {
        scale = Mathf.Max(scale,0.01f);
        octaves = Mathf.Max(octaves,1);
        lacunarity = Mathf.Max(lacunarity,1);
        persistence = Mathf.Clamp01(persistence);
    }
}

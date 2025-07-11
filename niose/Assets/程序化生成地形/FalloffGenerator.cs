using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFalloff(int size)
    {
        float[,] noiseMap = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;
                
                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                noiseMap[i, j] = Evaluate(value);
            }
        }
        return noiseMap;
    }

    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;
        
        return Mathf.Pow(value,a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}

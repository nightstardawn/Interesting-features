using UnityEngine;

public class PerlinNoise : MonoBehaviour
{
    public int width = 256;
    public int height = 256;
    public float scale = 20f;
    public float xOffset = 0f;
    public float yOffset = 0f;
    void Update()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = GeneratePerlinNoiseTexture(width, height);
    }
    private Texture2D GeneratePerlinNoiseTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = CalculateColor(x,y);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    private Color CalculateColor(int x, int y)
    {
        float xCoord = x/(float)width * scale + xOffset;
        float yCoord = y/(float)height * scale + yOffset;
        float sample = Mathf.PerlinNoise(xCoord, yCoord);
        return new Color(sample, sample, sample);
    }
}

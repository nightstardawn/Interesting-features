Shader "Custom/Terrain"
{
    Properties
    {
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        
        #pragma target 3.0

        const static int maxColourCount = 8;

        int baseColorCount;
        float3 baseColours[maxColourCount];
        float baseStartHeight[maxColourCount];
        
        float minHeight;
        float maxHeight;
        
        struct Input {
            float3 worldPos;
        }; 
        float inverseLerp(float a, float b, float value) {
            return saturate((value-a) / (b-a));
        }
        
        void surf(Input IN, inout SurfaceOutputStandard o) {
            float heightPercent = inverseLerp(minHeight,maxHeight,IN.worldPos.y); 
            for (int i = 0; i < baseColorCount; ++i) {
                float drawStrength = saturate(sign(heightPercent - baseStartHeight[i]));
                o.Albedo = o.Albedo * (1-drawStrength) + baseColours[i] * drawStrength;
            }
        }
        ENDCG
    }
    FallBack "Diffuse"
}
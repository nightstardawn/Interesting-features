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
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }
        LOD 200

        Pass
        {
            Name "SimpleColorPass"
            Tags { "LightMode"="SRPDefaultUnlit" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 worldPos     : TEXCOORD0;
            };

            // 材质属性
            CBUFFER_START(UnityPerMaterial)
                float _MinHeight;
                float _MaxHeight;
                float3 _BaseColors[8];
                float _BaseStartHeights[8];
                int _BaseColorCount;
            CBUFFER_END
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldPos = worldPos;
                return OUT;
            }
            
            float inverseLerp(float a, float b, float value) {
                return saturate((value - a) / (b - a));
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                // 计算高度百分比
                float heightPercent = inverseLerp(_MinHeight, _MaxHeight, IN.worldPos.y);
                half3 finalColor = 0;
                
                // 根据高度混合颜色
                for (int i = 0; i < _BaseColorCount; i++) 
                {
                    // 当前高度高于起始高度时绘制该颜色
                    float drawStrength = saturate(sign(heightPercent - _BaseStartHeights[i]));
                    finalColor = finalColor * (1 - drawStrength) + _BaseColors[i] * drawStrength;
                }
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
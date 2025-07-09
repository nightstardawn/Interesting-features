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
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 200

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            static const int maxColourCount = 8;
            static const float epsilon = 1E-4;

            CBUFFER_START(UnityPerMaterial)
                int _BaseColorCount;
                float4 _BaseColors[maxColourCount];
                float _BaseStartHeights[maxColourCount];
                float _BaseBlends[maxColourCount];
                float _MinHeight;
                float _MaxHeight;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vertexInput.positionCS;
                OUT.positionWS = vertexInput.positionWS;
                return OUT;
            }
            
            float inverseLerp(float a, float b, float value) {
                return saturate((value - a) / (b - a));
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float heightPercent = inverseLerp(_MinHeight, _MaxHeight, IN.positionWS.y); 
                float3 color = float3(0, 0, 0);
                
                for (int i = 0; i < _BaseColorCount; i++)
                {
                    float drawStrength = inverseLerp(
                        -_BaseBlends[i]/2 - epsilon, 
                        _BaseBlends[i]/2, 
                        heightPercent - _BaseStartHeights[i]
                    );
                    color = color * (1 - drawStrength) + _BaseColors[i].rgb * drawStrength;
                }
                
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Packages/com.unity.render-pipelines.universal/FallbackError"
}
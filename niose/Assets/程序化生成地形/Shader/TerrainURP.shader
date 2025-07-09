Shader "Custom/Terrain"
{
    Properties
    {
        // 属性保持空，由脚本控制
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            static const int maxLayerCount = 8;
            static const float epsilon = 1E-4;

            // 纹理数组声明
            TEXTURE2D_ARRAY(_BaseTextures);
            SAMPLER(sampler_BaseTextures);

            CBUFFER_START(UnityPerMaterial)
                int _LayerCount;
                float3 _BaseColors[maxLayerCount];
                float _BaseStartHeights[maxLayerCount];
                float _BaseBlends[maxLayerCount];
                float _BaseColourStrength[maxLayerCount];
                float _BaseTextureScales[maxLayerCount];
                float _MinHeight;
                float _MaxHeight;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vertexInput.positionCS;
                OUT.positionWS = vertexInput.positionWS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }
            
            // 与原Shader完全相同的函数
            float inverseLerp(float a, float b, float value) {
                return saturate((value - a) / (b - a));
            }
            
            // 直接移植的三平面贴图函数
            float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
                float3 scaledWorldPos = worldPos / scale;
                
                // X轴投影
                float3 xProjection = SAMPLE_TEXTURE2D_ARRAY(
                    _BaseTextures, sampler_BaseTextures, 
                    float2(scaledWorldPos.y, scaledWorldPos.z), textureIndex).rgb * blendAxes.x;
                    
                // Y轴投影
                float3 yProjection = SAMPLE_TEXTURE2D_ARRAY(
                    _BaseTextures, sampler_BaseTextures, 
                    float2(scaledWorldPos.x, scaledWorldPos.z), textureIndex).rgb * blendAxes.y;
                    
                // Z轴投影
                float3 zProjection = SAMPLE_TEXTURE2D_ARRAY(
                    _BaseTextures, sampler_BaseTextures, 
                    float2(scaledWorldPos.x, scaledWorldPos.y), textureIndex).rgb * blendAxes.z;
                
                return xProjection + yProjection + zProjection;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // 计算高度百分比
                float heightPercent = inverseLerp(_MinHeight, _MaxHeight, IN.positionWS.y); 
                
                // 计算法线混合权重
                float3 blendAxes = abs(IN.normalWS);
                blendAxes /= (blendAxes.x + blendAxes.y + blendAxes.z);
                
                float3 albedo = float3(0, 0, 0);
                
                // 图层混合循环
                for (int i = 0; i < _LayerCount; i++)
                {
                    // 计算图层强度
                    float drawStrength = inverseLerp(
                        -_BaseBlends[i]/2 - epsilon, 
                        _BaseBlends[i]/2, 
                        heightPercent - _BaseStartHeights[i]
                    );
                    
                    // 基础颜色
                    float3 baseColour = _BaseColors[i] * _BaseColourStrength[i];
                    
                    // 纹理颜色（三平面映射）
                    float3 textureColour = triplanar(
                        IN.positionWS, 
                        _BaseTextureScales[i], 
                        blendAxes, 
                        i
                    ) * (1 - _BaseColourStrength[i]);
                    
                    // 混合当前图层
                    albedo = albedo * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
                }
                
                // 基础光照（仅主光源）
                Light mainLight = GetMainLight();
                float3 diffuse = saturate(dot(IN.normalWS, mainLight.direction)) * mainLight.color;
                
                // 最终颜色输出
                return half4(albedo * diffuse, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
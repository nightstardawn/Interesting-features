Shader "Custom/Terrain"
{
    Properties
    {
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _NoiseScale("Noise Scale", Float) = 0.1
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
            // 添加深度写入和光照设置
            ZWrite On
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            static const int maxColourCount = 8;
            static const float epsilon = 1E-4;

            CBUFFER_START(UnityPerMaterial)
                int _LayerCount;
                float4 _BaseColors[maxColourCount];
                float _BaseStartHeights[maxColourCount];
                float _BaseBlends[maxColourCount];
                float _MinHeight;
                float _MaxHeight;
                
                // 添加法线纹理支持
                sampler2D _NoiseTex;
                float _NoiseScale;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL; // 添加法线输入
                float2 uv : TEXCOORD0; // 添加纹理坐标
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1; // 添加世界空间法线
                float2 uv : TEXCOORD2; // 添加纹理坐标输出
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vertexInput.positionCS;
                OUT.positionWS = vertexInput.positionWS;
                OUT.uv = IN.uv; // 传递纹理坐标
                
                // 计算世界空间法线
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                // 添加高度偏移以解决深度冲突
                #if defined(UNITY_REVERSED_Z)
                    OUT.positionCS.z -= 0.0001;
                #else
                    OUT.positionCS.z += 0.0001;
                #endif
                
                return OUT;
            }
            
            float inverseLerp(float a, float b, float value) {
                return saturate((value - a) / (b - a));
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float heightPercent = inverseLerp(_MinHeight, _MaxHeight, IN.positionWS.y); 
                float3 color = float3(0, 0, 0);
                
                // 添加简单的高度纹理扰动
                float heightNoise = tex2D(_NoiseTex, IN.positionWS.xz * _NoiseScale).r;
                heightPercent += (heightNoise - 0.5) * 0.1;
                
                for (int i = 0; i < _LayerCount; i++)
                {
                    // 添加图层混合抗锯齿
                    float blendRange = max(_BaseBlends[i], 0.01);
                    float drawStrength = inverseLerp(
                        -blendRange/2 - epsilon, 
                        blendRange/2, 
                        heightPercent - _BaseStartHeights[i]
                    );
                    
                    // 添加颜色混合平滑过渡
                    color = color * (1 - smoothstep(0, 1, drawStrength)) + 
                            _BaseColors[i].rgb * smoothstep(0, 1, drawStrength);
                }
                
                // 添加基础光照计算
                Light mainLight = GetMainLight();
                float3 normal = normalize(IN.normalWS);
                float3 lighting = saturate(dot(normal, mainLight.direction)) * mainLight.color;
                lighting += _GlossyEnvironmentColor.rgb * 0.1; // 环境光基础
                
                // 应用光照并确保完全不透明
                half4 finalColor = half4(color * lighting, 1.0);
                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
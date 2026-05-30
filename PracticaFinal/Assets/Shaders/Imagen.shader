Shader "Image"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _CrumpleIntensity ("Intensidad de Arrugas", Range(0, 0.5)) = 0.2
        _CrumpleScale ("Escala de Arrugas", Range(1, 50)) = 15
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float _CrumpleIntensity;
                float _CrumpleScale;
            CBUFFER_END

            #define MAX_IMPACTS 64
            int _ImpactCount;
            float4 _ImpactPos[MAX_IMPACTS];
            float4 _ImpactDir[MAX_IMPACTS]; 
            float4 _ImpactData[MAX_IMPACTS];

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0; 
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float2 uv          : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(IN.normalOS);
                worldNormal = normalize(worldNormal);

                for (int i = 0; i < _ImpactCount; i++)
                {
                    float dist = distance(worldPos, _ImpactPos[i].xyz);
                    float currentRadius = _ImpactData[i].x;
                    float currentStrength = _ImpactData[i].y;

                    if (dist < currentRadius)
                    {
                        float falloff = saturate(1.0 - (dist / currentRadius));
                        
                        float wrinkles = sin(worldPos.x * _CrumpleScale) * cos(worldPos.y * _CrumpleScale) * sin(worldPos.z * _CrumpleScale);
                        float edgeTension = smoothstep(0.1, 0.9, falloff) * (1.0 - falloff);
                        
                        float finalFalloff = falloff + (wrinkles * _CrumpleIntensity * edgeTension);
                        finalFalloff = saturate(finalFalloff * finalFalloff);

                        worldPos += _ImpactDir[i].xyz * (currentStrength * finalFalloff);
                    }
                }

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.normalWS = worldNormal;
                OUT.positionWS = worldPos;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                
                return OUT;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return tex * _Color;
            }
            ENDHLSL
        }
    }
    FallBack "Invisible"
}
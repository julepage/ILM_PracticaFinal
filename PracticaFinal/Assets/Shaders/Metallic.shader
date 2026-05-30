Shader "Custom/Metallic"
{
    Properties
    {
        _Color ("Color de la Pintura", Color) = (1, 0, 0, 1)
        _Metallic ("Metalizado", Range(0, 1)) = 0.9
        _Smoothness ("Brillo / Suavizado", Range(0, 1)) = 0.85
        _ReflectionStrength ("Intensidad de Reflejo", Float) = 1.2
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
                float3 viewDirWS   : TEXCOORD3;
                float2 uv          : TEXCOORD4;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Metallic;
                half _Smoothness;
                float _ReflectionStrength;
                float _CrumpleIntensity;
                float _CrumpleScale;
            CBUFFER_END

            #define MAX_IMPACTS 64
            int _ImpactCount;
            float4 _ImpactPos[MAX_IMPACTS];
            float4 _ImpactDir[MAX_IMPACTS]; 
            float4 _ImpactData[MAX_IMPACTS];

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
                OUT.viewDirWS = GetWorldSpaceViewDir(worldPos);
                OUT.uv = IN.uv;
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                Light light = GetMainLight();
                float3 L = normalize(light.direction);

                half NdotL = saturate(dot(N, L));
                half3 ambient = SampleSH(N);
                half3 diffuse = (light.color * NdotL) + ambient;

                float3 H = normalize(L + V);
                half NdotH = saturate(dot(N, H));
                half specular = pow(NdotH, _Smoothness * 128.0) * _Smoothness;

                half fresnel = pow(1.0 - saturate(dot(N, V)), 4.0);

                float3 R = reflect(-V, N);
                half4 env = SAMPLE_TEXTURECUBE(unity_SpecCube0, samplerunity_SpecCube0, R);

                float3 baseReflection = env.rgb * _ReflectionStrength;
                float3 materialColor = _Color.rgb * diffuse;

                float3 finalMetallic = lerp(materialColor, baseReflection, _Metallic * (0.2 + fresnel * 0.8));
                float3 finalColor = finalMetallic + (light.color * specular * _Smoothness);

                return half4(finalColor, _Color.a);
            }
            ENDHLSL
        }

    Pass
    {
        Name "ShadowCaster"
        Tags { "LightMode" = "ShadowCaster" }

        ZWrite On
        ZTest LEqual
        ColorMask 0

        HLSLPROGRAM
        #pragma vertex vert
        #pragma fragment fragShadow

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float3 normalWS : TEXCOORD0;
            float3 viewDirWS : TEXCOORD1;
        };

        Varyings vert (Attributes v)
        {
            Varyings o;
            o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
            o.viewDirWS = float3(0, 0, 0);
            return o;
        }

        half4 fragShadow(Varyings IN) : SV_Target
        {
            return 0;
        }
        ENDHLSL
    }
    }
    FallBack "Invisible"
}
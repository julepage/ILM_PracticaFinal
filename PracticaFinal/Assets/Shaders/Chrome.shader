Shader "Custom/Chrome"
{
    Properties
    {
        _Color1 ("Color Frontal (Cian)", Color) = (0.1, 0.6, 1.0, 1)
        _Color2 ("Color Lateral (Magenta)", Color) = (0.9, 0.2, 0.9, 1)
        _FresnelPower ("Cambio de Color (Fresnel)", Float) = 3.0
        _ReflectionStrength ("Intensidad del Reflejo", Float) = 1.5
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
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color1;
                float4 _Color2;
                float _FresnelPower;
                float _ReflectionStrength;
                float _CrumpleIntensity;
                float _CrumpleScale;
            CBUFFER_END

            #define MAX_IMPACTS 64
            int _ImpactCount;
            float4 _ImpactPos[MAX_IMPACTS];
            float4 _ImpactDir[MAX_IMPACTS]; 
            float4 _ImpactData[MAX_IMPACTS];

            Varyings vert (Attributes v)
            {
                Varyings o;

                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(v.normalOS);
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

                o.positionHCS = TransformWorldToHClip(worldPos);
                o.normalWS = worldNormal;
                o.viewDirWS = GetWorldSpaceViewDir(worldPos);

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 N = normalize(i.normalWS);
                float3 V = normalize(i.viewDirWS);

                Light light = GetMainLight();
                float3 L = normalize(light.direction);

                float fresnelFlip = pow(1.0 - saturate(dot(N, V)), _FresnelPower);
                float3 paintColor = lerp(_Color1.rgb, _Color2.rgb, fresnelFlip);

                float3 R = reflect(-V, N);
                half4 env = SAMPLE_TEXTURECUBE(unity_SpecCube0, samplerunity_SpecCube0, R);

                float3 reflectionColor = env.rgb * paintColor * _ReflectionStrength;

                float3 halfDir = normalize(L + V);
                float NdotH = saturate(dot(N, halfDir));
                float spec = pow(NdotH, 80.0) * 0.6;

                float NdotL = saturate(dot(N, L));
                float3 diffuse = paintColor * (NdotL * light.color + SampleSH(N));

                float fresnelReflect = pow(1.0 - saturate(dot(N, V)), 3.0);
                float3 finalColor = lerp(diffuse, reflectionColor, 0.6 + fresnelReflect * 0.4);
                finalColor += light.color * spec;

                return float4(finalColor, 1);
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
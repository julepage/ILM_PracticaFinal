Shader "Custom/Mate"
{
    Properties
    {
        [MainColor] _BaseColor("Color de la Pintura", Color) = (0.05, 0.05, 0.05, 1)
        _Smoothness("Ancho del Reflejo (Cielo/Sol)", Range(0.01, 0.5)) = 0.08
        _SpecIntensity("Intensidad del Reflejo", Range(0.0, 0.5)) = 0.12
        _RimIntensity("Claridad en Bordes (Fresnel)", Range(0.0, 1.0)) = 0.35
        _RimPower("Concentracion en Bordes", Range(1.0, 6.0)) = 2.5
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
                float3 viewDirWS   : TEXCOORD2;
                float3 positionWS  : TEXCOORD3;
                float2 uv          : TEXCOORD4;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Smoothness;
                half _SpecIntensity;
                half _RimIntensity;
                half _RimPower;
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
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(IN.viewDirWS);

                Light light = GetMainLight();
                float3 lightDirWS = normalize(light.direction);

                half NdotL = saturate(dot(normalWS, lightDirWS));
                half3 iluminacionAmbiental = SampleSH(normalWS);
                half3 luzTotalDifusa = (light.color * NdotL) + iluminacionAmbiental;

                float3 halfDir = normalize(lightDirWS + viewDirWS);
                half NdotH = saturate(dot(normalWS, halfDir));
                half especular = pow(NdotH, _Smoothness * 128.0) * _SpecIntensity;

                half fresnel = 1.0 - saturate(dot(normalWS, viewDirWS));
                half3 reflejoBordes = pow(fresnel, _RimPower) * _RimIntensity * (iluminacionAmbiental + light.color * 0.5);

                half3 colorFinal = (_BaseColor.rgb * luzTotalDifusa) + (light.color * especular) + reflejoBordes;

                return half4(colorFinal, _BaseColor.a);
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
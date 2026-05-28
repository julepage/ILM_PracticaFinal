Shader "Custom/StealthMattePerfecto"
{
    Properties
    {
        [MainColor] _BaseColor("Color de la Pintura", Color) = (0.05, 0.05, 0.05, 1)
        
        [Header(Reflejo de Luz Difuminado)]
        _Smoothness("Ancho del Reflejo (Cielo/Sol)", Range(0.01, 0.5)) = 0.08
        _SpecIntensity("Intensidad del Reflejo", Range(0.0, 0.5)) = 0.12
        
        [Header(Iluminacion de Bordes Curvos)]
        _RimIntensity("Claridad en Bordes (Fresnel)", Range(0.0, 1.0)) = 0.35
        _RimPower("Concentracion en Bordes", Range(1.0, 6.0)) = 2.5
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
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD1;
                float3 viewDirWS   : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Smoothness;
                half _SpecIntensity;
                half _RimIntensity;
                half _RimPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(positionWS);
                
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
    }
}
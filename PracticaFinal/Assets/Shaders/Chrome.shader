Shader "Custom/MetallicChrome"
{
    Properties
    {
        _Color1 ("Color base (gris metal)", Color) = (0.7,0.7,0.7,1)
        _Color2 ("Color iridiscente", Color) = (1,0,1,1)
        _FresnelPower ("Fresnel Power", Float) = 8
        _ReflectionStrength ("Reflection Strength", Float) = 1.5
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float4 _Color1;
            float4 _Color2;
            float _FresnelPower;
            float _ReflectionStrength;

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

                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);

                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.viewDirWS = GetWorldSpaceViewDir(positionWS);

                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 N = normalize(i.normalWS);
                float3 V = normalize(i.viewDirWS);

                //Fresnel 
                float fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);

                // base más metálica 
                float3 baseColor = _Color1.rgb;

                //iridiscencia solo en ángulos
                baseColor = lerp(baseColor, _Color2.rgb, fresnel * 0.6);

                // reflexión del entorno (CLAVE DEL METAL)
                float3 R = reflect(-V, N);
                half4 env = SAMPLE_TEXTURECUBE(unity_SpecCube0, samplerunity_SpecCube0, R);

                // mezcla metálica real
                float3 finalColor = lerp(baseColor, env.rgb, fresnel * _ReflectionStrength);

                //  brillo especular extra (cromo)
                float spec = pow(fresnel, 3);
                finalColor += spec;

                return float4(finalColor, 1);
            }

            ENDHLSL
        }
    }
}
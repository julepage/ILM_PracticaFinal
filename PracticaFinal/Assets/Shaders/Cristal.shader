Shader "Custom/GlassShatter64"
{
    Properties
    {
        _BaseColor("Color del Cristal", Color) = (0.2, 0.3, 0.3, 0.15)
        _Smoothness("Brillo del Cristal", Range(0, 1)) = 0.95
        _CrackIntensity("Intensidad de Grietas", Range(0.5, 5.0)) = 2.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On

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
                float3 normalWS    : TEXCOORD0;
                float3 viewDirWS   : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Smoothness;
                float _CrackIntensity;
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
                
                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = worldPos;
                OUT.viewDirWS = GetWorldSpaceViewDir(worldPos);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);

                Light light = GetMainLight();
                float3 L = normalize(light.direction);

                float cracks = 0.0;
                float3x3 worldToLocalMat = (float3x3)GetWorldToObjectMatrix();

                for (int i = 0; i < _ImpactCount; i++)
                {
                    float3 impactPoint = _ImpactPos[i].xyz;
                    float radius = _ImpactData[i].x;
                    float strength = _ImpactData[i].y;

                    float dist = distance(IN.positionWS, impactPoint);

                    if (dist < radius)
                    {
                        float falloff = saturate(1.0 - (dist / radius));
                        
                        float3 dirToPixelWS = IN.positionWS - impactPoint;
                        float3 dirToPixelLS = mul(worldToLocalMat, dirToPixelWS);
                        float3 normDirLS = normalize(dirToPixelLS);
                        
                        float angle = atan2(normDirLS.y, normDirLS.x);
                        float sectors = 14.0;
                        float angleSector = floor(angle * sectors) / sectors;

                        float radialNoise = sin(floor(dist * 35.0) * 92.13) * 0.15;
                        float radial = 1.0 - saturate(abs(sin(angle * sectors + radialNoise)) * 25.0);

                        float ringStep = sin(angleSector * 425.81) * 0.06;
                        float concentric = 1.0 - saturate(abs(sin((dist + ringStep) * 180.0)) * 18.0);

                        float centerPulverized = saturate(1.0 - (dist * 45.0)) * 2.0;
                        float microCracks = (1.0 - saturate(abs(sin(angle * 28.0 + dist * 60.0)) * 4.0)) * saturate(1.0 - dist * 5.0);

                        float combinedCracks = max(radial, concentric);
                        combinedCracks = max(combinedCracks, centerPulverized);
                        combinedCracks = max(combinedCracks, microCracks);

                        cracks += combinedCracks * falloff * strength * _CrackIntensity;
                    }
                }

                cracks = saturate(cracks);

                half NdotL = saturate(dot(N, L));
                half3 ambient = SampleSH(N);
                half3 diffuseLight = (light.color * NdotL) + ambient;

                float3 H = normalize(L + V);
                half NdotH = saturate(dot(N, H));
                half specular = pow(NdotH, _Smoothness * 128.0) * _Smoothness * 0.6;

                float3 R = reflect(-V, N);
                half4 env = SAMPLE_TEXTURECUBE(unity_SpecCube0, samplerunity_SpecCube0, R);

                half3 glassColor = _BaseColor.rgb * diffuseLight + env.rgb * _Smoothness;
                half3 crackColor = half3(0.95, 0.97, 0.97) * (diffuseLight + 0.5); 

                half3 finalColor = lerp(glassColor, crackColor, cracks) + (light.color * specular * (1.0 - cracks));
                half finalAlpha = lerp(_BaseColor.a, 0.98, cracks);

                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Transparent/Diffuse"
}
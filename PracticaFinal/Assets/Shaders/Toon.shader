Shader "Custom/TemplateCarDeform64_Toon_ConOutline"
{
    Properties
    {
        _DefaultColor("Color Base de la Plantilla", Color) = (0.5, 0.5, 0.5, 1)
        _Steps("Número de Escalones Toon", Range(2, 10)) = 4
        _AmbientIntensity("Intensidad Ambiente", Range(0, 1)) = 0.2

        [Header(Configuracion del Outline Negro)]
        _OutlineThickness("Grosor del Borde", Range(0, 0.5)) = 0.05
        _OutlineColor("Color del Borde", Color) = (0, 0, 0, 1)

        [Header(Configuracion de Arrugas de Metal)]
        _CrumpleIntensity ("Intensidad de Arrugas", Range(0, 0.5)) = 0.2
        _CrumpleScale ("Escala de Arrugas", Range(1, 50)) = 15
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        //solo bordes negros
        Pass
        {
            Name "Outline"
            Cull Front //para render de caras expandidas
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _DefaultColor;
                float _Steps;
                float _AmbientIntensity;
                float _OutlineThickness;
                float4 _OutlineColor;
                float _CrumpleIntensity;
                float _CrumpleScale;
            CBUFFER_END

            //para el outline
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

                //borde negro a panrtalla
                float4 posCS = TransformWorldToHClip(worldPos);
                float3 normalCS = TransformWorldToHClipDir(worldNormal);

                //expando borde negro
                float2 offset = normalize(normalCS.xy) * _OutlineThickness * posCS.w * 0.1;
                posCS.xy += offset;

                OUT.positionHCS = posCS;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_TARGET
            {
                return _OutlineColor;//borde negro
            }
            ENDHLSL
        }

        //solo color toon
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back //render caras de fuera

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0; 
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

           
            float _Steps;
            float _AmbientIntensity;
            half4 _DefaultColor;
            float _CrumpleIntensity;
            float _CrumpleScale;

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
                OUT.uv = IN.uv;
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_TARGET
            {
                float3 normalWS = normalize(IN.normalWS);   
                
                //iluminacion toon
                Light mainLight = GetMainLight();
                float ndotl = dot(normalWS, mainLight.direction);
                float halfLambert = ndotl * 0.5 + 0.5;

                float toonIntensity = floor(halfLambert * _Steps) / _Steps;
                toonIntensity = max(toonIntensity, _AmbientIntensity);

                float3 toonColor = _DefaultColor.rgb * mainLight.color * toonIntensity;

                //color mas vivo
                half3 colorFinal = pow(toonColor, 1.0 / 2.2);

                return half4(colorFinal, _DefaultColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Invisible"

}
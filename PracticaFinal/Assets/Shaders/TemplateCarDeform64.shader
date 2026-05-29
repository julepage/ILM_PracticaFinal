//NECESARIO USAR ColisionCar.cs en el objeto que contenga el rigidbody (este objeto debe ser padre de los objetos cuyos materiales desees modificar);
Shader "Custom/TemplateCarDeform64"
{
    Properties
    {
        // Añade aquí las texturas, colores o sliders que quieras usar en tu Fragment Shader.
        _DefaultColor("Color Base de la Plantilla", Color) = (0.5, 0.5, 0.5, 1)

        // NO MODIFICAR: CONFIGURACIÓN DE LA DEFORMACIÓN FÍSICA
        [Header(Configuracion de Arrugas de Metal)]
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
            
            //Descomentar para luces del mundo
            // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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

            CBUFFER_START(UnityPerMaterial)
                half4 _DefaultColor;
                float _CrumpleIntensity;
                float _CrumpleScale;
            CBUFFER_END

            // Variables globales de los 64 impactos enlazadas con tu script C#
            #define MAX_IMPACTS 64
            int _ImpactCount;
            float4 _ImpactPos[MAX_IMPACTS];
            float4 _ImpactDir[MAX_IMPACTS]; 
            float4 _ImpactData[MAX_IMPACTS]; // X = Radio, Y = Fuerza

            // VERTEX SHADER (FIJO / NO TOCAR)
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
                        
                        // Matemática de arrugas y tensión de la chapa
                        float wrinkles = sin(worldPos.x * _CrumpleScale) * cos(worldPos.y * _CrumpleScale) * sin(worldPos.z * _CrumpleScale);
                        float edgeTension = smoothstep(0.1, 0.9, falloff) * (1.0 - falloff);
                        
                        float finalFalloff = falloff + (wrinkles * _CrumpleIntensity * edgeTension);
                        finalFalloff = saturate(finalFalloff * finalFalloff);

                        // Desplazamos el vértice en la dirección real de la colisión física
                        worldPos += _ImpactDir[i].xyz * (currentStrength * finalFalloff);
                    }
                }

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.normalWS = worldNormal;
                OUT.positionWS = worldPos;
                OUT.uv = IN.uv;
                
                return OUT;
            }

            // Diseña aquí dentro el comportamiento visual / renderizado que quieras.
            half4 frag(Varyings IN) : SV_Target
            {
                // variables precalculadas listas para usar en tus fórmulas:
                float3 normalWS = normalize(IN.normalWS);   // La normal de la malla (cambia dinámicamente con los bollos)
                float3 positionWS = IN.positionWS;         // La posición exacta de este píxel en el mundo 3D
                float2 uv = IN.uv;                         // Las coordenadas UV del mapeado del objeto

               
                // ESCRIBE AQUÍ TU CÓDIGO VISUAL PERSONALIZADO:

                half3 colorFinal = _DefaultColor.rgb; //Actualmente solo muestra el color plano grisáceo

                // Devuelve el color final en formato RGBA (Rojo, Verde, Azul, Transparencia)
                return half4(colorFinal, _DefaultColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Invisible"
}
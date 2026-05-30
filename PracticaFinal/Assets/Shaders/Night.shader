Shader "Custom/NightSky"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Speed ("Velocidad", Float) = 0.002
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Background" "RenderType" = "Background" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _Color;
            float _Speed;

            struct Attributes
            {
                float4 positionOS: POSITION;
            };

            struct Varyings
            {
                float4 positionHCS: SV_POSITION;
                float3 dir: TEXCOORD0;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.dir = normalize(v.positionOS.xyz);
                return o;
             }

            half4 frag (Varyings i) : SV_Target
            {
                //para el skybox
                float2 uv;
                uv.x = atan2(i.dir.x, i.dir.z) / (2.0 * PI) + 0.5;
                uv.y = i.dir.y * 0.5 + 0.5;

                //mov lento
                uv.x += _Time.y * _Speed;
                uv.x = frac(uv.x);
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                return tex * _Color;
            }

            ENDHLSL
        }
    }
}
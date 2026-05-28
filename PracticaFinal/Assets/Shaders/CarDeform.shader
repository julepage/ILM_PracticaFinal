// Shader "Custom/SimpleDentMulti"
// {
//     Properties
//     {
//         _MainTex ("Texture", 2D) = "white" {}
//     }

//     SubShader
//     {
//         Tags { "RenderType"="Opaque" }

//         Pass
//         {
//             CGPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag

//             #include "UnityCG.cginc"

//             sampler2D _MainTex;
//             float4 _MainTex_ST;

//             #define MAX_IMPACTS 8

//             int _ImpactCount;
//             float4 _ImpactPos[MAX_IMPACTS];
//             float _Radius[MAX_IMPACTS];
//             float _Strength[MAX_IMPACTS];

//             struct appdata
//             {
//                 float4 vertex : POSITION;
//                 float3 normal : NORMAL;
//                 float2 uv : TEXCOORD0;
//             };

//             struct v2f
//             {
//                 float4 pos : SV_POSITION;
//                 float2 uv : TEXCOORD0;
//             };

//             v2f vert(appdata v)
//             {
//                 v2f o;

//                 float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
//                 float3 worldNormal = UnityObjectToWorldNormal(v.normal);

//                 for (int i = 0; i < _ImpactCount; i++)
//                 {
//                     float dist = distance(worldPos, _ImpactPos[i].xyz);

//                     float w = saturate(1 - dist / _Radius[i]);
//                     w = w * w;

//                     worldPos -= worldNormal * (_Strength[i] * w);
//                 }

//                 o.pos = UnityWorldToClipPos(worldPos);
//                 o.uv = TRANSFORM_TEX(v.uv, _MainTex);

//                 return o;
//             }

//             fixed4 frag(v2f i) : SV_Target
//             {
//                 return tex2D(_MainTex, i.uv);
//             }
//             ENDCG
//         }
//     }
// }
Shader "Custom/CarDeform"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ImpactPos ("Impact Pos", Vector) = (0,0,0,0)
        _Radius ("Radius", Float) = 1
        _Strength ("Strength", Float) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float3 _ImpactPos;
            float _Radius;
            float _Strength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                float dist = distance(worldPos, _ImpactPos);

                float falloff = saturate(1 - dist / _Radius);
                falloff = falloff * falloff;

                worldPos += float3(0, -1, 0) * falloff * _Strength;

                o.pos = UnityWorldToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }

            ENDCG
        }
    }
}
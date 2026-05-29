Shader "Custom/MirrorRetrovisor"
{
    Properties
    {
        _ReflectionTex ("Reflection Texture", 2D) = "white" {} //aqui le meto el render tecture luego por codigo
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS: POSITION;
            };

            struct Varyings
            {
                float4 positionCS: SV_POSITION;
                float4 screenPos: TEXCOORD0;
            };

            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);//lo de siempre
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float2 uvEspejo = input.screenPos.xy / input.screenPos.w;
                uvEspejo.x = 1.0 - uvEspejo.x;//efecto espejo invertido 
                return SAMPLE_TEXTURE2D(_ReflectionTex, sampler_ReflectionTex, uvEspejo);
            }
            ENDHLSL
        }
    }
}
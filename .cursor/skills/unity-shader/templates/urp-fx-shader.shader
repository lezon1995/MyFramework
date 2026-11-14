Shader "MyGame/Effects/FxAdditive"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 8)) = 1
        _SoftFade ("Soft Edge Fade", Range(0, 1)) = 0.15
        _ZWrite ("ZWrite", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ForwardAdditive"
            Tags { "LightMode" = "UniversalForward" }

            Blend One One            // 加法混合；如需 SrcAlpha OneMinusSrcAlpha 请改
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma shader_feature_local _SOFT_EDGE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Intensity;
                half _SoftFade;
                half _ZWrite;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = pos.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.fogFactor = ComputeFogFactor(pos.positionCS.z);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half4 color = tex * _BaseColor * _Intensity;

                #ifdef _SOFT_EDGE
                // 边缘柔化，避免硬边
                half2 d = abs(IN.uv - 0.5) * 2.0;
                half edge = saturate(1.0 - max(d.x, d.y));
                color.a *= smoothstep(0.0, _SoftFade, edge);
                #endif

                color.rgb = MixFog(color.rgb, IN.fogFactor);
                return color;
            }
            ENDHLSL
        }
    }

    Fallback Off
}

Shader "Gameplay/Sprite Instanced"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _GameplayColor("Instance Color", Color) = (1,1,1,1)
        _GameplayFlip("Instance Flip", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _Color;
            UNITY_INSTANCING_BUFFER_START(GameplaySprite)
                UNITY_DEFINE_INSTANCED_PROP(float4, _GameplayColor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _GameplayFlip)
            UNITY_INSTANCING_BUFFER_END(GameplaySprite)
            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                input.positionOS.xy *= UNITY_ACCESS_INSTANCED_PROP(GameplaySprite, _GameplayFlip).xy;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                output.color = _Color * UNITY_ACCESS_INSTANCED_PROP(GameplaySprite, _GameplayColor);
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                color.rgb *= color.a;
                return color;
            }
            ENDHLSL
        }
    }
}

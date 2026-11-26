Shader "Gameplay/Drop Mesh Particle Instanced"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _ColumnsX("Columns", Float) = 1
        _RowsY("Rows", Float) = 1
        _AnimationSpeed("Frames Per Second", Float) = 0
        _Cutoff("Alpha Cutoff (negative disables)", Float) = -1
        _UseVertexColor("Use Sprite Color", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Float) = 10
        [Toggle] _ZWrite("Depth Write", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull Off
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:ParticleInstancingSetup
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ParticlesInstancing.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float _ColumnsX, _RowsY, _AnimationSpeed, _Cutoff, _UseVertexColor;
            CBUFFER_END
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                #if defined(UNITY_PARTICLE_INSTANCING_ENABLED)
                    uint c = unity_ParticleInstanceData[unity_InstanceID].color;
                    output.color = float4(c & 255, (c >> 8) & 255, (c >> 16) & 255, (c >> 24) & 255) / 255.0;
                #endif
                output.color = lerp(half4(1,1,1,1), output.color, _UseVertexColor);
                uint columns = (uint)max(1, _ColumnsX), rows = (uint)max(1, _RowsY);
                uint frame = (uint)(_Time.y * _AnimationSpeed);
                uint x = frame % columns, y = (frame % (columns * rows)) / columns;
                output.uv = (input.uv + float2(x, rows - 1 - y)) / float2(columns, rows);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * input.color;
                clip(color.a - _Cutoff);
                // Sprites/Default uses premultiplied alpha. The cutout coin has opaque blending.
                if (_Cutoff < 0) color.rgb *= color.a;
                return color;
            }
            ENDHLSL
        }
    }
}

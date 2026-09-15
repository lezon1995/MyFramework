// 冲刺残影 / SpriteAfterImage
// 衰减由 C# 端驱动:每帧把残影材质的 _Alpha 乘以 _AlphaMultiplier 实现淡出。
// shader 本身是无状态的,所以 _Alpha 必须由 CPU 端每帧推一次新值。
//
// 接入方式：
//   1) 用本 Shader 建一个 Material
//   2) 把该材质拖到 SpriteAfterImageEmitter.afterImageMaterial
//   3) 在 PlayerDash2D.DashStart() 调用 emitter.Begin(),
//      在 DashStop() 调用 emitter.End()
//
// 兼容 Built-in / URP (URP 需把 Tags 改为 "RenderPipeline"="UniversalPipeline")

Shader "Game/SpriteAfterImage"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)
        _Alpha("Alpha (driven by C#)", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "DisableBatching" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Alpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 src = tex2D(_MainTex, i.uv);

                fixed4 outCol;
                outCol.rgb = src.rgb * _Color.rgb * i.color.rgb;
                outCol.a   = src.a * _Color.a * _Alpha * i.color.a;

                clip(outCol.a - 0.01);
                return outCol;
            }
            ENDCG
        }
    }

    Fallback Off
}
Shader "Game/SectorShockwave"
{
    Properties
    {
        [Header(Ring)]
        _MainTex        ("Ring Texture (圆形环/冲击波贴图)", 2D) = "white" {}
        _Color          ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0,4)) = 1.5

        [Header(Sector)]
        _SectorAngle    ("Sector Angle (0~1 = 0~360°)", Range(0,1)) = 0.5
        _SectorOffset   ("Sector Offset (旋转/起始方向)", Range(0,1)) = 0
        _EdgeSoftness   ("Edge Softness", Range(0.001,0.5)) = 0.05

        [Header(Expansion)]
        _Progress ("Progress (0=未扩散,1=完全扩散)", Range(0,1)) = 0
        _RingWidth      ("Ring Width (UV半径范围)", Range(0.01,1)) = 0.25 }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha      // 普通透明；如果要叠加发光改 Additive
        // Blend One One // Additive 叠加        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Intensity;

            float _SectorAngle;
            float _SectorOffset;
            float _EdgeSoftness;

            float _Progress;
            float _RingWidth;

            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f     { float4 pos:SV_POSITION; float2 uv:TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 c = uv - 0.5; // 以中心为原点
                float  r  = length(c) * 2.0;                // 0~1，越外越大（贴图外圈=1）
                float  a  = atan2(c.y, c.x);                // -π ~ π
                float  a01 = a / (2.0 * UNITY_PI) + 0.5;    // 0~1

                // —— 角度扇区遮罩（中心对称 / 连续扇形）——
                float diff = a01 - _SectorOffset;
                diff = frac(diff + 0.5) - 0.5;              // 环绕到 [-0.5, 0.5]
                float angleMask = step(abs(diff), _SectorAngle * 0.5);
                float edge      = 1.0 - smoothstep(
 _SectorAngle * 0.5 - _EdgeSoftness,
                                       _SectorAngle * 0.5,
                                       abs(diff));

                // —— 扩散环：从内向外推开一个 ring ——
                float ringCenter = lerp(0.0, 1.0 - _RingWidth, _Progress);
                float ring       = 1.0 - smoothstep(_RingWidth, _RingWidth + 0.02,
 abs(r - ringCenter));

                // —— 圆环贴图采样（用 r 当 U, a 当 V 重映射即可保留花纹）——
                // 简化：直接采原 UV，靠上面的 mask 切扇形
                fixed4 col = tex2D(_MainTex, uv);
                col.rgb  *= _Color.rgb * _Intensity;

                float mask = angleMask * edge * ring;
                col.a *= mask;

                return col;
            }
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
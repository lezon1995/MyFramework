// 序列帧播放 Shader
// 与同目录 FlipBook.shader 编译风格完全对齐：所有 uniform 用 float/uint
// 图片按行排列，行优先（左→右，上→下），第 0 帧在左上角

Shader "Game/SpriteAnimOnce"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)

        [Header(Layout)]
        _Columns("Columns (水平帧数)", int) = 1
        _Rows("Rows    (垂直帧数)", int) = 1

        [Header(Playback)]
        _FrameRate("Frame Rate (FPS)", float) = 12
        _Speed("Speed", float) = 1.0

        [Header(Progress Override)]
        // >= 0 时由外部脚本控制（0 = 第 0 帧，1 = 最后一帧）
        // <  0 时由 _Time 自动驱动
        _Progress ("Progress", float) = 0
    }
    SubShader
    {
        Tags
        {
            /*"Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "LightMode" = "ForwardBase"*/

            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "RenderType" = "TransparentCutout"
            "DisableBatching" = "True"
        }

        LOD 100

        Cull Off
        Lighting Off
        ZWrite Off
        Offset -1, -1
        Fog
        {
            Mode Off
        }
        ColorMask RGB
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
            fixed4 _Color;

            uint _Columns;
            uint _Rows;
            float _FrameRate;
            float _Speed;
            float _Progress;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // get single sprite size
                float2 size = float2(1.0f / _Columns, 1.0f / _Rows);
                uint totalFrames = _Columns * _Rows;

                // ---- 帧索引 ----
                // _Progress >= 0 → 外部驱动，映射到 [0, totalFrames-1]
                // _Progress <  0 → 自动模式，_Time.y * Speed / FrameRate
                float t = _Time.y * _Speed / max(_FrameRate, 0.01f);
                float frameFloat = _Progress * (totalFrames - 1.0f);

                // 循环 / 单次（float 版 fmod，与 FlipBook 风格对齐）
                frameFloat = clamp(frameFloat, 0.0f, totalFrames - 1.0f);

                // ---- UV 换算（与 FlipBook 完全一致的 UV 翻转约定）----
                float indexX = fmod(frameFloat, _Columns);
                float indexY = floor(frameFloat / _Columns);

                // FlipBook 中 offset.y 为负值，newUV.y 翻转后叠加到顶部
                float2 offset = float2(size.x * indexX, -size.y * indexY);
                float2 newUV = v.uv * size;
                newUV.y = newUV.y + size.y * (_Rows - 1.0f);

                o.uv = newUV + offset;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 srcColor = tex2D(_MainTex, i.uv);
                srcColor.rgb *= _Color.rgb * i.color.rgb;
                srcColor.a *= _Color.a * i.color.a;
                return srcColor;
            }
            ENDCG
        }
    }
}
// 序列帧播放 Shader
// 与同目录 FlipBook.shader 编译风格完全对齐：所有 uniform 用 float/uint
// 图片按行排列，行优先（左→右，上→下），第 0 帧在左上角

Shader "Game/SpriteAnimLoop"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1, 1, 1, 1)

        [Header(Layout)]
        _Columns("Columns (水平帧数)", int) = 1
        _Rows("Rows    (垂直帧数)", int) = 1

        [Header(Playback)]
        _AnimationSpeed("Frames Per Seconds", float) = 10
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
            float _AnimationSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // get single sprite size
                float2 size = float2(1.0f / _Columns, 1.0f / _Rows);
                uint totalFrames = _Columns * _Rows;

                 // use timer to increment index
                uint index = _Time.y * _AnimationSpeed;

                // ---- UV 换算（与 FlipBook 完全一致的 UV 翻转约定）----
                uint indexX = index % _Columns;
                uint indexY = floor((index % totalFrames) / _Columns);

                // FlipBook 中 offset.y 为负值，newUV.y 翻转后叠加到顶部
                float2 offset = float2(size.x * indexX, -size.y * indexY);
                float2 newUV = v.uv * size;
                newUV.y = newUV.y + size.y * (_Rows - 1);

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
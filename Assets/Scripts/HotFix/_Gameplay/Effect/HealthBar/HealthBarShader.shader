Shader "Game/HealthBar"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}

        // --- 前景层：当前实际血量 ---
        [Header(Foreground (Current HP))]
        _ForegroundColor ("Foreground Color", Color) = (1, 0, 0, 1)
        _ForegroundProgress ("Foreground Progress [0,1]", Range(0, 1)) = 1
        _FillOrigin ("Fill Origin", Int) = 0 // 0=Left, 1=Right, 2=Bottom, 3=Top
        _FlipHorizontal ("Flip Horizontal", Int) = 0
        _FlipVertical ("Flip Vertical", Int) = 0

        // --- 背景层：缓冲血量（延迟扣血效果） ---
        [Header(Background (Buffer HP))]
        [Toggle] _UseBuffer ("Enable Buffer Layer", Int) = 1
        _BufferColor ("Buffer Color", Color) = (0.8, 0.8, 0.8, 1)
        _BufferProgress ("Buffer Progress [0,1]", Range(0, 1)) = 1
        _BufferOffset ("Buffer Offset [0,1]", Range(0, 0.5)) = 0.03
        _BufferInnerFade ("Buffer Inner Edge Fade", Range(0, 0.05)) = 0.01
        _BufferOuterFade ("Buffer Outer Edge Fade", Range(0, 0.05)) = 0.005

        // --- 前景遮罩：尖角斜切样式 ---
        [Header(Foreground Mask Style)]
        [Toggle] _UseChamfer ("Chamfered Corner", Int) = 0
        _ChamferSize ("Chamfer Size", Range(0, 0.15)) = 0.08

        // --- 描边 ---
        [Header(Border)]
        [Toggle] _UseBorder ("Draw Border", Int) = 0
        _BorderColor ("Border Color", Color) = (0, 0, 0, 1)
        _BorderWidth ("Border Width [0,0.5]", Range(0, 0.5)) = 0.05
        _BorderSqueeze ("Border Squeeze [0,1]", Range(0, 0.5)) = 0

        // --- 高级 ---
        [Header(Advanced)]
        [Toggle] _UseAntiFlicker ("Anti-Flicker (Buffer Jitter Fix)", Int) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
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
            #pragma multi_compile_instancing
            #pragma multi_compile __ _USEBUFFER_ON
            #pragma multi_compile __ _USEBORDER_ON
            #pragma multi_compile __ _USECHAMFER_ON
            #pragma multi_compile __ _FLIPHORIZONTAL_ON
            #pragma multi_compile __ _FLIPVERTICAL_ON
            #pragma multi_compile __ _USEANTIFLICKER_ON

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // --- Properties ---
            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _ForegroundColor;
            half _ForegroundProgress;

            fixed4 _BufferColor;
            half _BufferProgress;
            half _BufferOffset;
            half _BufferInnerFade;
            half _BufferOuterFade;

            half _FlipHorizontal;
            half _FlipVertical;

            half _UseChamfer;
            half _ChamferSize;

            fixed4 _BorderColor;
            half _BorderWidth;
            half _BorderSqueeze;

            // =========================================================
            // 工具函数
            // =========================================================

            // 水平方向：从左或从右填充
            half GetHorizontalFill(half2 uv, half progress, half flipH)
            {
                half v = lerp(uv.x, 1.0 - uv.x, flipH);
                half fillStart = 0.0;
                half fillEnd = progress;
                return smoothstep(fillStart, fillStart + 0.002, v)
                    * (1.0 - smoothstep(fillEnd - 0.002, fillEnd, v));
            }

            // 垂直方向：从下或从上填充
            half GetVerticalFill(half2 uv, half progress, half flipV)
            {
                half v = lerp(uv.y, 1.0 - uv.y, flipV);
                half fillStart = 0.0;
                half fillEnd = progress;
                return smoothstep(fillStart, fillStart + 0.002, v)
                    * (1.0 - smoothstep(fillEnd - 0.002, fillEnd, v));
            }

            // 尖角裁剪（用于斜角血条风格）
            half GetChamferMask(half2 uv, half size)
            {
                half2 chamferUV = saturate(uv - size) / (1.0 - size);
                half cut = max(chamferUV.x, chamferUV.y);
                return step(cut, 1.0);
            }

            // 边缘过渡（让血条边缘柔和）
            half GetEdgeFade(half2 uv, half2 fade)
            {
                half2 fadeIn = smoothstep(0.0, fade, uv);
                half2 fadeOut = smoothstep(1.0, 1.0 - fade, uv);
                return fadeIn.x * fadeIn.y * fadeOut.x * fadeOut.y;
            }

            // =========================================================
            // 顶点着色器
            // =========================================================
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                #ifdef _FLIPHORIZONTAL_ON
                o.uv.x = 1.0 - o.uv.x;
                #endif
                #ifdef _FLIPVERTICAL_ON
                o.uv.y = 1.0 - o.uv.y;
                #endif

                return o;
            }

            // =========================================================
            // 片元着色器
            // =========================================================
            fixed4 frag(v2f i) : SV_Target
            {
                half2 uv = i.uv;
                half4 tex = tex2D(_MainTex, uv);

                half edgeFade = GetEdgeFade(uv, half2(_BufferOuterFade, _BufferOuterFade));
                half2 innerFade = half2(
                    smoothstep(0.0, _BufferInnerFade, uv.x) * smoothstep(1.0, 1.0 - _BufferInnerFade, uv.x),
                    smoothstep(0.0, _BufferInnerFade, uv.y) * smoothstep(1.0, 1.0 - _BufferInnerFade, uv.y)
                );
                half innerMask = innerFade.x * innerFade.y;

                // --- 前景层（实际血量） ---
                half fgFill = GetHorizontalFill(uv, _ForegroundProgress, _FlipHorizontal);
                half fgMask = fgFill * innerMask;

                #ifdef _USECHAMFER_ON
                fgMask *= GetChamferMask(uv, _ChamferSize);
                #endif

                fixed4 fg = fgMask * _ForegroundColor * tex.a;

                // --- 背景层（缓冲血量） ---
                fixed4 bg = fixed4(0, 0, 0, 0);

                #ifdef _USEBUFFER_ON
                if (_BufferProgress > _ForegroundProgress + _BufferOffset + 0.001)
                {
                    half bufferStart = _ForegroundProgress;
                    half bufferEnd = _BufferProgress;

                    half v = uv.x;
                    half bufferMask = smoothstep(bufferStart, bufferStart + 0.002, v)
                                    * (1.0 - smoothstep(bufferEnd - 0.002, bufferEnd, v));
                    bufferMask *= innerMask;

                    half innerFadeMask = smoothstep(0.0, _BufferInnerFade * 6.0, uv.x - bufferStart)
                                       * smoothstep(0.0, _BufferInnerFade * 6.0, bufferEnd - uv.x);

                #ifdef _USECHAMFER_ON
                    bufferMask *= GetChamferMask(uv, _ChamferSize);
                #endif

                    bg = bufferMask * _BufferColor * tex.a;
                    bg.a *= innerFadeMask * edgeFade;
                }
                #endif

                // --- 组合 ---
                fixed4 finalColor = bg;
                if (fg.a > bg.a)
                    finalColor = fg;

                #ifdef _USEBORDER_ON
                half2 borderUV = abs(uv - 0.5) * 2.0;
                half borderEdge = max(borderUV.x, borderUV.y);
                half borderMask = smoothstep(1.0 - _BorderWidth - _BorderSqueeze, 1.0 - _BorderWidth, borderEdge)
                                * (1.0 - smoothstep(1.0 - _BorderWidth, 1.0 - _BorderWidth + 0.002, borderEdge));
                half borderInnerMask = smoothstep(0.0, _BorderWidth, borderEdge);
                finalColor = lerp(finalColor, _BorderColor, borderMask * _BorderColor.a);
                #endif

                finalColor.a *= edgeFade;
                return finalColor;
            }
            ENDCG
        }
    }

    FallBack Off
}
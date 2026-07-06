Shader "Game/DamageChunkHealthBar"
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

        // --- 缓冲血量（当前缓冲进度，chunk 的整体终点） ---
        [Header(Buffer HP)]
        _BufferColor ("Buffer Color", Color) = (0.8, 0.8, 0.8, 1)
        _BufferProgress ("Buffer Progress [0,1]", Range(0, 1)) = 1
        _BufferOuterFade ("Buffer Outer Edge Fade", Range(0, 0.05)) = 0.005

        // --- DamageChunk 分段缓冲（最多 8 段） ---
        [Header(Damage Chunks (max 8))]
        _ChunkCount ("Active Chunk Count", Int) = 0
        // 每段 float3 = (startProgress, endProgress, opacity)，x,y ∈ [0,1], z ∈ [0,1]
        _Chunk0 ("Chunk 0 (start, end, opacity)", Vector) = (0,0,0,0)
        _Chunk1 ("Chunk 1 (start, end, opacity)", Vector) = (0,0,0,0)
        _Chunk2 ("Chunk 2 (start, end, opacity)", Vector) = (0,0,0,0)
        _Chunk3 ("Chunk 3 (start, end, opacity)", Vector) = (0,0,0,0)
        _Chunk4 ("Chunk 4 (start, end, opacity)", Vector) = (0,0,0,0)
        _Chunk5 ("Chunk 5 (start, end, opacity)", Vector) = (0,0,0,0)
        _Chunk6 ("Chunk 6 (start, end, opacity)", Vector) = (0,0,0,0)
        _Chunk7 ("Chunk 7 (start, end, opacity)", Vector) = (0,0,0,0)
        // 每段独立颜色
        _DamageChunkColor0 ("Chunk 0 Color", Color) = (0.78,0.78,0.78,1)
        _DamageChunkColor1 ("Chunk 1 Color", Color) = (0.78,0.78,0.78,1)
        _DamageChunkColor2 ("Chunk 2 Color", Color) = (0.78,0.78,0.78,1)
        _DamageChunkColor3 ("Chunk 3 Color", Color) = (0.78,0.78,0.78,1)
        _DamageChunkColor4 ("Chunk 4 Color", Color) = (0.78,0.78,0.78,1)
        _DamageChunkColor5 ("Chunk 5 Color", Color) = (0.78,0.78,0.78,1)
        _DamageChunkColor6 ("Chunk 6 Color", Color) = (0.78,0.78,0.78,1)
        _DamageChunkColor7 ("Chunk 7 Color", Color) = (0.78,0.78,0.78,1)

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
            #pragma multi_compile __ _USECHAMFER_ON
            #pragma multi_compile __ _USEBORDER_ON

            #include "UnityCG.cginc"

            // =========================================================
            // 输入输出结构
            // =========================================================
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // =========================================================
            // Properties
            // =========================================================
            sampler2D _MainTex;
            float4    _MainTex_ST;

            fixed4    _ForegroundColor;
            half      _ForegroundProgress;

            fixed4    _BufferColor;
            half      _BufferProgress;
            half      _BufferOuterFade;

            half      _FlipHorizontal;
            half      _FlipVertical;
            half      _FillOrigin;

            half      _UseChamfer;
            half      _ChamferSize;

            fixed4    _BorderColor;
            half      _BorderWidth;
            half      _BorderSqueeze;

            int       _ChunkCount;
            float3    _Chunk0, _Chunk1, _Chunk2, _Chunk3;
            float3    _Chunk4, _Chunk5, _Chunk6, _Chunk7;
            fixed4    _DamageChunkColor0, _DamageChunkColor1, _DamageChunkColor2, _DamageChunkColor3;
            fixed4    _DamageChunkColor4, _DamageChunkColor5, _DamageChunkColor6, _DamageChunkColor7;

            // =========================================================
            // 工具函数
            // =========================================================

            // 尖角遮罩
            half GetChamferMask(half2 uv, half size)
            {
                half2 cUV = saturate((uv - size) / (1.0 - size));
                return step(max(cUV.x, cUV.y), 1.0);
            }

            // 边缘柔和过渡
            half GetEdgeFade(half2 uv, half fade)
            {
                return smoothstep(0, fade, uv.x) * smoothstep(0, fade, uv.y)
                     * smoothstep(0, fade, 1 - uv.x) * smoothstep(0, fade, 1 - uv.y);
            }

            // 生成单段 chunk 遮罩（纯 alpha，0~1，边缘柔和）
            half GetChunkMask(half v, half chunkStart, half chunkEnd)
            {
                return smoothstep(chunkStart, chunkStart + 0.002, v)
                     * (1.0 - smoothstep(chunkEnd - 0.002, chunkEnd, v));
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

                half edgeFade = GetEdgeFade(uv, _BufferOuterFade);

                // 填充轴值（水平用 x，垂直用 y，受 FillOrigin + Flip 影响）
                half v;
                if (_FillOrigin <= 1)
                {
                    v = lerp(uv.x, 1.0 - uv.x, _FlipHorizontal);
                }
                else
                {
                    v = lerp(uv.y, 1.0 - uv.y, _FlipVertical);
                }

                // ---- 前景层 ----
                half fgMask = GetChunkMask(v, 0, _ForegroundProgress);
            #ifdef _USECHAMFER_ON
                fgMask *= GetChamferMask(uv, _ChamferSize);
            #endif
                fixed4 fg = fgMask * _ForegroundColor * tex.a;

                // ---- 8 个 DamageChunk 层（从旧到新叠加） ----
                fixed4 chunks = fixed4(0, 0, 0, 0);

                for (int ci = 0; ci < 8; ci++)
                {
                    if (ci >= _ChunkCount) break;

                    half3 chunkData;
                    switch (ci)
                    {
                        case 0: chunkData = _Chunk0; break;
                        case 1: chunkData = _Chunk1; break;
                        case 2: chunkData = _Chunk2; break;
                        case 3: chunkData = _Chunk3; break;
                        case 4: chunkData = _Chunk4; break;
                        case 5: chunkData = _Chunk5; break;
                        case 6: chunkData = _Chunk6; break;
                        default: chunkData = _Chunk7; break;
                    }

                    half chunkStart = chunkData.x;   // 受击时的前景值
                    half chunkEnd   = chunkData.y;   // 受击时的缓冲值
                    half chunkAlpha = chunkData.z;   // 当前透明度（由 C# 动画控制）

                    if (chunkAlpha < 0.001) continue;

                    half cMask = GetChunkMask(v, chunkStart, chunkEnd);

                #ifdef _USECHAMFER_ON
                    cMask *= GetChamferMask(uv, _ChamferSize);
                #endif

                    fixed4 cColor;
                    switch (ci)
                    {
                        case 0: cColor = _DamageChunkColor0; break;
                        case 1: cColor = _DamageChunkColor1; break;
                        case 2: cColor = _DamageChunkColor2; break;
                        case 3: cColor = _DamageChunkColor3; break;
                        case 4: cColor = _DamageChunkColor4; break;
                        case 5: cColor = _DamageChunkColor5; break;
                        case 6: cColor = _DamageChunkColor6; break;
                        default: cColor = _DamageChunkColor7; break;
                    }

                    // 混合：前景区域 foreground 优先，chunk 区域显示 chunk
                    half a = cMask * chunkAlpha * edgeFade;
                    chunks.rgb = lerp(chunks.rgb, cColor.rgb, a * cColor.a);
                    chunks.a   = max(chunks.a, a);
                }

                // ---- 合成（前景 > chunks > 透明） ----
                // fg.a > chunks.a → 前景区域用前景色；chunks.a > fg.a → chunk 区域用 chunk 色
                fixed4 finalColor = lerp(fg, chunks, step(fg.a, chunks.a));

            #ifdef _USEBORDER_ON
                half2 bUV = abs(uv - 0.5) * 2.0;
                half bEdge = max(bUV.x, bUV.y);
                half bMask = smoothstep(1.0 - _BorderWidth - _BorderSqueeze, 1.0 - _BorderWidth, bEdge)
                           * (1.0 - smoothstep(1.0 - _BorderWidth, 1.0 - _BorderWidth + 0.002, bEdge));
                finalColor = lerp(finalColor, _BorderColor, bMask * _BorderColor.a);
            #endif

                finalColor.a *= edgeFade;
                return finalColor;
            }
            ENDCG
        }
    }

    FallBack Off
}

Shader "Gameplay/DamageChunkHealthBar Instanced"
{
    Properties
    {
        _GameplayColor("Instance Color", Color) = (1,1,1,1)
        _GameplayFlip("Instance Flip", Vector) = (1,1,1,1)
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}

        // --- 前景层：当前实际血量 ---
        [Header(Foreground (Current HP))]
        _ForegroundColor ("Foreground Color", Color) = (1, 0, 0, 1)
        _ForegroundProgress ("Foreground Progress [0,1]", Range(0, 1)) = 1
        _FillOrigin ("Fill Origin", Int) = 0 // 0=Left, 1=Right, 2=Bottom, 3=Top
        _FlipHorizontal ("Flip Horizontal", Int) = 0
        _FlipVertical ("Flip Vertical", Int) = 0

        // --- 护盾层（位于前景血量之后，自动延伸） ---
        [Header(Shield Layer)]
        [Toggle] _UseShield ("Enable Shield", Int) = 0
        _ShieldColor ("Shield Color", Color) = (0, 0.6, 1, 1)
        _ShieldLength ("Shield Length [0,1]", Range(0, 1)) = 0
        _ShieldGlow ("Shield Glow Intensity", Range(0, 1)) = 0.3

        // --- 缓冲血量（当前缓冲进度，chunk 的整体终点） ---
        [Header(Buffer HP)]
        _BufferColor ("Buffer Color", Color) = (0.8, 0.8, 0.8, 1)
        _BufferProgress ("Buffer Progress [0,1]", Range(0, 1)) = 1
        _BufferOuterFade ("Buffer Outer Edge Fade", Range(0, 0.05)) = 0.005

        // --- DamageChunk 分段缓冲（最多 8 段） ---
        [Header(Damage Chunks (max 8))]
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
            Tags { "LightMode" = "Universal2D" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #pragma multi_compile_instancing
            #pragma instancing_options forcemaxcount:64
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // =========================================================
            // Properties
            // =========================================================
            sampler2D _MainTex;
            float4    _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(HealthBar)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ForegroundColor)
                UNITY_DEFINE_INSTANCED_PROP(float, _ForegroundProgress)
                UNITY_DEFINE_INSTANCED_PROP(float, _UseShield)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ShieldColor)
                UNITY_DEFINE_INSTANCED_PROP(float, _ShieldLength)
                UNITY_DEFINE_INSTANCED_PROP(float, _ShieldGlow)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BufferColor)
                UNITY_DEFINE_INSTANCED_PROP(float, _BufferProgress)
                UNITY_DEFINE_INSTANCED_PROP(float, _BufferOuterFade)
                UNITY_DEFINE_INSTANCED_PROP(float, _FlipHorizontal)
                UNITY_DEFINE_INSTANCED_PROP(float, _FlipVertical)
                UNITY_DEFINE_INSTANCED_PROP(float, _FillOrigin)
                UNITY_DEFINE_INSTANCED_PROP(float, _UseChamfer)
                UNITY_DEFINE_INSTANCED_PROP(float, _ChamferSize)
                UNITY_DEFINE_INSTANCED_PROP(float, _UseBorder)
                UNITY_DEFINE_INSTANCED_PROP(float4, _BorderColor)
                UNITY_DEFINE_INSTANCED_PROP(float, _BorderWidth)
                UNITY_DEFINE_INSTANCED_PROP(float, _BorderSqueeze)
                UNITY_DEFINE_INSTANCED_PROP(float4, _GameplayColor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _GameplayFlip)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Chunk0)
                UNITY_DEFINE_INSTANCED_PROP(float4, _DamageChunkColor0)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Chunk1)
                UNITY_DEFINE_INSTANCED_PROP(float4, _DamageChunkColor1)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Chunk2)
                UNITY_DEFINE_INSTANCED_PROP(float4, _DamageChunkColor2)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Chunk3)
                UNITY_DEFINE_INSTANCED_PROP(float4, _DamageChunkColor3)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Chunk4)
                UNITY_DEFINE_INSTANCED_PROP(float4, _DamageChunkColor4)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Chunk5)
                UNITY_DEFINE_INSTANCED_PROP(float4, _DamageChunkColor5)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Chunk6)
                UNITY_DEFINE_INSTANCED_PROP(float4, _DamageChunkColor6)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Chunk7)
                UNITY_DEFINE_INSTANCED_PROP(float4, _DamageChunkColor7)
            UNITY_INSTANCING_BUFFER_END(HealthBar)
            #define _ForegroundColor UNITY_ACCESS_INSTANCED_PROP(HealthBar, _ForegroundColor)
            #define _ForegroundProgress UNITY_ACCESS_INSTANCED_PROP(HealthBar, _ForegroundProgress)
            #define _UseShield UNITY_ACCESS_INSTANCED_PROP(HealthBar, _UseShield)
            #define _ShieldColor UNITY_ACCESS_INSTANCED_PROP(HealthBar, _ShieldColor)
            #define _ShieldLength UNITY_ACCESS_INSTANCED_PROP(HealthBar, _ShieldLength)
            #define _ShieldGlow UNITY_ACCESS_INSTANCED_PROP(HealthBar, _ShieldGlow)
            #define _BufferColor UNITY_ACCESS_INSTANCED_PROP(HealthBar, _BufferColor)
            #define _BufferProgress UNITY_ACCESS_INSTANCED_PROP(HealthBar, _BufferProgress)
            #define _BufferOuterFade UNITY_ACCESS_INSTANCED_PROP(HealthBar, _BufferOuterFade)
            #define _FlipHorizontal UNITY_ACCESS_INSTANCED_PROP(HealthBar, _FlipHorizontal)
            #define _FlipVertical UNITY_ACCESS_INSTANCED_PROP(HealthBar, _FlipVertical)
            #define _FillOrigin UNITY_ACCESS_INSTANCED_PROP(HealthBar, _FillOrigin)
            #define _UseChamfer UNITY_ACCESS_INSTANCED_PROP(HealthBar, _UseChamfer)
            #define _ChamferSize UNITY_ACCESS_INSTANCED_PROP(HealthBar, _ChamferSize)
            #define _UseBorder UNITY_ACCESS_INSTANCED_PROP(HealthBar, _UseBorder)
            #define _BorderColor UNITY_ACCESS_INSTANCED_PROP(HealthBar, _BorderColor)
            #define _BorderWidth UNITY_ACCESS_INSTANCED_PROP(HealthBar, _BorderWidth)
            #define _BorderSqueeze UNITY_ACCESS_INSTANCED_PROP(HealthBar, _BorderSqueeze)
            #define _GameplayColor UNITY_ACCESS_INSTANCED_PROP(HealthBar, _GameplayColor)
            #define _GameplayFlip UNITY_ACCESS_INSTANCED_PROP(HealthBar, _GameplayFlip)
            #define _Chunk0 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _Chunk0)
            #define _DamageChunkColor0 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _DamageChunkColor0)
            #define _Chunk1 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _Chunk1)
            #define _DamageChunkColor1 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _DamageChunkColor1)
            #define _Chunk2 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _Chunk2)
            #define _DamageChunkColor2 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _DamageChunkColor2)
            #define _Chunk3 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _Chunk3)
            #define _DamageChunkColor3 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _DamageChunkColor3)
            #define _Chunk4 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _Chunk4)
            #define _DamageChunkColor4 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _DamageChunkColor4)
            #define _Chunk5 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _Chunk5)
            #define _DamageChunkColor5 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _DamageChunkColor5)
            #define _Chunk6 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _Chunk6)
            #define _DamageChunkColor6 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _DamageChunkColor6)
            #define _Chunk7 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _Chunk7)
            #define _DamageChunkColor7 UNITY_ACCESS_INSTANCED_PROP(HealthBar, _DamageChunkColor7)

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
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                v.vertex.xy *= _GameplayFlip.xy;

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
                UNITY_SETUP_INSTANCE_ID(i);
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
            if (_UseChamfer > 0) {
                fgMask *= GetChamferMask(uv, _ChamferSize);
                }
                fixed4 fg = fgMask * _ForegroundColor * tex.a;

                // ---- 8 个 DamageChunk 层（从旧到新叠加） ----
                fixed4 chunks = fixed4(0, 0, 0, 0);

                for (int ci = 0; ci < 8; ci++)
                {
                    half3 chunkData;
                    switch (ci)
                    {
                        case 0: chunkData = _Chunk0.xyz; break;
                        case 1: chunkData = _Chunk1.xyz; break;
                        case 2: chunkData = _Chunk2.xyz; break;
                        case 3: chunkData = _Chunk3.xyz; break;
                        case 4: chunkData = _Chunk4.xyz; break;
                        case 5: chunkData = _Chunk5.xyz; break;
                        case 6: chunkData = _Chunk6.xyz; break;
                        default: chunkData = _Chunk7.xyz; break;
                    }

                    half chunkStart = chunkData.x;
                    half chunkEnd   = chunkData.y;
                    half chunkAlpha = chunkData.z;

                    if (chunkAlpha < 0.001) continue;

                    half cMask = GetChunkMask(v, chunkStart, chunkEnd);

                if (_UseChamfer > 0) {
                    cMask *= GetChamferMask(uv, _ChamferSize);
                    }

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

                    half a = cMask * chunkAlpha * edgeFade;
                    chunks.rgb = lerp(chunks.rgb, cColor.rgb, a * cColor.a);
                    chunks.a   = max(chunks.a, a);
                }

                // ---- 合成（前景 > chunks > 透明） ----
                fixed4 finalColor = lerp(fg, chunks, step(fg.a, chunks.a));

                // ---- 护盾层（自动接在前景之后） ----
                if (_UseShield > 0 && _ShieldLength > 0.001)
                {
                    // 护盾起点始终是"当前生命值的末尾"（即 _ForegroundProgress）
                    // 不考虑 chunks 的位置，因为 chunks 是动画残留，
                    // 玩家视觉上应该看到护盾永远在生命值之后
                    half shieldStart = _ForegroundProgress;
                    half shieldEnd = shieldStart + _ShieldLength;

                    // 护盾主体遮罩
                    half shieldMask = GetChunkMask(v, shieldStart, shieldEnd);

                if (_UseChamfer > 0) {
                    shieldMask *= GetChamferMask(uv, _ChamferSize);
                    }

                    // 护盾发光效果（在边缘处）
                    half glowFactor = smoothstep(shieldStart, shieldStart + 0.02, v)
                                    * (1.0 - smoothstep(shieldEnd - 0.02, shieldEnd, v));
                    fixed4 shieldGlow = _ShieldColor * _ShieldGlow * glowFactor * edgeFade;

                    // 护盾基础色
                    fixed4 shieldBase = shieldMask * _ShieldColor * tex.a;

                    // 合成护盾
                    fixed4 shieldLayer = lerp(shieldBase, shieldGlow, step(shieldBase.a, shieldGlow.a));

                    // 最终合成：前景+chunks > 护盾
                    finalColor = lerp(finalColor, shieldLayer, step(finalColor.a, shieldLayer.a));
                }

            if (_UseBorder > 0) {
                half2 bUV = abs(uv - 0.5) * 2.0;
                half bEdge = max(bUV.x, bUV.y);
                half bMask = smoothstep(1.0 - _BorderWidth - _BorderSqueeze, 1.0 - _BorderWidth, bEdge)
                           * (1.0 - smoothstep(1.0 - _BorderWidth, 1.0 - _BorderWidth + 0.002, bEdge));
                finalColor = lerp(finalColor, _BorderColor, bMask * _BorderColor.a);
                }

                finalColor.a *= edgeFade;
                return finalColor * _GameplayColor;
            }
            ENDCG
        }
    }

    FallBack Off
}

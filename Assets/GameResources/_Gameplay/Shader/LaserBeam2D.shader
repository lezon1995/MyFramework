// 2D 点到点激光特效 / LaserBeam2D
// ------------------------------------------------------------------
// 用法:
//   1) 新建 Material 选 Shader = "Game/LaserBeam2D"
//   2) 创建一个 Quad (1x1) 作为激光载体,材质赋给它
//   3) 挂 LaserBeamController,设置 StartPoint / EndPoint / StartRadius / EndRadius
//   4) Quad 在世界空间下覆盖整条激光 + 两端的圆形光斑
//
// 表现:
//   - 激光中间部分沿长度方向有强烈的流动条纹 + 高光带
//   - 起始端是一个圆,粒子从圆外飞向圆心(吸收能量)
//   - 结束端是一个圆,粒子从圆心飞向圆外(释放能量)
//   - Additive 混合,适合在 2D 场景中叠加
//
// 关键: 两端圆形用 *世界坐标* 计算,所以无论 Quad 如何旋转/缩放,
//       圆形始终是正圆。

Shader "Game/LaserBeam2D"
{
    Properties
    {
        [Header(Beam Colors)]
        _MainColor      ("Core Color",        Color) = (1, 1, 1, 1)
        _EdgeColor      ("Edge / Halo Color", Color) = (0.4, 0.8, 1.0, 1)

        [Header(Beam Shape)]
        _Thickness      ("Beam Thickness (0~1, cross-section softness)", Range(0, 1)) = 0.5
        _EdgeSoftness   ("Beam Edge Softness", Range(0.001, 0.5)) = 0.25

        [Header(Flow)]
        _FlowSpeed      ("Flow Speed",              Range(-10, 10)) = 4.5
        _FlowTiling     ("Flow Tiling (stripes)",   Range(1, 60))   = 24
        _FlowContrast   ("Flow Contrast",           Range(0.5, 5))  = 2.0
        _HighlightGain  ("Highlight Brightness",    Range(0, 4))    = 1.5

        [Header(Start Effect)]
        _StartColor     ("Start Color",      Color)          = (1.0, 0.95, 0.6, 1)
        _StartIntensity ("Start Intensity",  Range(0, 6))    = 2.5
        _StartRadius    ("Start Radius (world)", Range(0.05, 5)) = 0.6
        _StartSpeed     ("Start Particle Speed", Range(0.2, 6)) = 1.6
        _StartParticleCount ("Start Particle Count", Range(2, 16)) = 6

        [Header(End Effect)]
        _EndColor       ("End Color",        Color)          = (1.0, 0.4, 0.2, 1)
        _EndIntensity   ("End Intensity",    Range(0, 6))    = 2.5
        _EndRadius      ("End Radius (world)", Range(0.05, 5)) = 0.6
        _EndSpeed       ("End Particle Speed",  Range(0.2, 6)) = 1.6
        _EndParticleCount ("End Particle Count",  Range(2, 16)) = 6
        _HitPulseSpeed  ("Hit Pulse Speed",  Range(0, 10))   = 4.0

        [Header(Beam Trim)]
        // Beam body 的 UV.x 范围: [_StartFrac, _EndFrac]
        // 圆的外缘 (UV.x < _StartFrac 或 > _EndFrac) 不画 beam body
        // 由 C# 端根据 startRadius / endRadius / quadLength 计算
        _StartFrac      ("Start Beam UV Cutoff",  Range(0, 1)) = 0.1
        _EndFrac        ("End Beam UV Cutoff",    Range(0, 1)) = 0.9
        _BeamTrimSoftness ("Beam Trim Softness",  Range(0.0001, 0.05)) = 0.015

        [Header(Global)]
        _Intensity      ("Overall Intensity", Range(0, 4)) = 1.5
        _CorePower      ("Core Brightness",   Range(1, 8)) = 3.0
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
        // Additive: 让激光叠合在背景图上时更亮
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define TAU 6.2831853

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float2 worldPos : TEXCOORD1; // 世界 XY (2D)
            };

            // ===== Beam =====
            fixed4 _MainColor;
            fixed4 _EdgeColor;
            float  _Thickness;
            float  _EdgeSoftness;

            // ===== Flow =====
            float  _FlowSpeed;
            float  _FlowTiling;
            float  _FlowContrast;
            float  _HighlightGain;

            // ===== Start =====
            fixed4 _StartColor;
            float  _StartIntensity;
            float2 _StartPos;
            float  _StartRadius;
            float  _StartSpeed;
            float  _StartParticleCount;

            // ===== End =====
            fixed4 _EndColor;
            float  _EndIntensity;
            float2 _EndPos;
            float  _EndRadius;
            float  _EndSpeed;
            float  _EndParticleCount;
            float  _HitPulseSpeed;

            // ===== Beam Trim =====
            float _StartFrac;
            float _EndFrac;
            float _BeamTrimSoftness;

            // ===== Global =====
            float _Intensity;
            float _CorePower;

            // ----- Hash / 2D Value Noise -----
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // ----- 沿 UV.x 方向流动的条纹(多频叠加) -----
            float flowStripe(float u, float t)
            {
                float s = 0.0;
                s += valueNoise(float2(u * _FlowTiling        + t * 1.0, t)) * 0.6;
                s += valueNoise(float2(u * _FlowTiling * 2.3  + t * 1.6, t * 0.7)) * 0.3;
                s += valueNoise(float2(u * _FlowTiling * 4.7  + t * 2.1, t * 1.1)) * 0.1;
                return s;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.uv       = v.uv;
                // Quad 已经在世界坐标系下放好(由 controller 写入 transform),
                // 这里直接拿世界坐标。注意:z 不用,只取 xy。
                float3 wp  = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldPos = wp.xy;
                return o;
            }

            // ===== 圆形粒子光斑(世界坐标) =====
            // 思路:
            //   - 把当前像素转到 (radial, angle) 极坐标
            //   - 粒子环带:在 angle 上等距分布若干亮点,
            //                沿 radial 以速度向圆心/圆外运动
            //   - 输出每个像素处的粒子亮度
            //
            // dirToCenter: 从圆心指向像素的向量(已归一化到 0~1 半径)
            // t: 时间
            // direction: -1 = 向圆心(吸收), +1 = 向外(释放)
            // speed: 粒子飞行速度
            // count: 角向粒子数
            float circularParticles(float2 toCenter, float dist, float radius,
                                  float t, float direction, float speed, float count)
            {
                float2 d = toCenter / max(dist, 1e-4);  // 单位方向
                float theta = atan2(d.y, d.x);          // -π ~ π
                float radial = dist / radius;           // 0~1

                // 粒子归一化寿命 0~1
                float pulse = frac(t * speed * 0.5);
                // particleR 随 pulse 变化(无动态分支):
                //   direction=-1(吸收): 1 → 0   (外 → 内)
                //   direction=+1(释放): 0 → 1   (内 → 外)
                float dirFlag = saturate(-direction);          // 0 = 外飞, 1 = 内飞
                float particleR = lerp(pulse, 1.0 - pulse, dirFlag);

                // 角向亮点: count 个角位置,缓慢旋转
                float anglePhase = theta / TAU * count + t * 0.3;
                float angleFrac  = frac(anglePhase) - 0.5;       // [-0.5, 0.5]
                // 每个粒子在一个角位置上的高斯分布 (避开 pow(负数) 的兼容性问题)
                float aw         = angleFrac * count * 1.5;
                float angleBright = exp(-aw * aw);

                // 径向: 在 particleR 附近高亮(粒子"厚度")
                float radialDist  = abs(radial - particleR);
                float rw          = radialDist * 14.0;
                float radialBright = exp(-rw * rw);

                // 粒子内部轻微闪烁
                float flicker = 0.7 + 0.3 * sin(t * 8.0 + anglePhase * 6.28);

                return angleBright * radialBright * flicker;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv       = i.uv;
                float u         = uv.x;
                float v01       = uv.y;
                float2 wp       = i.worldPos;
                float t         = _Time.y;

                // ============= 1) 激光主体(沿 UV)=============
                // 粗细方向的厚度遮罩
                float distEdge = abs(v01 - 0.5) * 2.0;
                float thickness = saturate(_Thickness);
                float coreFalloff = 1.0 - smoothstep(
                    thickness * (1.0 - _EdgeSoftness),
                    thickness,
                    distEdge);
                float core = pow(saturate(1.0 - distEdge), _CorePower);

                // 流动条纹
                float scroll  = t * _FlowSpeed;
                float stripe0 = flowStripe(u, scroll);

                // 增强对比:把 stripe 从 [0,1] 推到 [0,1] 但更陡
                float stripe  = pow(saturate(stripe0), _FlowContrast);
                // 高光带:取最亮的 ~5% 再放大
                float highlight = smoothstep(0.85, 1.0, stripe0) * _HighlightGain;

                // 主体亮度 = 流动条纹调制 + 高光
                float beamLight = (0.45 + 0.55 * stripe + highlight) * coreFalloff;

                // — Beam body 修剪 mask: 只在 [StartFrac, EndFrac] 内画,
                //   圆的外缘(UV.x < StartFrac 或 > EndFrac)不画 beam,
                //   这样 beam body 只到圆心为止,不会越过圆心延伸到圆的边缘
                float trimLeft  = smoothstep(_StartFrac,
                                             _StartFrac + _BeamTrimSoftness,
                                             u);
                float trimRight = 1.0 - smoothstep(_EndFrac - _BeamTrimSoftness,
                                                   _EndFrac,
                                                   u);
                float beamMask  = trimLeft * trimRight;
                beamLight *= beamMask;

                fixed3 beamFinal = lerp(_EdgeColor.rgb, _MainColor.rgb, coreFalloff);
                beamFinal *= beamLight;

                // ============= 2) 起始端:吸收(粒子向圆心飞)=============
                float2 toStart   = wp - _StartPos;
                float  distStart = length(toStart);
                float  startMask = 1.0 - smoothstep(_StartRadius * 0.95,
                                                    _StartRadius,
                                                    distStart);
                // 边缘发光(光球边缘比中间亮一点,像吸能漩涡的边缘)
                float startRim   = smoothstep(_StartRadius * 0.6, _StartRadius * 0.95, distStart)
                                 * (1.0 - smoothstep(_StartRadius * 0.95, _StartRadius, distStart));
                // 中心核心(吸能漩涡中心,持续发光)
                float startCore  = exp(-pow(distStart / (_StartRadius * 0.4), 2.0));

                float startParticles = circularParticles(
                    toStart, distStart, _StartRadius,
                    t, -1.0, _StartSpeed, _StartParticleCount);

                // 合成:边缘环 + 中心核 + 飞入粒子
                float startGlow = startRim * 1.0
                                + startCore * 1.5
                                + startParticles * 1.2;

                fixed3 startFinal = _StartColor.rgb * _StartIntensity
                                  * startGlow * startMask;

                // ============= 3) 结束端:释放(粒子向圆外飞)=============
                float2 toEnd     = wp - _EndPos;
                float  distEnd   = length(toEnd);
                float  endMask   = 1.0 - smoothstep(_EndRadius * 0.95,
                                                    _EndRadius,
                                                    distEnd);
                // 中心闪烁
                float pulse      = 0.5 + 0.5 * sin(t * _HitPulseSpeed);
                float endCore    = exp(-pow(distEnd / (_EndRadius * 0.35), 2.0)) * (0.6 + 0.4 * pulse);

                // 扩散波纹(从中心向外)
                float ripplePhase = distEnd * 6.0 - t * 5.0;
                float ripple      = sin(ripplePhase) * 0.5 + 0.5;
                ripple = pow(saturate(ripple), 3.0);
                // 限定在 ring 范围内
                float ringMask    = smoothstep(0.0, _EndRadius * 0.15, distEnd)
                                  * (1.0 - smoothstep(_EndRadius * 0.85, _EndRadius, distEnd));
                float endRipple   = ripple * ringMask;

                // 粒子向外飞
                float endParticles = circularParticles(
                    toEnd, distEnd, _EndRadius,
                    t, +1.0, _EndSpeed, _EndParticleCount);

                float endGlow = endCore * 1.5
                              + endRipple * 1.2
                              + endParticles * 1.3;

                fixed3 endFinal = _EndColor.rgb * _EndIntensity
                                * endGlow * endMask;

                // ============= 4) 合成 =============
                // 激光主体的 alpha 由 UV.v 厚度遮罩决定(让 beam 边缘软衰减)
                // 端点圆形光斑已经乘了 startMask / endMask,球外自然为 0
                // 所以这里只对 beamFinal 做软边,startFinal/endFinal 直接相加
                float bodyAlpha = coreFalloff * coreFalloff;

                fixed3 finalCol = (beamFinal * bodyAlpha + startFinal + endFinal) * _Intensity;
                return fixed4(finalCol, 0.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
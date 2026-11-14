---
name: unity-shader
description: Unity Shader 编写与渲染管线优化指南，覆盖 ShaderLab/HLSL 语法、URP/HDRP 与内置管线差异、材质与 Keyword 变体管理、DrawCall 合批与透明排序、Shader 性能分析。Use when writing or editing .shader/.shadergraph/.hlsl files, choosing a render queue or blend mode, debugging pink/magenta shaders or missing keywords, reducing shader variants or draw calls, or analyzing rendering performance.
---

# Unity Shader 与渲染

## 何时使用

- 编写 / 修改 Shader、ShaderGraph、CG/HLSL 代码
- 排查材质变紫、Keyword 丢失、渲染顺序错误
- 降低 DrawCall、变体数量，控制移动端 GPU 开销
- 适配 URP / HDRP / Built-in 管线

## 1. 管线确认（第一步）

**写 Shader 前必须确认当前渲染管线**：

| 管线 | 关键点 |
|------|--------|
| Built-in | `UnityCG.cginc`，`LightMode` 用 `ForwardBase`/`ForwardAdd` |
| URP | `Core.hlsl`，`LightMode` 用 `UniversalForward`，需 `RenderPipeline` tag 或 URP pass |
| HDRP | `Core.hlsl`，`LightMode` 用 `HDRForward`/`Deferred` |

检查方式：`ProjectSettings > Graphics > Scriptable Render Pipeline Settings`；或看 URP Asset。

**本仓库**：项目同时存在多种第三方 Shader 与 `Assets/GameResources/_Gameplay` 下的自定义材质（`M_BrickUnitShadow.mat`、`Arc Centre 1.mat` 等），改动前先确认其管线归属。

## 2. ShaderLab 骨架（URP）

```hlsl
Shader "MyGame/Effects/Foo"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"       // Geometry=2000, AlphaTest=2450, Transparent=3000, Overlay=4000
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _Smoothness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = pos.positionCS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                #pragma multi_compile_fog
                OUT.fogFactor = ComputeFogFactor(pos.positionCS.z);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                color.rgb = MixFog(color.rgb, IN.fogFactor);
                return color;
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Lit"
}
```

## 3. 关键规则

### Pass 与 LightMode
- URP 前向：`"LightMode" = "UniversalForward"`；阴影投射 pass 用 `ShadowCaster`；深度 prepass 用 `DepthOnly`（URP 2022+ 为 `DepthNormals`/`Depth`）。
- **每个 Shader 至少提供 `ShadowCaster`**，否则动态阴影下会出现粉色/缺失。

### 队列与透明排序
| 需求 | Queue | Blend | ZWrite |
|------|-------|-------|--------|
| 不透明 | Geometry (2000) | One Zero | On |
| 裁剪 | AlphaTest (2450) | One Zero | On |
| 透明 | Transparent (3000) | SrcAlpha OneMinusSrcAlpha | Off |
| 叠加 UI | Overlay (4000) | SrcAlpha OneMinusSrcAlpha | Off |

- 透明物体不写深度 → 排序由渲染队列 + 距离决定，**同队列内顺序不可控**，美术上避免大面积半透明重叠。
- 透明 + `ZWrite On` 用于粒子/软边，可减少 Overdraw 排序问题但需谨慎。

### Keyword 与变体
- `#pragma shader_feature` → 材质勾选即可剔除变体（用于美术可控参数）。
- `#pragma multi_compile` → 保留所有变体（用于全局开关/光照烘焙）。
- **变体爆炸是首要性能问题**：为每个效果位开 keyword 会导致数十个变体。
- URP 管线常用内置 keyword：`_MAIN_LIGHT_SHADOWS`、`_ADDITIONAL_LIGHTS`、`_SHADOWS_SOFT`、`_FOG`、`_SPECULAR_HIGHLIGHTS` 等。
- 检查变体数：URP Asset → `Shader Stripping`；或 `Project > Analyze > Shader variants`（Shader Variant Analysis）。

### SRP Batcher 兼容
- 所有属性必须放在 **单个 `CBUFFER_START(UnityPerMaterial)`** 中，且不能包含 `UNITY_INSTANCING_BUFFER` 之外的 per-material 变量遗漏。
- 不要在 Shader 内写 `new`/动态数组，CBUFFER 内布局需 16 字节对齐。
- 变体之间 CBUFFER 布局不一致 → SRP Batcher 失效 → 每个物体单独 DrawCall。

### 顶点/片元优化
- 片元着色器里避免 `pow`、`normalize`、多次 `sin/cos`。
- 用 `half` 代替 `float`（移动端）。
- 纹理采样用 `SAMPLE_TEXTURE2D`（带 mipmap），避免 `tex2D` 无 mip 导致闪烁。
- `tex2Dlod` 用于顶点着色器采样。
- 阴影：加 `#pragma multi_compile_fog` 与 `_MAIN_LIGHT_SHADOWS` 等宏开关，否则阴影不会生效。

### 常见 Bug 对照
| 现象 | 原因 |
|------|------|
| 材质变粉/紫 | Shader 编译失败 → 看 Console / Shader Error 面板 |
| 阴影不生效 | 缺 `ShadowCaster` pass 或缺 shadow keyword |
| 物体被穿插/排序错 | 透明排序 + `ZWrite Off` |
| URP 下全黑 | `LightMode` 不匹配，用了 Built-in 的 `ForwardBase` |
| 实例化失效 | 缺 `UNITY_VERTEX_INPUT_INSTANCE_ID` / `UNITY_SETUP_INSTANCE_ID` |
| SRP Batcher 失效 | CBUFFER 布局不一致 / 用了 `MaterialPropertyBlock` 破坏批次（MPB 本身不破坏 SRP Batcher，但会破坏 GPU Instancing） |
| 雾效不生效 | 缺 `multi_compile_fog` 与 `ComputeFogFactor` |

## 4. 合批分析

1. Frame Debugger 查看 Pass 边界与 SetPassCall 数量。
2. Profiler → Rendering → Batches Count / SetPass Calls / Draw Calls Count。
3. 常见断批原因：
   - 材质不同（含不同 keyword 组合）
   - 材质实例不同（运行时 `material` 而非 `sharedMaterial`）
   - 渲染队列不同
   - 使用 `MaterialPropertyBlock`（破坏 GPU Instancing，保留 SRP Batcher）
   - 静态物体没打 Static 标记
4. 优化手段：材质共享、Shader Variant 精简、静态批处理、GPU Instancing、SRP Batcher。

## 5. 模板

- URP Unlit/Transparent Shader：[templates/urp-unlit.shader](templates/urp-unlit.shader)
- 通用效果 Shader（含 ShadowCaster / DepthOnly）：[templates/urp-fx-shader.shader](templates/urp-fx-shader.shader)
- Shader 走查清单：[templates/shader-review-checklist.md](templates/shader-review-checklist.md)

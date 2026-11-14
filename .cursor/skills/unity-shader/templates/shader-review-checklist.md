# Shader 走查清单

> 审查对象：`<shader 文件路径>`　管线：`URP / Built-in / HDRP`

## 1. 编译与正确性

- [ ] Shader 无编译错误（Console 无 `Shader error`）
- [ ] 材质在场景中显示正常（非粉色/紫色）
- [ ] 渲染管线 tag 与项目一致（`"RenderPipeline" = "UniversalPipeline"`）
- [ ] `LightMode` 与管线匹配（URP：`UniversalForward`）
- [ ] 提供了 `ShadowCaster` pass
- [ ] 提供了 `DepthOnly` / `DepthNormals` pass（若参与深度预通道或 SSAO）
- [ ] `Fallback` 合理（`Universal Render Pipeline/Lit` 或 `Off`）

## 2. SRP Batcher 兼容

- [ ] 所有材质属性都在单个 `CBUFFER_START(UnityPerMaterial)` 内
- [ ] CBUFFER 内变量 16 字节对齐（half 打包或用 float4）
- [ ] 变体之间 CBUFFER 布局完全一致
- [ ] 无 per-instance 需求时未滥用 `MaterialPropertyBlock`（会破坏 GPU Instancing）

## 3. Keyword 与变体

- [ ] `shader_feature` 用于材质可控参数
- [ ] `multi_compile` 只用于真正的全局/烘焙开关
- [ ] keyword 数量受控（记录变体数）
- [ ] 无 `""` 空 keyword 造成的额外变体
- [ ] URP 光照 keyword 正确引入（`_MAIN_LIGHT_SHADOWS` / `_ADDITIONAL_LIGHTS` 等）
- [ ] 阴影、雾相关 pragma 已声明且实际调用了对应宏

## 4. 渲染设置

- [ ] `Queue` 与 `RenderType` 匹配（不透明/裁剪/透明）
- [ ] 透明材质 `ZWrite Off`（除非刻意为之）
- [ ] `Cull` 设置符合预期
- [ ] `Blend` 模式符合效果需求（加法/正片/预乘）
- [ ] 透明物体排序冲突风险已评估

## 5. 性能

- [ ] 片元着色器无 `pow`/`normalize`/多次 `sin/cos` 等高开销操作
- [ ] 使用 `half` 代替 `float`（移动端）
- [ ] 纹理采样使用 `SAMPLE_TEXTURE2D`（含 mipmap）
- [ ] 顶点着色器采样用 `tex2Dlod`
- [ ] 无动态数组 / 动态循环 / 运行时分支分歧

## 6. 与其它系统交互

- [ ] 支持 GPU Instancing（若使用 `#pragma multi_compile_instancing`，需 `UNITY_SETUP_INSTANCE_ID`）
- [ ] 支持 SRP Batcher
- [ ] 支持 Fog（`multi_compile_fog` + `MixFog`）
- [ ] 支持相机深度纹理（若需要软粒子，需 `DeclareDepthTexture.hlsl`）

## 结论

- 通过项：`N / M`
- 需修改项：

| # | 位置 | 问题 | 严重级别 | 建议 |
|---|------|------|---------|------|
| 1 | | | 严重/一般/建议 | |

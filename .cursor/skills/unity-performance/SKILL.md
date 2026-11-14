---
name: unity-performance
description: Unity 性能优化指南，覆盖 GC 分配、CPU 帧耗时、GPU 渲染开销与内存占用。Use when profiling a Unity project, reducing garbage collection or allocations, optimizing Update loops, diagnosing frame drops or stutter, reducing draw calls and overdraw, managing textures/audio memory, or reviewing code for performance issues.
---

# Unity 性能优化

## 何时使用

- 帧率下降、卡顿、GC 尖峰排查
- 审查代码中的性能隐患
- 内存占用过高（包体、内存峰值）
- 降低 DrawCall / Overdraw / SetPassCall

## 0. 先测量，后优化

1. **Profiler**（`Window > Analysis > Profiler`）：确认是 CPU 还是 GPU 瓶颈。
   - CPU：`Main Thread` 时间分解 → `PlayerLoop` → `Scripts.Update` / `Physics` / `Animation`
   - GPU：`Rendering` 分类 → `SetPass Calls` / `Draw Calls Count` / `Batches Count`
2. **Deep Profile**：仅在 Development Build 勾选，耗时翻倍，只用于定位调用栈。
3. **Memory Profiler**：抓 `.snap`，看 Managed / Native / Texture / Mesh / Audio 分布。
4. **Frame Debugger**（`Window > Analysis > Frame Debugger`）：定位渲染 Pass 断裂与合批失败。
5. **Profiler 录制/分析**：`ProfilerRecorder`（运行时自测）比手动看更快。

**优化顺序**：先治 GC（内存尖峰 → 卡顿）→ 再治 CPU → 再治 DrawCall → 最后治 GPU。

## 1. GC / 内存分配

### 禁止项（高频路径）
- `new` 对象、`new List<T>()`、`new[]`、闭包捕获、字符串拼接、`$""` 插值
- `foreach` 遍历 `List<T>`（struct 元素会装箱）
- LINQ（`Where`/`Select`/`OrderBy`）在热路径
- `GetComponent` / `Find` / `Camera.main`
- `Debug.Log` 带字符串拼接
- `foreach` over `IEnumerable`（装箱枚举器）
- 自动属性 `public int Foo { get; set; }` 本身没问题，但**协程/迭代器**里赋值会装箱

### 手段
- 预分配缓存：`List`/`Dictionary` 初始化容量并 `Clear()` 复用。
- 对象池：`ObjectPool<T>`（`UnityEngine.Pool`）用于子弹、特效、UI。
- 缓存 `GetComponent` 结果到字段。
- `struct` 替代小 class；热路径避免 `Nullable` 装箱。
- 字符串：预生成 `static readonly string`；`ToString()` 用 `StringBuilder` 复用实例；避免 `string.Format`。
- 事件订阅用静态缓存委托，避免每次 `+=` 产生新委托对象（注意内存泄漏风险 → 用 `OnDisable` 退订）。
- 协程改 `UniTask`（无 GC）；`IEnumerator` 本身会分配。

### 检测手段
- Profiler → GC Alloc 列；开启 `Deep Profile` 看分配点。
- 代码扫描：搜索 `new `、`string.Format`、`$"`、`.Select(`、`.Where(`、`.ToList()`。
- Memory Profiler 快照对比（Managed Heap 是否持续增长 = 泄漏）。

## 2. CPU 优化

### Update 频率分级
| 需求 | 方案 |
|------|------|
| 每帧 | `Update` |
| 固定间隔 | `InvokeRepeating` / 协程 / `Time.time` 累计 |
| 物理步 | `FixedUpdate` |
| 渲染后 | `LateUpdate` |
| 摄像机跟随 | `LateUpdate` + `Time.deltaTime` |
| 大量对象 | 自建更新管理器集中调度，或 `IUpdateManager` 模式 |

### 常见热点
- `Update` 数量过多（>1000 个 MonoBehaviour Update）
- 物理：过多 Collider、频繁 `Rigidbody` 状态改变、`Physics.autoSyncTransforms`
- 动画：Animator 状态机开销、SkinnedMesh 蒙皮
- UI：重建 Canvas、`LayoutGroup` 每帧刷新 → 用 `Canvas` 拆分 + 缓存
- 字符串/日志热路径
- 虚方法/接口调用装箱
- `Camera.main`（内部 tag 查找，Unity 6 用 `Camera.main` 已优化但仍建议缓存）

### 手段
- **静态批处理**：Static 标记、`Static Batching Utility`。
- **GPU Instancing**：材质勾选 `Enable GPU Instancing`，材质变体用 `MaterialPropertyBlock`。
- 对象池减少实例化/销毁。
- 降物理频率：`Time.fixedDeltaTime` 调大；`Physics.reuseCollisionCallbacks`。
- 关闭不需要的模块：`Animator`、`ParticleSystem` 自动更新关闭、`AudioSource`。
- `Application.targetFrameRate` / `QualitySettings.vSyncCount`（移动端关 vSync）。
- 协程/`Task` 切换点：避免在主线程做 IO。

## 3. GPU 优化

- **Overdraw**：半透明 UI/特效重叠；关闭不需要的粒子；用 `Camera` 的 `Allow HDR`/`Allow MSAA` 谨慎。
- **DrawCall 合并**：静态物体合批；同材质合并；`static` + `Batching Static`。
- **Material 数量**：共享材质实例；避免运行时 `material`（会实例化）→ 用 `MaterialPropertyBlock` 或 `sharedMaterial`。
- **Shader 复杂度**：变体数量控制（`ShaderVariantCollection`），避免 keyword 爆炸；移动端用 `#pragma target 3.5` 以下。
- **分辨率**：URP 的 Render Scale（`UniversalRenderPipelineAsset.renderScale`）降内部分辨率。
- **光照**：烘焙 + 光照贴图；限制实时阴影（`Shadow Distance`、`Cascades`）。
- **LOD / Occlusion**：LODGroup、`Occlusion Culling`（烘焙 visibility data）。
- **纹理**：压缩格式（ASTC/ETC2）、Mipmap、`Max Size`；避免 Read/Write。
- **后处理**：URP Volume 数量、`Camera Stack` 层数、`Bloom` 成本高。

## 4. 内存管理

- 纹理：关闭 Read/Write，改用压缩；图集合并。
- 音频：加载方式（`Load in Background`）、`Preload Audio Data` 慎开、长音频用 Streaming。
- AssetBundle / Addressables：按需加载，及时 `Release`；`UnloadUnusedAssets` 慎用（会卡顿）。
- 对象池：复用但要设上限（`maxSize`）。
- 大数组：考虑 `NativeArray`/`ArrayPool<T>`。
- 场景：对象过多用 `SceneManager` 卸载场景 + `DontDestroyOnLoad` 只保留必要单例。

## 5. 优化报告模板

```
## 性能分析结论
- 瓶颈类型：CPU / GPU / 内存
- 关键证据：Profiler 数据（帧时间、GC Alloc、DrawCall）

## 发现的问题
### [严重级别] 位置
- 问题：
- 影响：
- 优化方案：
- 预期收益：

## 实施步骤
1. ...
```
严重级别：`严重` / `一般` / `建议`。

## 6. 参考模板

- 性能分析报告模板：[templates/perf-report.md](templates/perf-report.md)
- 对象池实现：[templates/object-pool.cs](templates/object-pool.cs)
- 帧率/GC 自测探针：[templates/perf-probe.cs](templates/perf-probe.cs)

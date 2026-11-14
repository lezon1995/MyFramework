---
name: unity-debugging
description: Unity Bug 定位与排查流程，覆盖日志分析、断点调试、常见异常类型（空引用、MissingReference、shader 错误、物理穿模、GC 卡顿）与最小复现。Use when investigating a Unity bug or crash, reading Editor.log or player logs, diagnosing NullReferenceException / MissingReferenceException / pink materials / physics tunneling, bisecting recent changes, or writing a debugging checklist and bug report.
---

# Unity 调试与问题排查

## 何时使用

- 定位具体 Bug、崩溃、异常堆栈
- 分析日志（Editor.log / Player.log / CI 日志）
- 性能卡顿、内存泄漏、渲染异常排查
- 需要产出 Bug 报告或复现步骤

## 0. 排查流程（通用）

1. **稳定复现**：先拿到最小复现步骤（哪一步、什么输入、什么环境）。不能复现 → 优先收集日志。
2. **收集证据**：
   - 完整堆栈（含所有 `at` 行），不要只截第一行
   - Editor.log：`%LOCALAPPDATA%/Unity/Editor/Editor.log`
   - Player.log：Windows `%USERPROFILE%/AppData/LocalLow/<Company>/<Product>/Player.log`，Android `/sdcard/Android/data/<pkg>/files`
   - Console 上下文（前后各若干行，含 Warning）
3. **分类症状** → 跳转对应章节（见下）。
4. **缩小范围**：二分法注释代码 / `git stash` / 逐个禁用模块。
5. **验证修复**：加回归测试或可复现验证步骤。

**关键原则**：先看堆栈最底层的**业务帧**（不是 Unity 内部帧），那通常是真凶位置。

## 1. 日志分析

### 读取堆栈
```
NullReferenceException: Object reference not set to an instance of an object
  at MyGame.Ball.Player.TakeDamage (System.Int32 amount) in E:\...\Assets\Scripts\HotFix\_Gameplay\Ball\Player.cs:42
  at MyGame.Ball.Player.FixedUpdate () in ...
```
- 第 1 行：错误类型 + 消息
- 第 2 行起：调用链，**从上到下是调用顺序**，最上面是抛出点
- 注意 `at ... in ...:行号` 中的行号是**旧行号**，代码改动后可能偏移

### 常见日志关键词
| 关键词 | 含义 |
|--------|------|
| `NullReferenceException` | 空引用 |
| `MissingReferenceException` | 对象被 Destroy 但仍被访问（常见于协程/异步回调） |
| `ArgumentException: An item with the same key` | Dictionary 重复 Add |
| `InvalidOperationException: Collection was modified` | 遍历中修改集合 |
| `IndexOutOfRangeException` | 越界 |
| `Shader error in 'X'` | Shader 编译失败 |
| `Trying to destroy already destroyed object` | 重复 Destroy |
| `Coroutine couldn't be started because the game object 'X' is inactive` | 协程在 inactive 对象上启动 |
| `SendMessage cannot be called during physics` | 物理回调中调用 SendMessage |
| `The referenced script on this Behaviour is missing` | 脚本丢失引用 |

### 日志规范（本项目）
- 统一使用带模块前缀的日志：`Debug.Log("[ModuleName] message")`；建议封装统一 Logger。
- 生产包不要用 `Debug.Log`（字符串拼接有 GC 开销），用条件编译：`[Conditional("UNITY_EDITOR")] [Conditional("DEVELOPMENT_BUILD")]`。

## 2. 异常类型速查

### NullReferenceException
排查顺序：
1. `UnityEngine.Object` 的"假 null"——被 `Destroy` 后 `== null` 为 true，但 `ReferenceEquals` 为 false。**对象被销毁后仍持有引用**是最常见原因。
2. 序列化字段未赋值（Inspector 上字段为空）→ 加 `OnValidate` 检查。
3. 事件回调中对象已销毁。
4. 字典 `TryGetValue` 返回值未判空。

防御：
```csharp
if (target == null) { return; }              // Unity 对象
if (!dict.TryGetValue(id, out var v)) { return; }
```

### MissingReferenceException
典型场景：协程 / `Invoke` / 事件回调在对象销毁后仍执行。
```csharp
private IEnumerator Routine()
{
    yield return new WaitForSeconds(1f);
    if (this == null) { yield break; }   // 销毁后中断
    DoSomething();
}
```
更优：用 UniTask + `CancellationToken`，并在 `OnDestroy` 取消。

### 协程相关
- 对象 inactive 时 `StartCoroutine` 失败 → 先 `SetActive(true)` 或由外部启动。
- `WaitForSeconds` 受 `Time.timeScale` 影响；`WaitForSecondsRealtime` 不受影响。
- 协程内访问已销毁的 Transform → 同 MissingReference。

## 3. 常见功能性问题

### 物理穿模 / 掉落
- Rigidbody `Collision Detection` = `Continuous Dynamic`（快速移动物体）
- `Interpolate` = `Interpolate`
- 固定步长太小导致穿透：`Time.fixedDeltaTime` 默认 0.02，增大可减少穿透但降低精度
- `Rigidbody.velocity` 建议用 `MovePosition`/`AddForce`，不要直接改 `transform.position`
- Layer Collision Matrix 配错 → 检查 `ProjectSettings > Physics`
- 静态 Collider 与动态 Rigidbody 组合：静态物体不应挂 Rigidbody

### 渲染异常
- 粉色/紫色材质 → Shader 编译错误，看 Console
- 物体不可见 → Layer 被 Camera culling mask 排除；或被裁剪面裁掉
- Z-fighting → 调整 offset / 拉开距离 / 改渲染队列

### 帧率骤降
- 见 `unity-performance` skill；先看 Profiler 的 `GC Alloc` 与 `Physics` 段。

## 4. 调试技巧

| 手段 | 用途 |
|------|------|
| `Debug.Log` + 上下文 | 最小侵入，但热路径慎用 |
| `Debug.Break()` / `Debug.Assert` | 触发断点 |
| Unity Inspector 实时 `Debug` 面板 | 修 bug 时临时开启（记得提交前关闭） |
| Conditional 属性 | 正式代码里保留调试日志 |
| `[ContextMenu("Debug/Dump State")]` | 右键菜单手动触发诊断输出 |
| `UnityEngine.Assertions` | 开发期断言 |
| `Application.logMessageReceived` | 捕获并汇总日志 |
| 断点 + 条件断点 | VS/Rider 里对 `if (id == 123)` 打断点 |
| `git bisect` | 二分定位引入 Bug 的提交 |

### 只在开发期存在的诊断代码
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
[System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
public static void LogState(object state)
{
    Debug.Log($"[State] {state}");
}
```

## 5. Bug 报告模板

使用：[templates/bug-report.md](templates/bug-report.md)
诊断辅助脚本：[templates/debug-utils.cs](templates/debug-utils.cs)

## 6. 输出约定

排查结论按以下结构输出：

```
## 结论
- 根因：<一句话>
- 触发条件：<关键条件>

## 证据
- 堆栈关键帧：`file.cs:行`
- 日志片段：
- 复现步骤：1) ... 2) ...

## 修复方案
- 修改文件：`path/File.cs`
- 具体改动：
- 风险与副作用：

## 验证
- [ ] 按复现步骤确认不再复现
- [ ] 相关场景回归通过
```

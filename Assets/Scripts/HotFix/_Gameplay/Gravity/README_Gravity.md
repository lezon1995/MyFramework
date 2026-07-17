# 万有引力模拟 — 使用说明

## 文件说明

| 文件 | 说明 |
|------|------|
| `GravitySource.cs` | 挂在行星上，定义引力范围和坠落时间区间 |
| `GravityBody.cs` | 挂在物体 A 上，状态机驱动，进入引力范围后必定坠向行星中心 |
| `GravitySimulationSetup.cs` | 编辑器辅助组件，一键生成演示场景 |
| `README_Gravity.md` | 本文档 |

---

## 快速开始

1. 在场景中新建空对象，命名为 `Planet`
2. 添加 `GravitySimulationSetup` 组件
3. 配置参数（见下文）
4. **Play**，右键组件 → **Re-launch Object A** 可重新发射

---

## 行为逻辑

```
┌─────────┐  进入引力范围   ┌──────────┐  progress≥1  ┌─────────┐
│  Flying │ ─────────────▶ │  Falling │ ────────────▶ │ Crashed │
└─────────┘                └──────────┘              └─────────┘
  匀速直线飞行              螺旋轨迹坠落
                       必定命中，不飞出去
```

### Flying 阶段
- 匀速直线运动，不受引力影响
- 每帧检测是否进入某 `GravitySource` 的引力范围

### Falling 阶段

进入引力范围瞬间：

1. **计算入射角 α**：`velocity` 反方向与进入点法线的夹角（0° ~ 90°）
2. **查表得坠落时间**：`duration = Lerp(minDuration, maxDuration, α/90°)`
3. **锁定轨迹参数**（entry 位置、法线、切向向量）
4. 之后沿**解析螺旋路径**运动，不可能飞出引力范围

---

## 轨迹公式

```
t ∈ [0, 1]  — 坠落进度（0=进入瞬间，1=抵达行星中心）
α ∈ [0°, 90°] — 入射角

offset = sin(α) × R_entry × 4t(1-t)       ← 螺旋幅度
pos(t) = Lerp(entryPos, planetPos, t)
       + offset × tangent                  ← 叠加切向位移
```

| 入射角 α | 效果 | sin(α) | 坠落时间 |
|---------|------|--------|---------|
| 0°（垂直射入） | 直线坠落，无圆弧 | 0 | minDuration |
| 45°（斜向） | 轻微螺旋 | ~0.707 | 中间值 |
| 90°（切向进入） | 完整圆弧绕行 | 1 | maxDuration |

---

## 核心参数

### GravitySource（行星）

| 参数 | 说明 | 建议值 |
|------|------|--------|
| `gravityRange` | 引力生效半径，物体进入此范围后开始坠落 | 10~30 |
| `minDuration` | 入射角 0° 时的最小坠落时间（秒） | 0.5~3 |
| `maxDuration` | 入射角 90° 时的最大坠落时间（秒） | 3~15 |

### GravityBody（物体 A）

| 参数 | 说明 | 建议值 |
|------|------|--------|
| `initialVelocity` | 初始速度矢量 | — |
| `crashRadius` | 坠毁判定半径（设成行星视觉半径） | 0.5~2 |

### GravitySimulationSetup（演示）

| 参数 | 说明 | 建议值 |
|------|------|--------|
| `gravityRange` | 与 `GravitySource.gravityRange` 同步 | — |
| `minDuration` / `maxDuration` | 与 `GravitySource` 同步 | — |
| `initialSpeed` | 物体 A 速度大小 | 3~20 |
| `launchAngle` | 发射方向（XZ 平面，度数），与引力范围边界相切时物体沿切线飞入引力范围 | -180~180 |
| `planetVisualRadius` | 行星视觉大小 | 0.5~3 |

> **launchAngle = 0°** 时物体沿 X 正方向飞入，此时速度方向与引力方向（Z 轴负方向）夹角为 90°，对应最大坠落时间 maxDuration。
> **launchAngle = -90°** 时物体直接射向行星中心，对应最小坠落时间 minDuration。

---

## 扩展建议

### 1. 多引力源

```csharp
void HandleFlying()
{
    transform.position += _velocity * Time.fixedDeltaTime;

    var sources = FindObjectsOfType<GravitySource>();
    GravitySource nearest = null;
    float minDist = float.MaxValue;
    foreach (var s in sources)
    {
        if (s.IsWithinRange(transform.position))
        {
            float d = (transform.position - s.Position).sqrMagnitude;
            if (d < minDist) { minDist = d; nearest = s; }
        }
    }
    if (nearest != null) Capture(nearest);
}
```

### 2. 坠毁特效

```csharp
protected override void OnCrash(GravitySource source)
{
    // 播放爆炸特效预制件、屏幕震动等
    base.OnCrash(source); // 会触发 destroyOnCrash
}
```

### 3. 进入引力范围时触发事件

```csharp
public System.Action<GravityBody, GravitySource> OnCaptured;

void Capture(GravitySource source)
{
    // ... 原有逻辑 ...
    OnCaptured?.Invoke(this, source);
}
```

### 4. 拖尾轨迹

在 `GravityBody` 中新增：

```csharp
Queue<Vector3> _trailHistory = new();
const int MaxTrail = 300;

void HandleFalling()
{
    _trailHistory.Enqueue(transform.position);
    if (_trailHistory.Count > MaxTrail)
        _trailHistory.Dequeue();
}
```

配合一个 Trail Renderer 组件即可。

### 5. 运行时动态调整

```csharp
// 改变坠落时间区间后，下次 Capture 时生效
public void SetDurationRange(float min, float max)
{
    var s = GetComponent<GravitySource>();
    s.minDuration = min;
    s.maxDuration = max;
}
```

---

## 常见问题

**Q: 物体穿过行星飞走了？**
- `crashRadius` 应设成行星视觉半径。物体抵达行星位置即判定坠毁，不会穿过去。

**Q: 坠落时间不符合预期？**
- `launchAngle` 决定入射角，参考上文的参数对照表调整。
- `minDuration` 和 `maxDuration` 决定坠落时间区间。

**Q: 物体没有进入引力范围？**
- 检查物体初始位置是否在 `gravityRange` 之外（否则不会触发 Flying 检测）
- 检查 `initialSpeed` 是否为 0（物体不动就不会进入范围）

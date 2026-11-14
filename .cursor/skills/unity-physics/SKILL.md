---
name: unity-physics
description: Unity 物理系统指南，覆盖 Rigidbody、Collider、WheelCollider 车辆控制、2D/3D 物理材质与层矩阵、射线检测与触发器、CharacterController、固定时间步与物理模拟调优。Use when configuring rigidbodies or colliders, implementing character movement or vehicle physics, tuning bounce/friction, setting up trigger detection, layer collision matrices, raycasts, or debugging physics tunneling and jitter.
---

# Unity 物理系统

## 何时使用

- 配置 Rigidbody / Collider / PhysicMaterial
- 实现角色移动、车辆（WheelCollider）、投掷物
- 处理触发器、射线检测、层与碰撞矩阵
- 排查穿模、抖动、弹跳异常、检测不到等问题

## 1. 核心概念

| 组件 | 作用 |
|------|------|
| `Rigidbody` | 赋予物体物理属性（质量、阻力、约束），受重力与力影响 |
| `Collider` | 碰撞形状，定义物理接触面 |
| `PhysicMaterial`（PhysicMaterial / PhysicsMaterial） | 摩擦与弹性组合（Unity 6 中 `PhysicMaterial`） |
| `Joint` | 约束（铰链、弹簧、固定） |
| `WheelCollider` | 内置车辆轮胎模型 |
| `CharacterController` | 胶囊角色移动（自行处理重力与台阶） |

**关键**：静态物体（无 Rigidbody）不参与动力学模拟，只作为"无限质量"的碰撞体。

## 2. Rigidbody 配置

| 属性 | 建议 |
|------|------|
| `Mass` | 按真实量级（kg）；过小会导致抖动 |
| `Drag` / `Angular Drag` | 3D 默认 0 / 0.05；2D 是 `linearDamping`/`angularDamping` |
| `Use Gravity` | 车辆/飞行器关闭 |
| `Is Kinematic` | 手动驱动的物体用 kinematic（`MovePosition`/`MoveRotation`），避免与物理争抢 |
| `Constraints` | 锁定不需要的轴（如只允许平面移动 → Freeze X/Z） |
| `Interpolation` | `Interpolate` 消除视觉抖动（`Extrapolate` 用于预测） |
| `Collision Detection` | 快速物体用 `Continuous Dynamic`（代价高），慢速用 `Discrete` |
| `Max Depenetration Velocity` | 穿透恢复速度，太大导致弹飞 |

### 力的应用方式
| 需求 | API |
|------|-----|
| 持续推力 | `AddForce(force, ForceMode)` |
| 瞬时冲量 | `AddForce(force, ForceMode.Impulse)` |
| 直接改速度 | `velocity`（会绕过物理约束，仅用于简单情况） |
| 刚体移动 | `MovePosition` / `MoveRotation`（kinematic 用） |

`ForceMode`：`Force`（受质量影响）、`Acceleration`（忽略质量）、`Impulse`（瞬时）、`VelocityChange`（忽略质量瞬时）。

**反模式**：直接写 `transform.position` 移动带 Rigidbody 的物体 → 与物理引擎冲突、穿模。应改 `Rigidbody.MovePosition` 或用 kinematic。

## 3. Collider 配置

| 类型 | 用途 |
|------|------|
| `BoxCollider` | 默认，最省性能 |
| `SphereCollider` | 球，检测开销低 |
| `CapsuleCollider` | 角色 |
| `MeshCollider` | 精确网格，**非凸网格不能与其它 MeshCollider 动态碰撞**（Unity 只支持凸网格动态碰撞） |
| `TerrainCollider` | 地形 |

### Trigger
- 勾选 `isTrigger` → 只检测不产生碰撞响应，通过 `OnTriggerEnter/Stay/Exit` 回调。
- **触发器至少一方必须有 Rigidbody**（两边都没刚体不会触发回调）。
- 触发器回调没有 `collision.relativeVelocity` 等碰撞信息。

### PhysicMaterial（摩擦/弹性）
- `Dynamic Friction` / `Static Friction` / `Bounciness` / `Friction Combine` / `Bounce Combine`。
- Combine 模式：`Average`（默认）、`Minimum`、`Maximum`、`Multiply`。
- 常见需求：角色不打滑 → Dynamic Friction 高；球弹跳 → Bounciness 0.6~0.8。

## 4. 层与碰撞矩阵

- `ProjectSettings > Physics > Layer Collision Matrix` 决定哪些层互相碰撞。
- Layer 命名约定建议（可按项目补充）：
  - `Default`、`Player`、`Enemy`、`Bullet`、`Ground`、`Wall`、`Trigger`、`Pickup`、`Vehicle`
- 优化：把不需要互相碰撞的层设为不碰撞（如 `Bullet` × `Bullet`），减少物理开销。
- **注意**：Layer Matrix 是全局的，改动会影响所有场景；查询用 `Physics.GetIgnoreLayerCollision` / `Physics.IgnoreLayerCollision`（运行时动态设置）。

## 5. 检测 API

### 射线
```csharp
// 3D
if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
{
    Debug.Log(hit.collider.gameObject.name);
}

// 射线组（多结果）
RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxDistance, layerMask);
Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

// 球形扫描
Physics.SphereCast(origin, radius, direction, out hit, maxDistance, layerMask);

// 无碰撞体遮挡检测（Linecast）
Physics.Linecast(a, b, layerMask);
```

### 2D
`Physics2D.Raycast` / `CircleCast` / `OverlapCircle` / `BoxCast` / `OverlapBoxAll`。

### QueryTriggerInteraction
- `Ignore`：忽略触发器（默认）
- `Collide`：包含触发器
- `UseGlobal`：用全局设置（`Physics.queriesHitTriggers`）

### 非分配版本（性能关键）
```csharp
private readonly RaycastHit[] _hits = new RaycastHit[8];

int count = Physics.RaycastNonAlloc(origin, direction, _hits, maxDistance, layerMask);
```
避免 `RaycastAll` 的数组分配（GC）。

## 6. 2D 与 3D 不能混用
`Physics.*`（3D）与 `Physics2D.*`（2D）是两套独立系统，Collider 与 Rigidbody2D 不能与 3D 版本配合。

## 7. WheelCollider 车辆

```csharp
[SerializeField] private WheelCollider frontLeft;
[SerializeField] private WheelCollider frontRight;
[SerializeField] private WheelCollider rearLeft;
[SerializeField] private WheelCollider rearRight;

[SerializeField] private float maxSteerAngle = 30f;
[SerializeField] private float maxMotorTorque = 1500f;
[SerializeField] private float brakeTorque = 3000f;

private void FixedUpdate()
{
    float steer = Input.GetAxis("Horizontal") * maxSteerAngle;
    float torque = Input.GetAxis("Vertical") * maxMotorTorque;

    frontLeft.steerAngle = steer;
    frontRight.steerAngle = steer;

    rearLeft.motorTorque = torque;
    rearRight.motorTorque = torque;

    if (Input.GetKey(KeyCode.Space))
    {
        rearLeft.brakeTorque = brakeTorque;
        rearRight.brakeTorque = brakeTorque;
    }
    else
    {
        rearLeft.brakeTorque = 0;
        rearRight.brakeTorque = 0;
    }
}
```

### 关键点
- `WheelCollider` 是**不可见的**，需配一个可见的 Mesh/GameObject 做视觉同步（放在 `WheelVisual` 子对象，代码中同步 `position`/`rotation`）。
- `motorTorque` 正负决定前进/后退；`brakeTorque` 负责刹车（不要用负 motorTorque 当刹车）。
- `steerAngle` 需在 `FixedUpdate` 中设置。
- 参数起步：`mass` 1500、`motorTorque` 1500~3000、`brakeTorque` 3000~6000、`steerAngle` 25~35；`suspensionDistance` 0.3~0.5。
- 防翻车：降低重心、限制 `maxSuspensionForce`、`maxSuspensionTravel`。
- 空中控制：`WheelCollider` 不做空气动力学，需自行实现空气转向/俯仰。

## 8. CharacterController

```csharp
private CharacterController _controller;
private float _gravityVelocity;

private void Update()
{
    float horizontal = Input.GetAxisRaw("Horizontal");
    float vertical = Input.GetAxisRaw("Vertical");
    Vector3 direction = (transform.right * horizontal + transform.forward * vertical).normalized;

    if (_controller.isGrounded && _gravityVelocity < 0f)
    {
        _gravityVelocity = -2f;   // 贴地
    }

    _gravityVelocity += Physics.gravity.y * Time.deltaTime;

    Vector3 move = direction * moveSpeed * Time.deltaTime;
    move.y = _gravityVelocity * Time.deltaTime;

    _controller.Move(move);
}
```

要点：
- `Move` 实际位移可能小于参数值（撞墙时）→ 用 `Move` 返回值计算真实速度。
- `isGrounded` 需留余量（slopeLimit / stepOffset 配合）。
- `CharacterController` **不是 Rigidbody**，不受物理力影响，自身也不产生碰撞事件。

## 9. 固定时间步与模拟设置

`ProjectSettings > Time`：
- `Fixed Timestep`：默认 0.02s（50Hz）。物理计算频率。
- `Maximum Allowed Timestep`：默认 0.333s，防卡顿后补算过多物理步。
- `Time Scale`：0 → 物理暂停；影响 `WaitForSeconds`、粒子、部分动画。

`ProjectSettings > Physics`：
- `Solver Iterations`（默认 6）/ `Solver Velocity Iterations`（3）：影响堆叠稳定性。
- `Default Contact Offset`（0.01）、`Bounce Threshold`（2）：高速小物体接触判定。
- `Auto Sync Transforms`（默认关闭）：手动改 transform 后需 `Physics.SyncTransforms()`，否则碰撞体未同步。

## 10. 常见问题排查

| 现象 | 原因与解决 |
|------|-----------|
| 物体穿墙 | 速度过快 → `Continuous Dynamic`；`fixedDeltaTime` 太小导致步进跳跃；缩放变化未同步 `Physics.SyncTransforms` |
| 物体抖动/爆炸 | 质量差异过大；`Solver Iterations` 太低；多个 Collider 重叠；Rigidbody 与 transform 同时被改 |
| 抓不住地面 | `Bounce Threshold` 过高导致碰撞被忽略；摩擦为 0；Rigidbody 质量过大 |
| 触发器不触发 | 双方无 Rigidbody；用了 3D/2D 混用；层被禁碰撞 |
| 射线打不中 | LayerMask 不对；忘了 `QueryTriggerInteraction`；Collider 是 Trigger 而用了 `Ignore` |
| 车辆打滑/翻车 | 重心过高、扭矩过大、`steerAngle` 过激、`brakeTorque` 分配不当 |
| 物理帧率影响手感 | 物理在 FixedUpdate（50Hz），渲染在 Update（可变）→ 手感不同步时在 `Update` 里做视觉插值 |
| 刚体被"吸"到某处 | 手写 `transform` 与 Rigidbody 同时驱动 |

## 11. 模板

- Rigidbody 刚体组件：[templates/rigidbody-setup.cs](templates/rigidbody-setup.cs)
- 车辆控制器：[templates/vehicle-controller.cs](templates/vehicle-controller.cs)
- 角色控制器：[templates/character-controller.cs](templates/character-controller.cs)
- 物理排查清单：[templates/physics-checklist.md](templates/physics-checklist.md)

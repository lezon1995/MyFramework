# 2D体积感系统 - 吸血鬼幸存者风格

这是一个纯手工实现的2D体积碰撞系统，灵感来源于吸血鬼幸存者。不使用Unity内置物理系统，纯靠速度、质量和碰撞体大小来计算体积感。

## 核心功能

### 1. 体积碰撞（Volume Collision）
- 怪物和玩家之间不完全重叠
- 可通过参数控制最大重叠程度
- 基于圆形碰撞体计算

### 2. 挤压感（Squeeze）
- 怪物可以推着玩家动
- 玩家也可以推着怪物动
- 取决于质量、速度和推力权重

### 3. 链式击退（Chain Knockback）
- 子弹可以击退怪物
- 被击退的怪物会连带击退身后方向的其他怪物
- 力度会逐级递减

## 文件结构

```
Assets/Scripts/HotFix/_TopDownEngine/Characters/Volume/
├── EntityBody2D.cs           # 实体体积数据组件
├── VolumeManager.cs          # 全局体积碰撞管理器
├── VolumeCollisionResult.cs  # 碰撞结果数据结构
├── KnockbackSystem.cs        # 击退系统接口和工具
├── VolumeUtils.cs            # 辅助函数和预设
├── VolumeTestComponent.cs    # 测试组件（简单）
└── VolumeDemo.cs             # 演示组件（完整）

Assets/Scripts/HotFix/_TopDownEngine/Bricks/
├── BrickVolumeBody.cs        # Brick的体积感组件
└── Brick.VolumeExtensions.cs # Brick体积感扩展

Assets/Scripts/HotFix/_TopDownEngine/Balls/
└── Ball.VolumeKnockback.cs   # 球与体积系统集成
```

## 快速开始

### 方法一：使用演示场景

1. 创建一个空的GameObject，命名为 `VolumeTest`
2. 添加 `VolumeDemo` 组件
3. 运行游戏即可看到效果

### 方法二：手动集成

#### 步骤1：添加组件

在需要体积感的实体上添加 `EntityBody2D` 组件：

```csharp
// 运行时添加
var body = gameObject.AddComponent<EntityBody2D>();
body.Radius = 0.5f;      // 碰撞半径
body.Mass = 1f;          // 质量
body.MaxOverlapRatio = 0.3f; // 最大重叠比率
```

#### 步骤2：注册到管理器

```csharp
// 获取或创建VolumeManager
if (VolumeManager.Instance == null)
{
    var go = new GameObject("VolumeManager");
    VolumeManager.Instance = go.AddComponent<VolumeManager>();
}

// 注册实体
VolumeManager.Instance.Register(body);
```

#### 步骤3：施打击退

```csharp
// 对实体施打击退
Vector2 direction = Vector2.left;
float force = 10f;
VolumeManager.Instance.ApplyKnockback(body, direction, force);
```

## 参数说明

### EntityBody2D 参数

| 参数 | 说明 | 范围 |
|------|------|------|
| Radius | 碰撞半径 | 任意正数 |
| Mass | 质量，影响碰撞时谁推谁动 | 0.1-10 |
| MaxOverlapRatio | 最大重叠比率，0表示完全不能重叠 | 0-1 |
| PushForceWeight | 推力权重，决定谁推谁动 | 0-10 |
| KnockbackResistance | 击退抗性，0表示完全受击退 | 0-1 |
| KnockbackSpreadRatio | 击退被分担的比率 | 0-1 |
| SeparationForce | 分离力，越大越快分开 | 0-50 |
| VelocityDamping | 速度阻力 | 0-1 |

### VolumeManager 参数

| 参数 | 说明 | 默认值 |
|------|------|--------|
| Enabled | 是否启用系统 | true |
| MaxCollisionChecksPerFrame | 每帧最大检测次数 | 1000 |
| BaseSeparationForce | 基础分离力 | 10f |
| MassDifferenceInfluence | 质量差影响系数 | 0.5 |
| EnableChainKnockback | 启用链式击退 | true |
| MaxChainLevel | 链式击退最大层级 | 5 |
| ChainDecayRatio | 每级衰减比率 | 0.6 |
| ChainKnockbackRadiusMultiplier | 链式检测半径乘数 | 1.5 |

## 使用示例

### 示例1：玩家与怪物交互

```csharp
public class MonsterVolumeSetup : MonoBehaviour
{
    void Start()
    {
        var body = gameObject.AddComponent<EntityBody2D>();
        body.Radius = 0.4f;
        body.Mass = 1f;
        body.MaxOverlapRatio = 0.2f;

        VolumeManager.Instance.Register(body);
    }

    void Update()
    {
        // AI移动逻辑...
        var body = GetComponent<EntityBody2D>();
        body.Velocity = aiDirection * moveSpeed;
    }
}
```

### 示例2：球击中怪物触发链式击退

```csharp
// 在球的击中事件中
public void OnHitMonster(Brick brick, Vector2 hitNormal)
{
    var volumeBody = brick.GetComponent<BrickVolumeBody>();
    if (volumeBody != null)
    {
        volumeBody.OnHitByBall(hitNormal, knockbackForce);
    }
}
```

### 示例3：玩家施放技能产生范围击退

```csharp
public void CastKnockbackSkill(Vector2 center, float radius, float force)
{
    var info = new KnockbackInfo(
        direction: Vector2.one, // 径向击退，方向会被覆盖
        force: force,
        position: center,
        enableChain: true,
        chainDecay: 0.6f
    );

    KnockbackApplier.ApplyAreaKnockback(center, radius, info);
}
```

### 示例4：使用预设

```csharp
// 应用预设
var playerPreset = VolumePreset.Player;
playerPreset.ApplyTo(playerBody);

var lightPreset = VolumePreset.LightWeight;
lightPreset.ApplyTo(smallEnemyBody);

var heavyPreset = VolumePreset.Heavy;
heavyPreset.ApplyTo(bossBody);
```

## 架构设计

### 核心类

1. **VolumeManager** - 单例管理器，处理所有碰撞检测和击退逻辑
2. **EntityBody2D** - 实体体积数据组件，挂在每个需要碰撞的物体上
3. **VolumeCollisionResult** - 碰撞结果，包含方向、距离、重叠量等信息

### 碰撞检测流程

```
每帧更新:
1. VolumeManager.ProcessAllCollisions()
   └── 遍历所有实体对
       └── ProcessPairCollision()
           ├── 检查是否碰撞
           ├── 如果超出最大重叠 -> CalculateSeparation()
           └── 如果有相对速度 -> CalculateSqueeze()
```

### 链式击退流程

```
ApplyKnockback() 调用:
1. 对目标施打击退力
2. ProcessChainKnockback()
   └── BFS遍历
       ├── 检测击退方向后方的实体
       ├── 计算衰减后的击退力
       └── 添加到链式击退列表
3. ApplyAllKnockbackForces()
   └── 应用所有链式击退
```

## 性能优化

1. **碰撞检测限制**：`MaxCollisionChecksPerFrame` 防止每帧检测过多
2. **更新间隔**：`UpdateInterval` 可以降低更新频率
3. **空间分区**：大量实体时建议实现网格或四叉树分区
4. **对象池**：频繁创建销毁实体时使用对象池

## 调试功能

```csharp
// 在VolumeManager上启用调试显示
VolumeManager.Instance.ShowAllGizmos = true;
VolumeManager.Instance.ShowCollisionLines = true;

// 在EntityBody2D上启用
body.ShowGizmos = true;
```

## 扩展开发

### 添加新的击退源

```csharp
public class CustomKnockbackSource : MonoBehaviour, IKnockbackSource
{
    public float force = 10f;
    public Vector2 direction = Vector2.right;

    public float GetKnockbackForce() => force;
    public Vector2 GetKnockbackDirection() => direction;
    public Vector2 GetKnockbackPosition() => transform.position;
    public bool IsChainKnockbackEnabled() => true;

    public void TriggerKnockback(EntityBody2D target)
    {
        KnockbackApplier.Apply(target, this);
    }
}
```

## 常见问题

### Q: 实体穿过彼此怎么办？
A: 检查 `MaxOverlapRatio` 是否设置过大，或增加 `SeparationForce`。

### Q: 击退效果不明显？
A: 增加 `KnockbackResistance`（降低抗性），或增加击退力。

### Q: 性能下降严重？
A: 减少实体数量，降低 `UpdateInterval`，或实现空间分区。

### Q: 链式击退范围太大？
A: 减小 `ChainKnockbackRadiusMultiplier` 或 `MaxChainLevel`。

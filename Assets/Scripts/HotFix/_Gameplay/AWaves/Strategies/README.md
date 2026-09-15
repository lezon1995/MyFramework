# 刷怪策略系统架构

本目录包含基于 `WaveManager` 重构而来的刷怪策略系统。

## 目录结构

```
Strategies/
├── MonsterSpawnStrategy.cs              # 抽象基类
├── VampireLikeMonsterSpawnStrategy.cs   # 幸存者玩法
├── BallXPitLikeMonsterSpawnStrategy.cs  # Ball x Pit 玩法（含子策略配置）
├── MonsterSpawnStrategyFactory.cs       # 工厂类 + 策略管理器
├── SubSpawnStrategy.cs                  # 子策略抽象基类
└── SubStrategies/
    ├── TopRowStrategy.cs                # 顶部行生成
    ├── RandomRowStrategy.cs             # 随机行生成
    ├── RandomColStrategy.cs             # 随机列生成
    └── RandomEmptyStrategy.cs           # 随机空位生成
```

## 核心类说明

### 1. `MonsterSpawnStrategy`（抽象基类）

定义刷怪系统的核心行为接口：
- `Initialize(...)`：初始化策略
- `Update(dt, activeMonsterCount)`：每帧更新
- `NotifyMonsterKilled()`：通知怪物击杀
- `Reset()`：重置策略状态

### 2. `VampireLikeMonsterSpawnStrategy`

类幸存者玩法的刷怪策略（原 `WaveManager` 的逻辑）：
- 怪物从场地边缘生成
- 持续刷怪模式
- 智能刷怪（基于密度）
- 形状生成支持
- 击杀速度追踪与动态间隔

### 3. `BallXPitLikeMonsterSpawnStrategy`

类 Ball x Pit 玩法的刷怪策略：
- 砖块在网格内生成
- 通过子策略决定生成方式
- 支持 1x1, 1x2, 2x1, 2x2 等多种砖块尺寸
- 所有砖块必须完全在网格范围内

### 4. 子策略（`SubSpawnStrategy`）

#### 4.1 `TopRowStrategy`
- 在网格的最上方行生成砖块
- 可配置顶部行偏移
- 连续失败时向下扩展搜索范围

#### 4.2 `RandomRowStrategy`
- 在随机选择的行生成砖块
- 可配置批大小
- 该行无空位时自动寻找附近行

#### 4.3 `RandomColStrategy`
- 在随机选择的列生成砖块
- 可配置批大小
- 该列无空位时自动寻找附近列

#### 4.4 `RandomEmptyStrategy`
- 在随机的空格子处生成砖块
- 可配置是否优先在边缘生成
- 智能处理多格砖块的位置检查

## 使用示例

### 基本使用

```csharp
// 在 WaveManager 中创建并使用策略
var strategyManager = new MonsterSpawnStrategyManager();
strategyManager.Initialize(waveManager, waveConfig, levelConfig);
strategyManager.SetStrategy(MonsterSpawnStrategyType.VampireLike);

// 在 Update 中调用
strategyManager.Update(Time.deltaTime, activeMonsterCount);

// 怪物被击杀时
strategyManager.NotifyMonsterKilled();
```

### 使用 Ball x Pit 玩法

```csharp
strategyManager.SetStrategy(MonsterSpawnStrategyType.BallXPitLike);

// 切换子策略
strategyManager.SetSubStrategy(SubSpawnStrategyType.TopRow);
// 或
strategyManager.SetSubStrategy(SubSpawnStrategyType.RandomRow);
// 或
strategyManager.SetSubStrategy(SubSpawnStrategyType.RandomCol);
// 或
strategyManager.SetSubStrategy(SubSpawnStrategyType.RandomEmpty);
```

### 通过工厂创建策略

```csharp
var strategy = MonsterSpawnStrategyFactory.CreateAndInitialize(
    MonsterSpawnStrategyType.BallXPitLike,
    waveManager,
    waveConfig,
    levelConfig
);
```

### 自定义 Ball x Pit 子策略配置

```csharp
strategyManager.UpdateBallXPitLikeConfig(ballXPitStrategy =>
{
    ballXPitStrategy.spawnInterval = 1.5f;
    ballXPitStrategy.spawnCountPerBatch = 5;
    ballXPitStrategy.maxActiveBricks = 30;
    ballXPitStrategy.allowMultiCellBricks = true;
    
    // 配置砖块尺寸
    ballXPitStrategy.brickSizeConfigs.Clear();
    ballXPitStrategy.brickSizeConfigs.Add(new BrickSizeConfig 
    { 
        size = new Vector2Int(1, 1), 
        weight = 10f 
    });
    ballXPitStrategy.brickSizeConfigs.Add(new BrickSizeConfig 
    { 
        size = new Vector2Int(2, 1), 
        weight = 5f 
    });
    
    // 配置子策略
    ballXPitStrategy.subStrategies.Add(new SubSpawnStrategyConfig
    {
        strategyType = SubSpawnStrategyType.TopRow,
        topRowStartOffset = 0
    });
});
```

## 重要约束

1. **网格范围**：所有砖块生成时必须确保完全在网格范围内（0 ≤ x < cols, 0 ≤ y < rows）
2. **空位检查**：多格砖块必须确保其覆盖的所有格子都为空
3. **尺寸支持**：通过 `BrickSizeConfig` 配置可用尺寸及其权重
4. **默认行为**：如果未配置子策略，`BallXPitLike` 会默认使用 `TopRowStrategy`

## 与原 WaveManager 的对应关系

原 `WaveManager` 中的刷怪逻辑：
- `ProcessMonsterSpawn()` → `VampireLikeMonsterSpawnStrategy.Update()`
- `ProcessContinuousSpawn()` → `VampireLikeMonsterSpawnStrategy.ProcessContinuousSpawn()`
- `ProcessLimitedSpawn()` → `VampireLikeMonsterSpawnStrategy.ProcessLimitedSpawn()`
- `SpawnRandomMonster()` → `VampireLikeMonsterSpawnStrategy.SpawnRandomMonster()`
- `SpawnRandomShape()` → `VampireLikeMonsterSpawnStrategy.SpawnRandomShape()`
- `GetSmartSpawnPosition()` → 保留在 `WaveManager` 中供策略调用
- `GetEdgeBiasedRandomEmptyCell()` → 保留在 `WaveManager` 中供策略调用

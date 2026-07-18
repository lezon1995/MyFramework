# 波次刷怪系统 (Wave Spawning System)

这是一个类土豆兄弟（Brotato）风格的2D关卡波次刷怪系统，支持智能刷怪、动态平衡和多种通关策略。

## 功能特性

### 1. 波次配置系统
- 支持配置多个波次（Wave）
- 每个波次可独立设置：
  - 持续时间
  - 通关策略（坚持时间/击败全部/击败Boss）
  - 可生成的怪物类型
  - 怪物数量限制
  - 属性增长倍率

### 2. 智能刷怪系统
- 基于怪物密度分布的智能生成
- 怪物密集区域少刷/不刷
- 怪物稀疏区域多刷
- 保持场上怪物数量动态平衡
- **支持边界偏向生成**（可配置在边界生成的概率）

### 3. 动态怪物类型平衡
- 小怪、精英怪、Boss怪的权重配置
- 根据当前场上分布动态调整生成比例
- 避免某一类型过度集中

### 4. 属性增长系统
- 怪物血量、伤害、速度、防御随波次增长
- 支持全局和单波次的增长倍率配置

### 5. 通关策略
- **SurviveUntilEnd**: 坚持到倒计时结束
- **DefeatAllMonsters**: 击败所有怪物（**新增最大生成数量限制**）
- **DefeatBoss**: 击败Boss通关（最后一关）

### 6. 波次间奖励系统
- 通关后进入奖励选择阶段
- **玩家主动确认后才进入下一波**
- 提供属性提升、治疗、金币等多种奖励

## 文件结构

```
Assets/Scripts/HotFix/_Gameplay/AWaves/
├── WaveConfig.cs           # 波次配置数据结构
├── WaveManager.cs          # 波次管理器（核心）
├── WaveRewardManager.cs    # 奖励管理器
├── WaveGameMode.cs         # 游戏模式整合器
├── WaveEvents.cs           # 事件定义
├── WaveSystemBootstrap.cs  # 系统启动器
├── WaveUI.cs               # UI组件
└── WaveConfigExamples.cs   # 配置示例

Assets/Scripts/HotFix/Editor/AWaves/
└── WaveConfigEditor.cs     # 编辑器工具
```

## 修复的问题

1. **奖励阶段阻塞**：进入奖励选择阶段后，必须玩家主动调用`ConfirmRewardSelection()`或`SkipRewardAndContinue()`才会进入下一波
2. **DefeatAll怪物上限**：击败所有怪物策略时，会限制最大总生成数量（默认为`maxActiveMonsters * 3`）
3. **WaveKillCount更新**：在`UpdateMonsterStates()`和`CleanupDeadMonsters()`中都会更新击杀计数
4. **边界偏向生成**：新增`edgeBias`配置，可控制怪物在边界生成的概率

## 快速开始

### 1. 创建关卡配置

在Unity中：
1. 右键 -> Create -> MoreMountains -> WaveLevelConfig
2. 配置关卡名称、描述
3. 添加波次并配置参数
4. 保存配置

### 2. 启动波次游戏

在场景中添加 `WaveSystemBootstrap` 组件：
```csharp
// 或通过代码启动
var config = Resources.Load<WaveLevelConfig>("YourLevelConfig");
WaveGameMode.Instance.StartGame(config);
```

### 3. 监听事件

```csharp
// 监听波次开始
WaveManager.Instance.OnWaveStart += (config) => {
    Debug.Log($"Wave {config.waveNumber} started!");
};

// 监听波次完成
WaveManager.Instance.OnWaveComplete += (config) => {
    Debug.Log($"Wave {config.waveNumber} completed!");
};

// 监听游戏结束
WaveManager.Instance.OnGameEnd += (result) => {
    if (result == GameResult.Victory)
        Debug.Log("You Win!");
    else
        Debug.Log("Game Over!");
};
```

## 配置说明

### WaveLevelConfig (关卡配置)

| 参数 | 说明 | 默认值 |
|------|------|--------|
| levelName | 关卡名称 | - |
| globalMaxActiveMonsters | 最大同时存活怪物数 | 20 |
| globalMinActiveMonsters | 最小存活怪物数 | 5 |
| globalBaseSpawnInterval | 基础刷怪间隔(秒) | 2f |
| globalHealthScalingPerWave | 血量增长倍率 | 1.15f |
| globalDamageScalingPerWave | 伤害增长倍率 | 1.12f |
| spawnAreaLeft/Right/Top/Bottom | 生成区域边界 | - |

### WaveConfig (波次配置)

| 参数 | 说明 | 默认值 |
|------|------|--------|
| waveNumber | 波次编号 | - |
| waveName | 波次名称 | - |
| duration | 持续时间(秒)，0=无限 | 60f |
| clearStrategy | 通关策略 | SurviveUntilEnd |
| maxActiveMonsters | 最大存活数 | 10 |
| minActiveMonsters | 最小存活数 | 3 |
| defeatAllMaxTotalSpawn | 击败所有策略的最大生成数 | 0 (使用默认maxActiveMonsters*3) |
| normalMonsterWeight | 小怪生成权重 | 70f |
| eliteMonsterWeight | 精英怪权重 | 25f |
| bossMonsterWeight | Boss权重 | 5f |
| edgeBias | 边界生成偏向(0-1) | 0.8f |
| enableSmartSpawning | 启用智能刷怪 | true |
| denseRadius | 密集判定半径 | 5f |
| sparseThreshold | 稀疏判定阈值 | 1 |

## API 参考

### WaveManager

```csharp
// 启动关卡
void StartLevel(WaveLevelConfig config)

// 开始下一波
void StartNextWave()

// 生成一个怪物
AMonster SpawnMonster(string monsterId, SpawnEnemyType type, Vector3? position = null)

// 进入奖励选择阶段
void EnterRewardSelection()

// 离开奖励选择阶段，开始下一波
void ExitRewardSelection()

// 获取智能生成位置
Vector3 GetSmartSpawnPosition()

// 获取动态怪物类型
SpawnEnemyType GetWeightedEnemyType()

// 选择指定类型的怪物
string SelectMonsterByType(SpawnEnemyType type)

// 获取属性增长数据
MonsterScalingData GetScalingData()

// 获取当前动态刷怪间隔
float GetDynamicSpawnInterval()

// 是否处于奖励选择阶段
bool IsInRewardSelection { get; }
```

### WaveGameMode

```csharp
// 开始游戏
void StartGame(WaveLevelConfig levelConfig)

// 进入奖励选择阶段
void EnterRewardPhase()

// 确认奖励选择并继续
void ConfirmRewardSelection()

// 跳过奖励并继续
void SkipRewardAndContinue()

// 暂停游戏
void PauseGame()

// 恢复游戏
void ResumeGame()
```

## 事件列表

| 事件 | 说明 |
|------|------|
| OnWaveStart | 波次开始 |
| OnWaveComplete | 波次完成 |
| OnWaveFailed | 波次失败 |
| OnLevelStart | 关卡开始 |
| OnLevelComplete | 关卡完成 |
| OnGameEnd | 游戏结束 |
| OnMonsterSpawned | 怪物生成 |
| OnMonsterKilled | 怪物死亡 |
| OnBossSpawned | Boss生成 |
| OnBossDefeated | Boss被击败 |
| OnStateChanged | 状态改变 |
| OnWaveTimeUpdate | 时间更新 |
| OnRewardSelectionStarted | 奖励选择开始 |
| OnRewardSelectionEnded | 奖励选择结束 |

## 奖励系统使用流程

```
波次完成 -> WaveManager.EnterRewardSelection() -> 状态变为 RewardSelecting
                                                    |
                                                    v
                         WaveGameMode.EnterRewardPhase() 生成奖励选项
                                                    |
                                                    v
                         UI显示奖励 -> 玩家选择奖励 -> 玩家点击"继续"
                                                    |
                                                    v
                         WaveGameMode.ConfirmRewardSelection()
                                                    |
                                                    v
                         WaveManager.ExitRewardSelection() -> 开始下一波
```

## 示例配置

系统包含 `WaveConfigExamples` 组件，可直接使用示例配置进行测试：

1. 在场景中添加 `WaveConfigExamples` 组件
2. 勾选 `useExampleConfig`
3. 运行游戏

示例配置包含5个波次：
- Wave 1: 新手波次（纯小怪）
- Wave 2: 增加精英怪
- Wave 3: 击败所有怪物（有限制最大生成数量）
- Wave 4: 生存挑战
- Wave 5: 最终Boss战

## 扩展建议

### 1. 自定义怪物生成
修改 `WaveManager.CreateMonster` 方法以适配你的怪物创建系统

### 2. 自定义属性应用
修改 `WaveManager.ApplyScalingToMonster` 方法以适配你的属性系统

### 3. 添加新奖励类型
在 `WaveRewardManager` 中扩展 `ApplyReward` 方法

### 4. 自定义UI
基于 `WaveUI` 组件扩展你自己的UI实现

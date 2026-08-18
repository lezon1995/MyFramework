using System;
using System.Collections.Generic;
using System.Linq;
using UniStats;
using UnityEngine;
using Random = System.Random;

namespace MoreMountains
{
    /// <summary>
    /// 波次状态
    /// </summary>
    public enum WaveState
    {
        Idle, // 空闲/未开始
        Preparing, // 准备阶段
        Active, // 波次进行中
        Clearing, // 清理阶段（等待剩余怪物死亡）
        RewardSelecting, // 奖励选择阶段
        Completed, // 波次完成
        Failed, // 波次失败
        AllCleared // 全部波次通关
    }

    /// <summary>
    /// 游戏结果
    /// </summary>
    public enum GameResult
    {
        None, // 未定
        Victory, // 胜利
        Defeat, // 失败
    }

    /// <summary>
    /// 波次刷怪管理器 - 核心系统
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        #region Properties

        public WaveLevelConfig CurLevel { get; set; } // 当前关卡配置
        public WaveConfig CurWave { get; set; } // 当前波次配置
        public WaveConfig NextWave { get; set; } // 下一波次配置
        public int WaveNumber { get; set; } // 当前波次编号（从1开始）
        public WaveState State { get; set; } // 当前波次状态
        public float WaveTimeRemaining { get; set; } // 当前波次剩余时间
        public float WaveTimeElapsed { get; set; } // 当前波次已用时间
        public List<Brick> ActiveMonsters { get; } = new();
        public List<Brick> ActiveBosses { get; } = new();
        public int WaveKillCount { get; set; } // 波次内的怪物击杀数
        public int WaveSpawnCount { get; set; } // 波次内的总生成怪物数
        public GameResult FinalResult { get; set; } // 游戏最终结果

        // 是否正在进行游戏
        public bool IsPlaying
        {
            get
            {
                switch (State)
                {
                    case WaveState.Idle:
                    case WaveState.AllCleared:
                    case WaveState.Failed:
                        return false;
                    case WaveState.Preparing:
                    case WaveState.Active:
                    case WaveState.Clearing:
                    case WaveState.RewardSelecting:
                    case WaveState.Completed:
                        return true;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// 当前活跃怪物数量
        /// </summary>
        public int ActiveMonsterCount
        {
            get
            {
                int count = 0;
                foreach (var m in ActiveMonsters)
                    if (m.IsAlive())
                        count++;
                return count;
            }
        }

        /// <summary>
        /// 当前存活Boss数量
        /// </summary>
        public int ActiveBossCount
        {
            get
            {
                int count = 0;
                foreach (var m in ActiveBosses)
                    if (m.IsAlive())
                        count++;
                return count;
            }
        }

        public bool HasBossSpawned { get; set; } // 是否Boss已生成
        public bool IsInRewardSelection => State == WaveState.RewardSelecting; // 是否处于奖励选择阶段

        #endregion

        #region Private Fields

        float _spawnTimer;
        float _waveTimer;
        float _bossSpawnTimer;
        bool _bossSpawnedThisWave;
        List<MonsterSpawnConfig> _pendingForceSpawns = new();
        MonsterScalingData _scalingData = new();
        int _forceSpawnIndex;
        Random _spawnRandom;
        Vector2 _spawnAreaMin;
        Vector2 _spawnAreaMax;

        // ---------------------------------------------------------------
        // Grid 网格视图 (取代 brickLayout).
        //
        // 如果在 Inspector 里手动指定, 则优先使用;
        // 否则 Awake 时通过 FindObjectOfType 自动获取场景中的第一个 GridManager.
        // 它提供 Rows / Columns / CellSize / OriginOffset 等, 把 spawn 点锚定到网格 cell 中心.
        // ---------------------------------------------------------------

        [Tooltip("网格管理器. 不指定时, 在首次 spawn 时通过 FindObjectOfType 自动获取场景中的 GridManager.")]
        public GridManager GridView;

        [Tooltip("形状库列表. 当 enableShapeSpawning 为 true 时, 从这些库中随机选取形状生成.")]
        Dictionary<int, List<ShapeEntry>> shapeDict = new();

        List<int> shapeCellCount = new();

        GridManager _resolvedGridView;
        bool _gridViewResolved;

        // 问题2修复：击败所有怪物策略的最大生成数量
        int _waveMaxTotalSpawn;
        int _waveCurrentTotalSpawn;

        // 持续刷怪相关
        float _killSpeedTimer; // 击杀速度计时器
        int _killsInLastInterval; // 上一个间隔内的击杀数
        int _killsThisInterval; // 当前间隔的击杀数
        float _spawnIntervalOverride; // 刷怪间隔覆盖值（用于紧急补充）

        // 每个 MonsterSpawnConfig 在当前波次中已生成的累计数量
        Dictionary<MonsterSpawnConfig, int> _spawnedCountByConfig = new();

        #endregion

        #region Events

        public event Action<WaveConfig> OnWaveStart;
        public event Action<WaveConfig> OnWaveComplete;
        public event Action<WaveConfig, GameResult> OnWaveFailed;
        public event Action<WaveLevelConfig> OnLevelStart;
        public event Action<WaveLevelConfig> OnLevelComplete;
        public event Action<GameResult> OnGameEnd;
        public event Action<Brick> OnMonsterSpawned;
        public event Action<Brick> OnMonsterKilled;
        public event Action<WaveState> OnStateChanged;
        public event Action<float> OnWaveTimeUpdate;
        public event Action OnRewardSelectionStarted;
        public event Action OnRewardSelectionEnded;

        #endregion

        void Awake()
        {
            _spawnRandom = new();

            var gridManager = ResolveGridManager();
            // 收集所有库中所有非空形状
            using var _ = new ListScope<ShapeEntry>(out var allShapes);
            foreach (var lib in gridManager.ShapesLibrary)
            {
                if (!lib) continue;

                allShapes.AddRange(lib.shapes);
            }

            foreach (var g in allShapes.GroupBy(entry => entry.expandedCells.Count))
            {
                shapeDict[g.Key] = g.ToList();
            }

            shapeCellCount.AddRange(shapeDict.Keys);
            shapeCellCount.Sort();
        }

        /// <summary>
        /// 解析 GridManager 引用. 优先用 Inspector 字段; 否则尝试从场景里找.
        /// 返回 null 表示当前场景中没有可用的 GridManager.
        /// </summary>
        public GridManager ResolveGridManager()
        {
            if (_gridViewResolved)
                return _resolvedGridView;

            if (GridView)
            {
                _resolvedGridView = GridView;
                _gridViewResolved = true;
                return _resolvedGridView;
            }

#if UNITY_2023_1_OR_NEWER
            _resolvedGridView = FindFirstObjectByType<GridManager>(FindObjectsInactive.Include);
#else
            _resolvedGridView = UnityEngine.Object.FindObjectOfType<GridManager>(true);
#endif
            _gridViewResolved = true;
            return _resolvedGridView;
        }

        /// <summary>
        /// 清空 GridView 缓存 (例如场景切换或手动换引用时手动调用).
        /// </summary>
        public void InvalidateGridView()
        {
            _resolvedGridView = null;
            _gridViewResolved = false;
        }


        public void Update()
        {
            var dt = Time.deltaTime;

            if (!IsPlaying)
                return;

            switch (State)
            {
                case WaveState.Preparing:
                    UpdatePreparing(dt);
                    break;
                case WaveState.Active:
                    UpdateActive(dt);
                    break;
                case WaveState.Clearing:
                    UpdateClearing(dt);
                    break;
                case WaveState.RewardSelecting:
                    // 奖励选择阶段不更新波次逻辑，等待玩家完成选择
                    break;
            }

            UpdateMonsterStates();
            OnWaveTimeUpdate?.Invoke(WaveTimeRemaining);
        }

        #region Public Methods

        /// <summary>
        /// 开始一个关卡
        /// </summary>
        public void StartLevel(WaveLevelConfig levelConfig)
        {
            if (levelConfig == null || levelConfig.waves.Count == 0)
            {
                Debug.LogError("[WaveManager] Invalid level config!");
                return;
            }

            CurLevel = levelConfig;
            WaveNumber = 0;
            FinalResult = GameResult.None;
            HasBossSpawned = false;

            // 设置生成区域
            _spawnAreaMin = new(levelConfig.spawnAreaLeft, levelConfig.spawnAreaBottom);
            _spawnAreaMax = new(levelConfig.spawnAreaRight, levelConfig.spawnAreaTop);

            // 初始化随机种子
            _spawnRandom = new();

            // 触发关卡开始事件
            OnLevelStart?.Invoke(levelConfig);
        }

        /// <summary>
        /// 开始下一波
        /// </summary>
        public void StartNextWave()
        {
            if (CurLevel == null)
            {
                Debug.LogError("[WaveManager] No level config loaded!");
                return;
            }

            // 检查是否还有下一波
            if (WaveNumber >= CurLevel.MaxWave)
            {
                CompleteLevel();
                return;
            }

            WaveNumber++;
            CurWave = CurLevel.GetWaveConfig(WaveNumber);
            NextWave = CurLevel.GetWaveConfig(WaveNumber + 1);

            if (CurWave == null)
            {
                Debug.LogError($"[WaveManager] Wave {WaveNumber} config not found!");
                CompleteLevel();
                return;
            }

            // 初始化波次数据
            WaveTimeRemaining = CurWave.duration;
            WaveTimeElapsed = 0f;
            _spawnTimer = 0f;
            _bossSpawnTimer = 0f;
            _bossSpawnedThisWave = false;
            WaveKillCount = 0;
            WaveSpawnCount = 0;
            _forceSpawnIndex = 0;
            HasBossSpawned = false;

            // 问题2修复：初始化击败所有怪物策略的最大生成数量
            _waveMaxTotalSpawn = 0;
            _waveCurrentTotalSpawn = 0;
            if (CurWave.clearStrategy == WaveClearStrategy.DefeatAllMonsters)
            {
                // 击败所有怪物策略：使用配置的GetDefeatAllMaxTotalSpawn()获取最大生成数量
                _waveMaxTotalSpawn = CurWave.GetDefeatAllMaxTotalSpawn();
            }

            // 初始化每个 MonsterSpawnConfig 的累计生成计数
            _spawnedCountByConfig.Clear();
            foreach (var config in CurWave.availableMonsters)
            {
                if (config != null)
                    _spawnedCountByConfig[config] = 0;
            }

            // 准备强制生成的怪物列表
            _pendingForceSpawns.Clear();
            foreach (var config in CurWave.availableMonsters)
            {
                if (config.forceSpawnOnce)
                {
                    _pendingForceSpawns.Add(config);
                }
            }

            // 应用属性增长
            _scalingData.ApplyWaveScaling(WaveNumber, CurWave, CurLevel);

            // 进入准备阶段
            SetState(WaveState.Preparing);

            Debug.Log($"[WaveManager] Wave {WaveNumber} started: {CurWave.waveName}");
        }

        /// <summary>
        /// 跳过准备阶段，直接开始波次
        /// </summary>
        public void SkipPreparing()
        {
            if (State == WaveState.Preparing)
            {
                SetState(WaveState.Active);
            }
        }

        /// <summary>
        /// 手动结束当前波次（用于测试或特殊逻辑）
        /// </summary>
        public void ForceEndWave()
        {
            if (State is WaveState.Active or WaveState.Clearing)
            {
                WaveTimeRemaining = 0f;
            }
        }

        /// <summary>
        /// 强制波次失败
        /// </summary>
        public void ForceWaveFailed()
        {
            FinalResult = GameResult.Defeat;
            SetState(WaveState.Failed);
            OnWaveFailed?.Invoke(CurWave, FinalResult);
            OnGameEnd?.Invoke(FinalResult);
        }

        /// <summary>
        /// 生成一个怪物
        /// </summary>
        /// <param name="originConfig">产生本次生成的 MonsterSpawnConfig（用于跟踪每配置的最少生成个数）。仅在创建成功时计入计数。</param>
        public Brick SpawnMonster(BrickDef monsterDef, Vector3? position = null, MonsterSpawnConfig originConfig = null)
        {
            if (CurWave == null)
                return null;

            // 检查最大怪物数量限制
            int maxMonsters = Mathf.Min(CurWave.maxActiveMonsters, CurLevel.globalMaxActiveMonsters);
            if (ActiveMonsterCount >= maxMonsters)
            {
                Debug.Log("[WaveManager] Max active monsters reached, skipping spawn.");
                return null;
            }

            // 问题2修复：击败所有怪物策略时，检查最大生成总数
            if (_waveMaxTotalSpawn > 0 && _waveCurrentTotalSpawn >= _waveMaxTotalSpawn)
            {
                Debug.Log("[WaveManager] Max total spawn reached for DefeatAll strategy, stopping spawn.");
                return null;
            }

            // 生成位置
            Vector3 spawnPos;
            if (position.HasValue)
                spawnPos = position.Value;
            else if (GetSmartSpawnPosition(out var p))
                spawnPos = p;
            else
            {
                Debug.Log("[WaveManager] Can find empty cell to spawn, skipping spawn.");
                return null;
            }

            // 创建怪物
            var monster = CreateMonster(monsterDef, spawnPos);
            if (monster)
            {
                // 应用属性增长
                ApplyScalingToMonster(monster);

                // 添加到活跃列表
                ActiveMonsters.Add(monster);
                if (monsterDef.Type == SpawnEnemyType.Boss)
                {
                    ActiveBosses.Add(monster);
                }

                WaveSpawnCount++;
                _waveCurrentTotalSpawn++;
                RecordConfigSpawn(originConfig); // 累计 originConfig 的生成数量
                OnMonsterSpawned?.Invoke(monster);

                Debug.Log($"[WaveManager] Spawned {monsterDef.Type} monster: {monsterDef.name} at {spawnPos}");
            }

            return monster;
        }

        /// <summary>
        /// 注册一个外部生成的怪物
        /// </summary>
        public void RegisterExternalMonster(Brick monster, SpawnEnemyType type)
        {
            if (monster == null)
                return;

            ActiveMonsters.Add(monster);
            if (type == SpawnEnemyType.Boss)
            {
                ActiveBosses.Add(monster);
            }
        }

        /// <summary>
        /// 注销一个怪物
        /// </summary>
        public void UnregisterMonster(Brick monster)
        {
            if (monster == null)
                return;

            ActiveMonsters.Remove(monster);
            ActiveBosses.Remove(monster);
            WaveKillCount++;
            NotifyMonsterKilled(); // 持续刷怪：通知击杀
            OnMonsterKilled?.Invoke(monster);
        }

        /// <summary>
        /// 进入奖励选择阶段
        /// </summary>
        public void EnterRewardSelection()
        {
            if (State is WaveState.Clearing or WaveState.Completed)
            {
                SetState(WaveState.RewardSelecting);
                OnRewardSelectionStarted?.Invoke();
                Debug.Log("[WaveManager] Entered reward selection phase.");

                GameManager.Instance.Pause();
            }
        }

        /// <summary>
        /// 离开奖励选择阶段，开始下一波
        /// </summary>
        public void ExitRewardSelection()
        {
            if (State == WaveState.RewardSelecting)
            {
                OnRewardSelectionEnded?.Invoke();
                Debug.Log("[WaveManager] Exited reward selection phase.");

                GameManager.Instance.UnPause();
            }
        }

        List<Vector3> sparsePositions = new();

        /// <summary>
        /// 获取智能生成位置（基于现有怪物密度）
        /// 改造后: 优先使用 Grid2D 系统的 grid 信息 (GridView), 不再依赖 brickLayout.
        /// 在网格上的所有空置 cell 中挑选 "周围最稀疏" 的那个 cell, 返回其中心世界坐标.
        /// </summary>
        public bool GetSmartSpawnPosition(out Vector2 spawnPos)
        {
            if (CurWave is not { enableSmartSpawning: true })
            {
                return GetEdgeBiasedRandomEmptyCell(out var emptyCell, out spawnPos);
            }

            var gm = ResolveGridManager();
            if (gm == null)
            {
                return GetEdgeBiasedRandomEmptyCell(out var emptyCell, out spawnPos);
            }

            var grid = gm.CurrentGrid();
            if (grid.Columns <= 0 || grid.Rows <= 0)
            {
                spawnPos = Vector2.zero;
                return false;
            }

            // 收集所有空置 cell (基于 grid 的 cols/rows), brickManager 上的占用表依然作为来源.
            using var _ = new ListScope<Vector2Int>(out var emptyList);
            brickManager.CollectEmptyCells(ref emptyList);
            if (emptyList.Count == 0)
            {
                spawnPos = Vector2.zero;
                return false;
            }

            sparsePositions.Clear();
            var sampleCount = Mathf.Min(20, emptyList.Count);
            var radius = CurWave.denseRadius;
            for (var i = 0; i < sampleCount; i++)
            {
                var cell = emptyList[_spawnRandom.Next(emptyList.Count)];
                var cellCenter = grid.CellToWorld(cell);
                var samplePos = new Vector3(cellCenter.x, cellCenter.y, 0);
                var nearbyCount = CountNearbyMonsters(samplePos, radius);
                if (nearbyCount <= CurWave.sparseThreshold)
                {
                    sparsePositions.Add(samplePos);
                }
            }

            if (sparsePositions.Count > 0)
            {
                spawnPos = sparsePositions[_spawnRandom.Next(sparsePositions.Count)];
                return true;
            }

            return GetEdgeBiasedRandomEmptyCell(out var _emptyCell, out spawnPos);
        }

        /// <summary>
        /// 获取特定怪物类型的动态生成权重
        /// </summary>
        public SpawnEnemyType GetWeightedEnemyType()
        {
            if (CurWave == null)
                return SpawnEnemyType.Normal;

            // 获取当前怪物类型分布
            int normalCount = 0;
            foreach (var m in ActiveMonsters)
            {
                if (m.type == EnemyType.NORMAL && m.IsAlive())
                    normalCount++;
            }

            int eliteCount = 0;
            foreach (var m in ActiveMonsters)
            {
                if (m.type == EnemyType.ELITE && m.IsAlive())
                    eliteCount++;
            }

            int bossCount = 0;
            foreach (var m in ActiveBosses)
            {
                if (m.type == EnemyType.BOSS && m.IsAlive())
                    bossCount++;
            }

            float totalWeight = CurWave.normalMonsterWeight +
                                CurWave.eliteMonsterWeight +
                                CurWave.bossMonsterWeight;

            // 根据当前分布动态调整权重
            float normalWeight = CurWave.normalMonsterWeight;
            float eliteWeight = CurWave.eliteMonsterWeight;
            float bossWeight = CurWave.bossMonsterWeight;

            // 如果精英怪太多，减少精英权重
            if (eliteCount > 2)
            {
                eliteWeight *= 0.5f;
            }

            // 如果Boss太多，暂不生成Boss
            if (bossCount >= 1)
            {
                bossWeight = 0f;
            }

            // 如果小怪太少，增加小怪权重
            if (normalCount < CurWave.minActiveMonsters)
            {
                normalWeight *= 1.5f;
            }

            // 计算总权重
            totalWeight = normalWeight + eliteWeight + bossWeight;

            // 随机选择
            float roll = (float)_spawnRandom.NextDouble() * totalWeight;

            if (roll < normalWeight)
                return SpawnEnemyType.Normal;
            if (roll < normalWeight + eliteWeight)
                return SpawnEnemyType.Elite;
            return SpawnEnemyType.Boss;
        }

        /// <summary>
        /// 根据类型选择合适的怪物
        /// </summary>
        public bool SelectMonsterByType(SpawnEnemyType type, out BrickDef monsterDef)
        {
            return SelectMonsterByType(type, out monsterDef, out _);
        }

        /// <summary>
        /// 根据类型选择合适的怪物，并返回被选中的 MonsterSpawnConfig（用于追踪每配置的最少生成量）。
        /// 过滤掉已达到 atLeastSpawnCount 上限的配置；
        /// 若所有匹配类型都已达标，则忽略该上限以保证波次能继续进行。
        /// </summary>
        public bool SelectMonsterByType(SpawnEnemyType type, out BrickDef monsterDef, out MonsterSpawnConfig selectedConfig)
        {
            monsterDef = null;
            selectedConfig = null;
            if (CurWave == null || CurWave.availableMonsters.Count == 0)
                return false;

            // 收集满足类型且未达到 atLeastSpawnCount 上限的候选
            using var _ = new ListScope<MonsterSpawnConfig>(out var candidates);
            foreach (var config in CurWave.availableMonsters)
            {
                if (config == null || config.monsterDef == null)
                    continue;
                if (config.monsterDef.Type != type)
                    continue;
                if (IsConfigQuotaReached(config))
                    continue;
                candidates.Add(config);
            }

            // 如果所有匹配类型都已达标，忽略该上限以保证波次正常推进
            if (candidates.Count == 0)
            {
                foreach (var config in CurWave.availableMonsters)
                {
                    if (config == null || config.monsterDef == null)
                        continue;
                    if (config.monsterDef.Type == type)
                        candidates.Add(config);
                }
            }

            // 仍然为空，则回退到所有怪物
            if (candidates.Count == 0)
            {
                // 如果没有该类型，回退到所有怪物
                candidates = CurWave.availableMonsters;
            }

            if (candidates.Count == 0)
                return false;

            // 根据权重随机选择
            float totalWeight = 0;
            foreach (var config in candidates)
            {
                totalWeight += config.spawnWeight;
            }

            float roll = (float)_spawnRandom.NextDouble() * totalWeight;

            foreach (var candidate in candidates)
            {
                roll -= candidate.spawnWeight;
                if (roll <= 0)
                {
                    selectedConfig = candidate;
                    monsterDef = candidate.monsterDef;
                    return monsterDef != null;
                }
            }

            selectedConfig = candidates[0];
            monsterDef = candidates[0].monsterDef;
            return monsterDef != null;
        }

        /// <summary>
        /// 判断指定 MonsterSpawnConfig 在本波次是否已生成到 atLeastSpawnCount 上限。
        /// </summary>
        bool IsConfigQuotaReached(MonsterSpawnConfig config)
        {
            if (config == null || config.atLeastSpawnCount <= 0)
                return false;
            if (_spawnedCountByConfig.TryGetValue(config, out var spawned))
                return spawned >= config.atLeastSpawnCount;
            return false;
        }

        /// <summary>
        /// 记录指定 MonsterSpawnConfig 成功生成了一个怪物。
        /// </summary>
        void RecordConfigSpawn(MonsterSpawnConfig config)
        {
            if (config == null)
                return;
            _spawnedCountByConfig.TryGetValue(config, out var current);
            _spawnedCountByConfig[config] = current + 1;
        }

        /// <summary>
        /// 获取怪物属性增长数据
        /// </summary>
        public MonsterScalingData GetScalingData() => _scalingData;

        /// <summary>
        /// 获取当前动态刷怪间隔
        /// </summary>
        public float GetDynamicSpawnInterval()
        {
            if (CurWave == null)
                return CurLevel?.globalBaseSpawnInterval ?? 2f;

            int currentCount = ActiveMonsterCount;
            int maxCount = Mathf.Min(CurWave.maxActiveMonsters, CurLevel.globalMaxActiveMonsters);
            int minCount = Mathf.Max(CurWave.minActiveMonsters, CurLevel.globalMinActiveMonsters);

            // 如果当前怪物数量接近最大，减少刷怪间隔（更频繁）
            // 如果当前怪物数量接近最小，增加刷怪间隔（更稀疏）
            float ratio = (float)currentCount / maxCount;

            if (ratio < 0.3f)
            {
                // 怪物很少，加快刷怪
                return CurWave.availableMonsters.Count > 0
                    ? CurWave.availableMonsters[0].baseSpawnInterval * 0.7f
                    : CurLevel.globalBaseSpawnInterval * 0.7f;
            }

            if (ratio > 0.8f)
            {
                // 怪物很多，减慢刷怪
                return CurWave.availableMonsters.Count > 0
                    ? CurWave.availableMonsters[0].baseSpawnInterval * 1.5f
                    : CurLevel.globalBaseSpawnInterval * 1.5f;
            }

            return CurWave.availableMonsters.Count > 0
                ? CurWave.availableMonsters[0].baseSpawnInterval
                : CurLevel.globalBaseSpawnInterval;
        }

        /// <summary>
        /// 清理所有活跃怪物
        /// </summary>
        public void ClearAllActiveMonsters()
        {
            foreach (var monster in ActiveMonsters)
            {
                if (monster && monster.IsAlive())
                {
                    // 触发死亡但不掉落奖励
                    monster.die();
                }
            }

            ActiveMonsters.Clear();
            ActiveBosses.Clear();
        }

        /// <summary>
        /// 清理地图掉落物
        /// </summary>
        public void ClearMapDrops()
        {
            // TODO: 实现掉落物清理逻辑
            // Debug.Log("[WaveManager] Clearing map drops...");
        }

        public void SetPlayerHandleWeaponAbilityPermitted(APlayer p, bool active)
        {
            foreach (var handleWeapon in p.handleWeapons)
            {
                if (!active)
                    handleWeapon.ForceStop();

                handleWeapon.SetAbilityPermitted(active);
            }
        }

        public void SetPlayerMovementAbilityPermitted(APlayer p, bool active)
        {
            p.Movement.ResetSpeed();
            p.Movement.ResetAbility();
            p.Movement.SetAbilityPermitted(active);
        }

        public void SetAllMonstersMovementAbilityPermitted(bool active)
        {
            foreach (var brick in ActiveMonsters)
                brick.Controller.MovementDisabled = !active;
            foreach (var brick in ActiveBosses)
                brick.Controller.MovementDisabled = !active;
        }

        /// <summary>
        /// 重置波次管理器
        /// </summary>
        public void Reset()
        {
            ClearAllActiveMonsters();
            CurLevel = null;
            CurWave = null;
            NextWave = null;
            WaveNumber = 0;
            State = WaveState.Idle;
            FinalResult = GameResult.None;
            HasBossSpawned = false;
            _scalingData.Reset();
        }

        #endregion

        #region Private Methods

        void SetState(WaveState newState)
        {
            if (State == newState)
                return;

            var oldState = State;
            State = newState;
            OnStateChanged?.Invoke(State);

            // Debug.Log($"[WaveManager] State changed: {oldState} -> {State}");
        }

        void UpdatePreparing(float dt)
        {
            // 准备阶段可以做一些准备工作
            // 例如显示波次信息、播放音效等
            SetState(WaveState.Active);
            OnWaveStart?.Invoke(CurWave);

            SetPlayerHandleWeaponAbilityPermitted(player, true);
            SetPlayerMovementAbilityPermitted(player, true);
            SetAllMonstersMovementAbilityPermitted(true);
        }

        void UpdateActive(float dt)
        {
            WaveTimeElapsed += dt;

            // 更新倒计时
            if (CurWave.duration > 0)
            {
                WaveTimeRemaining -= dt;
                if (WaveTimeRemaining <= 0)
                {
                    WaveTimeRemaining = 0;
                }
            }

            // 检查通关策略
            if (CheckWinCondition())
            {
                CompleteWave();
                return;
            }

            // 检查失败策略
            if (CheckLoseCondition())
            {
                FinalResult = GameResult.Defeat;
                SetState(WaveState.Failed);
                OnWaveFailed?.Invoke(CurWave, FinalResult);
                OnGameEnd?.Invoke(FinalResult);
                return;
            }

            // 处理强制生成
            ProcessForceSpawns(dt);

            // 处理Boss生成
            ProcessBossSpawn();

            // 处理普通怪物生成
            ProcessMonsterSpawn(dt);

            // 清理死亡怪物
            CleanupDeadMonsters();
        }

        void UpdateClearing(float dt)
        {
            // 等待剩余怪物死亡
            if (ActiveMonsterCount == 0)
            {
                FinishWaveCompletion();
            }
        }

        void UpdateMonsterStates()
        {
            // 定期检查并清理死亡怪物
            for (int i = ActiveMonsters.Count - 1; i >= 0; i--)
            {
                var monster = ActiveMonsters[i];
                if (monster == null || monster.IsDead())
                {
                    // 问题3修复：当怪物死亡时，更新击杀计数
                    if (monster && monster.IsDead())
                    {
                        WaveKillCount++;
                        NotifyMonsterKilled(); // 持续刷怪：通知击杀
                        OnMonsterKilled?.Invoke(monster);
                    }

                    ActiveMonsters.RemoveAt(i);
                }
            }

            for (int i = ActiveBosses.Count - 1; i >= 0; i--)
            {
                var boss = ActiveBosses[i];
                if (boss == null || boss.IsDead())
                {
                    if (boss && boss.IsDead())
                    {
                        WaveKillCount++;
                        NotifyMonsterKilled(); // 持续刷怪：通知击杀
                        OnMonsterKilled?.Invoke(boss);
                    }

                    ActiveBosses.RemoveAt(i);
                }
            }
        }

        bool CheckWinCondition()
        {
            if (CurWave == null)
                return false;

            switch (CurWave.clearStrategy)
            {
                case WaveClearStrategy.SurviveUntilEnd:
                    // 坚持到时间结束
                    return CurWave.duration > 0 && WaveTimeRemaining <= 0;

                case WaveClearStrategy.DefeatAllMonsters:
                    // 击败所有怪物：所有已生成的怪物都被击杀
                    return _waveCurrentTotalSpawn > 0 && ActiveMonsterCount == 0;

                case WaveClearStrategy.DefeatBoss:
                    // 击败Boss
                    return _bossSpawnedThisWave && ActiveBossCount == 0;

                default:
                    return false;
            }
        }

        bool CheckLoseCondition()
        {
            // 如果玩家死亡
            if (player && player.IsDead())
            {
                return true;
            }

            // 如果是击败所有敌人或击败Boss策略，且设置了持续时间
            // 则超时也视为失败
            if (CurWave.clearStrategy == WaveClearStrategy.DefeatAllMonsters ||
                CurWave.clearStrategy == WaveClearStrategy.DefeatBoss)
            {
                // 如果设置了超时时间
                if (CurWave.duration > 0 && WaveTimeRemaining <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        void ProcessForceSpawns(float dt)
        {
            if (_pendingForceSpawns.Count == 0)
                return;

            // 每隔一段时间尝试生成一个强制怪物
            _spawnTimer += dt;

            if (_spawnTimer >= 1f) // 每秒检查一次
            {
                _spawnTimer = 0f;

                // 生成一个强制怪物
                var config = _pendingForceSpawns[_forceSpawnIndex];
                SpawnMonster(config.monsterDef);
                _forceSpawnIndex++;

                if (_forceSpawnIndex >= _pendingForceSpawns.Count)
                {
                    _pendingForceSpawns.Clear();
                }
            }
        }

        void ProcessBossSpawn()
        {
            if (_bossSpawnedThisWave || CurWave == null)
                return;

            // 检查是否应该生成Boss
            if (CurWave.clearStrategy == WaveClearStrategy.DefeatBoss)
            {
                if (WaveTimeElapsed >= CurWave.bossSpawnTime)
                {
                    SpawnBoss();
                }
            }
            else if (CurLevel.IsLastWave(WaveNumber))
            {
                // 最后一波，在特定时间生成Boss
                if (WaveTimeElapsed >= CurWave.bossSpawnTime)
                {
                    SpawnBoss();
                }
            }
        }

        void ProcessMonsterSpawn(float dt)
        {
            if (CurWave == null || CurWave.availableMonsters.Count == 0)
                return;

            // 持续刷怪模式
            if (CurWave.enableContinuousSpawning)
            {
                ProcessContinuousSpawn(dt);
            }
            else
            {
                // 原有逻辑：有限刷怪模式
                ProcessLimitedSpawn(dt);
            }
        }

        /// <summary>
        /// 持续刷怪模式 - 怪物死亡后立即补充
        /// </summary>
        void ProcessContinuousSpawn(float dt)
        {
            // 获取配置参数（优先使用波次配置，否则使用全局配置）
            float targetCoverage = CurWave.targetCoverageRatio;
            float minInterval = CurWave.minSpawnInterval;
            float maxInterval = CurWave.maxSpawnInterval;
            float sensitivity = CurWave.killSpeedSensitivity;

            if (CurLevel != null)
            {
                if (targetCoverage <= 0) targetCoverage = CurLevel.globalTargetCoverageRatio;
                if (minInterval <= 0) minInterval = CurLevel.globalMinSpawnInterval;
                if (maxInterval <= 0) maxInterval = CurLevel.globalMaxSpawnInterval;
            }

            // 计算基于覆盖率的理想怪物数量
            float spawnArea = (_spawnAreaMax.x - _spawnAreaMin.x) * (_spawnAreaMax.y - _spawnAreaMin.y);
            float monsterSize = 0.675f;
            float totalMonsterSlots = spawnArea / (monsterSize * monsterSize);
            int targetMonsterCount = Mathf.FloorToInt(totalMonsterSlots * targetCoverage);
            targetMonsterCount = Mathf.Max(1, targetMonsterCount);

            // 更新击杀速度追踪
            _killSpeedTimer += dt;
            if (_killSpeedTimer >= 1f)
            {
                _killsInLastInterval = _killsThisInterval;
                _killsThisInterval = 0;
                _killSpeedTimer = 0f;
            }

            // 动态计算刷怪间隔
            float currentSpawnInterval = CalculateDynamicSpawnInterval(
                ActiveMonsterCount,
                targetMonsterCount,
                _killsInLastInterval,
                minInterval,
                maxInterval,
                sensitivity);

            // 检查是否可以生成更多怪物
            int maxMonsters = Mathf.Min(CurWave.maxActiveMonsters, CurLevel.globalMaxActiveMonsters);
            maxMonsters = Mathf.Max(maxMonsters, targetMonsterCount); // 确保至少能达到目标数量

            int minCount = Mathf.Max(CurWave.minActiveMonsters, CurLevel.globalMinActiveMonsters);

            // 如果怪物数量低于目标，增加紧迫感
            if (ActiveMonsterCount < targetMonsterCount)
            {
                currentSpawnInterval = Mathf.Min(currentSpawnInterval, minInterval * 2f);
            }

            // 如果怪物数量远低于目标，使用紧急间隔
            if (ActiveMonsterCount < minCount)
            {
                currentSpawnInterval = minInterval;
            }

            // 检查是否需要生成
            if (ActiveMonsterCount < maxMonsters)
            {
                _spawnTimer += dt;

                if (_spawnTimer >= currentSpawnInterval)
                {
                    _spawnTimer = 0f;
                    SpawnRandomMonster();
                }
            }
        }

        /// <summary>
        /// 计算动态刷怪间隔
        /// </summary>
        float CalculateDynamicSpawnInterval(
            int currentMonsters,
            int targetMonsters,
            int killsPerSecond,
            float minInterval,
            float maxInterval,
            float sensitivity)
        {
            // 基础间隔：当前怪物数量与目标的差距越大，间隔越短
            float fillRatio = (float)currentMonsters / targetMonsters;
            float baseInterval = Mathf.Lerp(minInterval, maxInterval, fillRatio);

            // 根据击杀速度调整：如果玩家杀得很快，说明怪物太少了
            // killsPerSecond 表示每秒击杀数，我们需要根据这个调整间隔
            if (killsPerSecond > 0)
            {
                // 每秒击杀超过1个，说明怪物不够用
                float killFactor = Mathf.Min(killsPerSecond * sensitivity * 0.5f, 1f);
                baseInterval = Mathf.Lerp(baseInterval, minInterval, killFactor);
            }

            return baseInterval;
        }

        /// <summary>
        /// 通知怪物被击杀（用于击杀速度追踪）
        /// </summary>
        public void NotifyMonsterKilled()
        {
            _killsThisInterval++;
        }

        /// <summary>
        /// 有限刷怪模式（原逻辑）
        /// </summary>
        void ProcessLimitedSpawn(float dt)
        {
            // 问题2修复：击败所有怪物策略时，检查是否已生成达到上限
            if (_waveMaxTotalSpawn > 0 && _waveCurrentTotalSpawn >= _waveMaxTotalSpawn)
            {
                return;
            }

            // 检查是否可以生成更多怪物
            int maxMonsters = Mathf.Min(CurWave.maxActiveMonsters, CurLevel.globalMaxActiveMonsters);
            if (ActiveMonsterCount >= maxMonsters)
                return;

            // 检查是否需要生成
            if (ActiveMonsterCount < CurWave.minActiveMonsters)
            {
                // 生成怪物补足到最小数量
                // 使用较小间隔批量生成，避免第一帧生成太多
                _spawnTimer += dt;
                float quickSpawnInterval = 0.1f; // 快速填充间隔

                if (_spawnTimer >= quickSpawnInterval)
                {
                    SpawnRandomMonster();
                    _spawnTimer = 0f;
                }
            }
            else
            {
                // 达到最小数量后，按照正常间隔刷怪
                float interval = GetDynamicSpawnInterval();
                _spawnTimer += dt;

                if (_spawnTimer >= interval)
                {
                    _spawnTimer = 0f;
                    SpawnRandomMonster();
                }
            }
        }

        void SpawnBoss()
        {
            if (CurWave.bossMonsterId == null)
            {
                Debug.LogWarning("[WaveManager] Boss monster ID not configured!");
                return;
            }

            SpawnMonster(CurWave.bossMonsterId);
            _bossSpawnedThisWave = true;
            HasBossSpawned = true;
        }

        void SpawnRandomMonster()
        {
            // 决定是生成形状还是单个砖块
            if (CurWave is { enableShapeSpawning: true } && shapeDict is { Count: > 0 })
            {
                // 按权重决定是否生成形状
                float shapeRoll = (float)_spawnRandom.NextDouble() * (CurWave.shapeSpawnWeight + 100f);
                if (shapeRoll < CurWave.shapeSpawnWeight)
                {
                    SpawnRandomShape();
                    return;
                }
            }

            var type = GetWeightedEnemyType();
            if (SelectMonsterByType(type, out var monsterDef, out var pickedConfig))
            {
                SpawnMonster(monsterDef, originConfig: pickedConfig);
            }
        }

        /// <summary>
        /// 从 ShapesLibrary 中随机选取一个 ShapeEntry，在空置的网格位置上生成砖块组合。
        /// </summary>
        void SpawnRandomShape()
        {
            if (shapeDict.Count == 0)
            {
                Debug.LogWarning("[WaveManager] ShapeLibraries is empty, falling back to single brick spawn.");
                return;
            }

            //随机选择这次形状的Cell个数
            var cellCount = shapeCellCount[_spawnRandom.Next(shapeCellCount.Count)];

            // 随机挑一个形状
            var shapeEntries = shapeDict[cellCount];
            var selectedShape = shapeEntries[_spawnRandom.Next(shapeEntries.Count)];

            // 获取形状在世界中的生成位置（使用 edge-biased 逻辑寻找空位）
            GetEdgeBiasedRandomEmptyCell(out var randomEmptyCell, out var randomEmptyCellPos);
            var maxRetries = CurWave?.shapeSpawnMaxRetries ?? 20;
            var found = brickManager.FindEmptyCellForShape(randomEmptyCell, selectedShape.bricks, out var emptyCell, maxRetries);
            if (!found)
            {
                Debug.Log($"[WaveManager] Could not find empty spot for shape '{selectedShape.name}' after {maxRetries} retries.");
                return;
            }

            // 生成砖块
            using var a = new ListScope<BrickTemplate>(out var spawnedBricks);
            var success = brickManager.acquireShape(emptyCell, selectedShape.bricks, CurWave, ref spawnedBricks);
            if (!success || spawnedBricks.Count == 0)
            {
                Debug.LogWarning($"[WaveManager] acquireShape returned empty for shape '{selectedShape.name}'.");
                return;
            }

            // Debug.Log($"[WaveManager] Spawned shape '{selectedShape.name}' with {spawnedBricks.Count} bricks at {emptyCell}");

            // 如果配置了形状上生成怪物，则在每个砖块上生成一个怪物
            foreach (var template in spawnedBricks)
            {
                var type = GetWeightedEnemyType();
                if (SelectMonsterByType(type, out var monsterDef, out var pickedConfig))
                {
                    SpawnMonster(template.def, template.position, originConfig: pickedConfig);
                }
            }
        }

        /// <summary>
        /// 问题4修复：获取偏向边界的随机位置
        /// 改造后: 优先用 Grid2D 系统 (GridView) 取得 cols/rows,
        /// 在"未占用的边缘 cell"中随机挑一个, 返回该 cell 中心的世界坐标.
        /// Grid2D 不可用时, 退回到玩家附近.
        /// </summary>
        bool GetEdgeBiasedRandomEmptyCell(out Vector2Int result, out Vector2 cellPos)
        {
            var gm = ResolveGridManager();
            if (gm == null)
            {
                result = Vector2Int.zero;
                cellPos = Vector2.zero;
                return false;
            }

            var grid = gm.CurrentGrid();
            if (grid.Columns <= 0 || grid.Rows <= 0)
            {
                result = Vector2Int.zero;
                cellPos = Vector2.zero;
                return false;
            }

            int cols = grid.Columns;
            int rows = grid.Rows;

            // 边缘 cell (第一行/最后一行/第一列/最后一列), 且未被占
            float edgeProbability = CurWave?.edgeBiasProbability ?? 0.8f;
            bool preferEdge = (float)_spawnRandom.NextDouble() < edgeProbability;

            bool hasPicked = false;
            Vector2Int picked = default;

            if (preferEdge)
            {
                float edgePercent = CurWave?.edgeBiasPercent ?? 0.8f;
                float edgePercentAmplitude = CurWave?.edgeBiasPercentAmplitude ?? 0.1f;

                // 从 4 个边里随机选, 在该边上随机抽一个未被占的 cell
                int edge = _spawnRandom.Next(4);
                int tries = 0;
                const int kMaxTries = 32;
                while (tries++ < kMaxTries)
                {
                    Vector2Int candidate;
                    int x;
                    int y;
                    switch (edge)
                    {
                        case 0:
                            x = Mathf.RoundToInt(Mathf.Lerp(0, cols - 1, (float)_spawnRandom.NextDouble()));
                            y = Mathf.RoundToInt(Mathf.Lerp(rows / 2F, rows - 1, (float)(edgePercent + edgePercentAmplitude * (_spawnRandom.NextDouble() * 2F - 1F))));
                            candidate = new(x, y); // top
                            break;
                        case 1:
                            x = Mathf.RoundToInt(Mathf.Lerp(0, cols - 1, (float)_spawnRandom.NextDouble()));
                            y = Mathf.RoundToInt(Mathf.Lerp(rows / 2F - 1, 0, (float)(edgePercent + edgePercentAmplitude * (_spawnRandom.NextDouble() * 2F - 1F))));
                            candidate = new(x, y); // bottom
                            break;
                        case 2:
                            x = Mathf.RoundToInt(Mathf.Lerp(cols / 2F - 1, 0, (float)(edgePercent + edgePercentAmplitude * (_spawnRandom.NextDouble() * 2F - 1F))));
                            y = Mathf.RoundToInt(Mathf.Lerp(0, rows - 1, (float)_spawnRandom.NextDouble()));
                            candidate = new(x, y); // left
                            break;
                        case 3:
                            x = Mathf.RoundToInt(Mathf.Lerp(cols / 2F, cols - 1, (float)(edgePercent + edgePercentAmplitude * (_spawnRandom.NextDouble() * 2F - 1F))));
                            y = Mathf.RoundToInt(Mathf.Lerp(0, rows - 1, (float)_spawnRandom.NextDouble()));
                            candidate = new(x, y); // right
                            break;
                        default:
                            candidate = new(_spawnRandom.Next(cols), _spawnRandom.Next(rows)); // random
                            break;
                    }

                    if (brickManager == null || brickManager.IsCellEmpty(candidate))
                    {
                        hasPicked = true;
                        picked = candidate;
                        break;
                    }
                }
            }

            // 没在边缘抽到, 退回到"grid 上所有 cell" 中任选一个空置的
            if (!hasPicked)
            {
                if (brickManager != null)
                {
                    hasPicked = GetRandomEmptyCell(out picked);
                }
                else
                {
                    // 没有 brickManager, 随机挑一个 grid 上的 cell
                    hasPicked = GetRandomCell(cols, rows, out picked);
                }
            }

            if (!hasPicked)
            {
                result = Vector2Int.zero;
                cellPos = Vector2.zero;
                return false;
            }

            // 落在 grid 外的 cell 视为"无" (例如 brickManager 残留的占用记录超出 grid 范围)
            if (picked.x < 0 || picked.x >= cols || picked.y < 0 || picked.y >= rows)
            {
                result = Vector2Int.zero;
                cellPos = Vector2.zero;
                return false;
            }

            result = picked;
            cellPos = grid.CellToWorld(picked);
            return true;
        }

        bool GetRandomEmptyCell(out Vector2Int cell)
        {
            using var _ = new ListScope<Vector2Int>(out var emptyCells);
            brickManager.CollectEmptyCells(ref emptyCells);
            if (emptyCells.Count > 0)
            {
                cell = emptyCells[_spawnRandom.Next(emptyCells.Count)];
                return true;
            }

            cell = default;
            return false;
        }

        bool GetRandomCell(int cols, int rows, out Vector2Int cell)
        {
            cell = new(_spawnRandom.Next(cols), _spawnRandom.Next(rows));
            return true;
        }

        int CountNearbyMonsters(Vector3 position, float radius)
        {
            int count = 0;
            float radiusSq = radius * radius;
            foreach (var monster in ActiveMonsters)
            {
                if (monster == null || monster.IsDead())
                    continue;

                var monsterPos = monster.getWorldPosition();
                var distSq = (position - monsterPos).sqrMagnitude;
                if (distSq < radiusSq)
                {
                    count++;
                }
            }

            return count;
        }

        Brick CreateMonster(BrickDef monsterDef, Vector3 position)
        {
            // Debug.Log($"[WaveManager] Creating monster: {monsterId}, Type: {type}, Position: {position}");
            var brick = brickManager.acquireBrick(monsterDef, position);
            return brick;
        }

        void ApplyScalingToMonster(Brick monster)
        {
            // 应用属性增长到怪物
            if (monster == null)
                return;

            // 例如：
            if (monster.GetStat(Brick.Stat.HealthMax, out var healthMax))
            {
                var bonusHealthPerWave = monster.getDef().BonusHealthPerWave;
                var bonusHealth = bonusHealthPerWave * (WaveNumber - 1);
                if (bonusHealth > 0)
                    healthMax.BonusFlat.AddFlat(bonusHealth);

                var bonusPct = _scalingData.healthMultiplier - 1;
                if (bonusPct > 0)
                    healthMax.BonusPct.AddFlat(bonusPct);

                monster.Health.InitializeCurrentHealth(RefreshHealthBarType.Immediately);
            }

            if (monster.GetStat(Brick.Stat.AD, out var damage))
            {
                var bonusDamagePerWave = monster.getDef().BonusDamagePerWave;
                var bonusDamage = bonusDamagePerWave * (WaveNumber - 1);
                if (bonusDamage > 0)
                    damage.BonusFlat.AddFlat(bonusDamage);

                var bonusPct = _scalingData.damageMultiplier - 1;
                if (bonusPct > 0)
                    damage.BonusPct.AddFlat(bonusPct);
            }

            if (monster.GetStat(Brick.Stat.MS, out var ms))
            {
                var bonusMoveSpeedPerWave = monster.getDef().BonusMoveSpeedPerWave;
                var bonusMoveSpeed = bonusMoveSpeedPerWave * (WaveNumber - 1);
                if (bonusMoveSpeed > 0)
                    ms.BonusFlat.AddFlat(bonusMoveSpeed);

                var bonusPct = _scalingData.speedMultiplier - 1;
                if (bonusPct > 0)
                    ms.BonusPct.AddFlat(bonusPct);
            }

            if (monster.GetStat(Brick.Stat.AR, out var armor))
            {
                var bonusArmorPerWave = monster.getDef().BonusArmorPerWave;
                var bonusArmor = bonusArmorPerWave * (WaveNumber - 1);
                if (bonusArmor > 0)
                    armor.BonusFlat.AddFlat(bonusArmor);

                var bonusPct = _scalingData.defenseMultiplier - 1;
                if (bonusPct > 0)
                    armor.BonusPct.AddFlat(bonusPct);
            }

            if (monster.GetStat(Brick.Stat.KnockbackResistance, out var knockbackResist))
            {
                var bonusKnockbackResistPerWave = monster.getDef().BonusKnockbackResistPerWave;
                var bonusKnockbackResist = bonusKnockbackResistPerWave * (WaveNumber - 1);
                if (bonusKnockbackResist > 0)
                    knockbackResist.BonusFlat.AddFlat(bonusKnockbackResist);

                var bonusPct = _scalingData.knockbackResistMultiplier - 1;
                if (bonusPct > 0)
                    knockbackResist.BonusPct.AddFlat(bonusPct);
            }
        }

        void CleanupDeadMonsters()
        {
            for (int i = ActiveMonsters.Count - 1; i >= 0; i--)
            {
                var monster = ActiveMonsters[i];
                if (monster == null || monster.IsDead())
                {
                    // 问题3修复：确保死亡时更新击杀计数
                    if (monster)
                    {
                        WaveKillCount++;
                        NotifyMonsterKilled(); // 持续刷怪：通知击杀
                        OnMonsterKilled?.Invoke(monster);
                    }

                    ActiveMonsters.RemoveAt(i);
                }
            }
        }

        void CompleteWave()
        {
            SetState(WaveState.Clearing);

            // 清理地图掉落物
            ClearMapDrops();

            // 等待所有怪物死亡
            // if (ActiveMonsterCount == 0)
            {
                FinishWaveCompletion();
            }
        }

        void FinishWaveCompletion()
        {
            SetState(WaveState.Completed);
            OnWaveComplete?.Invoke(CurWave);

            // Debug.Log($"[WaveManager] Wave {WaveNumber} completed! Kills: {WaveKillCount}, Spawns: {WaveSpawnCount}");

            // 清理剩余怪物
            // ClearAllActiveMonsters();

            SetPlayerHandleWeaponAbilityPermitted(player, false);
            SetPlayerMovementAbilityPermitted(player, false);
            SetAllMonstersMovementAbilityPermitted(false);
        }

        void CompleteLevel()
        {
            FinalResult = GameResult.Victory;
            SetState(WaveState.AllCleared);
            OnLevelComplete?.Invoke(CurLevel);
            OnGameEnd?.Invoke(FinalResult);

            Debug.Log($"[WaveManager] Level completed! All waves cleared!");
        }

        #endregion

        public void OnDestroy()
        {
            Reset();
        }


        void OnGUI()
        {
            var m = this;
            GUILayout.BeginArea(new Rect(10, 10, 320, 450));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"=== Wave Debug Info ===");
            GUILayout.Label($"State: {m.State}");
            GUILayout.Label($"Wave: {m.WaveNumber}/{m.CurLevel?.MaxWave ?? 0}");
            GUILayout.Label($"Active Monsters: {m.ActiveMonsterCount}");
            GUILayout.Label($"Active Bosses: {m.ActiveBossCount}");
            GUILayout.Label($"Time Remaining: {m.WaveTimeRemaining:F1}s");
            GUILayout.Label($"Time Elapsed: {m.WaveTimeElapsed:F1}s");
            GUILayout.Label($"Kill Count: {m.WaveKillCount}");
            GUILayout.Label($"Spawn Count: {m.WaveSpawnCount}");

            GUILayout.Space(10);
            GUILayout.Label($"=== Continuous Spawning ===");
            if (CurWave != null)
            {
                GUILayout.Label($"Continuous Mode: {(CurWave.enableContinuousSpawning ? "ON" : "OFF")}");
                GUILayout.Label($"Target Coverage: {CurWave.targetCoverageRatio:P0}");
                GUILayout.Label($"Min/Max Interval: {CurWave.minSpawnInterval:F1}s / {CurWave.maxSpawnInterval:F1}s");
            }

            GUILayout.Label($"Kills/sec (last): {_killsInLastInterval}");
            GUILayout.Label($"Kills/sec (curr): {_killsThisInterval}");

            GUILayout.Space(10);
            GUILayout.Label($"=== Scaling ===");
            var scaling = m.GetScalingData();
            GUILayout.Label($"Health: x{scaling.healthMultiplier:F2}");
            GUILayout.Label($"Damage: x{scaling.damageMultiplier:F2}");
            GUILayout.Label($"Speed: x{scaling.speedMultiplier:F2}");
            GUILayout.Label($"Defense: x{scaling.defenseMultiplier:F2}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
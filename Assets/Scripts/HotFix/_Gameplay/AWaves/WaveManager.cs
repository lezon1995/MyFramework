using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 波次状态
    /// </summary>
    public enum WaveState
    {
        Idle,           // 空闲/未开始
        Preparing,      // 准备阶段
        Active,         // 波次进行中
        Clearing,       // 清理阶段（等待剩余怪物死亡）
        RewardSelecting, // 奖励选择阶段
        Completed,      // 波次完成
        Failed,         // 波次失败
        AllCleared      // 全部波次通关
    }

    /// <summary>
    /// 游戏结果
    /// </summary>
    public enum GameResult
    {
        Victory,        // 胜利
        Defeat,         // 失败
        None            // 未定
    }

    /// <summary>
    /// 波次刷怪管理器 - 核心系统
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        #region Properties
        /// <summary>
        /// 当前关卡配置
        /// </summary>
        public WaveLevelConfig CurLevel { get; set; }

        /// <summary>
        /// 当前波次配置
        /// </summary>
        public WaveConfig CurWave { get; set; }

        /// <summary>
        /// 当前波次编号（从1开始）
        /// </summary>
        public int WaveNumber { get; set; }

        /// <summary>
        /// 当前波次状态
        /// </summary>
        public WaveState State { get; set; } = WaveState.Idle;

        /// <summary>
        /// 当前波次剩余时间
        /// </summary>
        public float WaveTimeRemaining { get; set; }

        /// <summary>
        /// 当前波次已用时间
        /// </summary>
        public float WaveTimeElapsed { get; set; }

        /// <summary>
        /// 当前活跃的怪物列表
        /// </summary>
        public List<AMonster> ActiveMonsters { get; } = new();

        /// <summary>
        /// 当前存活的Boss列表
        /// </summary>
        public List<AMonster> ActiveBosses { get; } = new();

        /// <summary>
        /// 波次内的怪物击杀数
        /// </summary>
        public int WaveKillCount { get; set; }

        /// <summary>
        /// 波次内的总生成怪物数
        /// </summary>
        public int WaveSpawnCount { get; set; }

        /// <summary>
        /// 游戏最终结果
        /// </summary>
        public GameResult FinalResult { get; set; } = GameResult.None;

        /// <summary>
        /// 是否正在进行游戏
        /// </summary>
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
        public int ActiveMonsterCount => ActiveMonsters.Count(m => m.IsAlive());

        /// <summary>
        /// 当前存活Boss数量
        /// </summary>
        public int ActiveBossCount => ActiveBosses.Count(b => b.IsAlive());

        /// <summary>
        /// 是否Boss已生成
        /// </summary>
        public bool HasBossSpawned { get; set; }

        /// <summary>
        /// 是否处于奖励选择阶段
        /// </summary>
        public bool IsInRewardSelection => State == WaveState.RewardSelecting;
        #endregion

        #region Private Fields
        float _spawnTimer;
        float _waveTimer;
        float _bossSpawnTimer;
        bool _bossSpawnedThisWave;
        List<MonsterSpawnConfig> _pendingForceSpawns = new();
        MonsterScalingData _scalingData = new();
        int _forceSpawnIndex;
        System.Random _spawnRandom;
        Vector2 _spawnAreaMin;
        Vector2 _spawnAreaMax;

        // 问题2修复：击败所有怪物策略的最大生成数量
        int _waveMaxTotalSpawn;
        int _waveCurrentTotalSpawn;
        #endregion

        #region Events
        public event Action<WaveConfig> OnWaveStart;
        public event Action<WaveConfig> OnWaveComplete;
        public event Action<WaveConfig, GameResult> OnWaveFailed;
        public event Action<WaveLevelConfig> OnLevelStart;
        public event Action<WaveLevelConfig> OnLevelComplete;
        public event Action<GameResult> OnGameEnd;
        public event Action<AMonster> OnMonsterSpawned;
        public event Action<AMonster> OnMonsterKilled;
        public event Action<WaveState> OnStateChanged;
        public event Action<float> OnWaveTimeUpdate;
        public event Action OnRewardSelectionStarted;
        public event Action OnRewardSelectionEnded;
        #endregion

        void Awake()
        {
            _spawnRandom = new();
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

            // 准备强制生成的怪物列表
            _pendingForceSpawns = CurWave.availableMonsters
                .Where(m => m.forceSpawnOnce)
                .ToList();

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
        public AMonster SpawnMonster(string monsterId, SpawnEnemyType type, Vector3? position = null)
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
            else
                spawnPos = GetSmartSpawnPosition();

            // 创建怪物
            var monster = CreateMonster(monsterId, type, spawnPos);
            if (monster)
            {
                // 应用属性增长
                ApplyScalingToMonster(monster);

                // 添加到活跃列表
                ActiveMonsters.Add(monster);
                if (type == SpawnEnemyType.Boss)
                {
                    ActiveBosses.Add(monster);
                }

                WaveSpawnCount++;
                _waveCurrentTotalSpawn++;
                OnMonsterSpawned?.Invoke(monster);

                Debug.Log($"[WaveManager] Spawned {type} monster: {monsterId} at {spawnPos}");
            }

            return monster;
        }

        /// <summary>
        /// 注册一个外部生成的怪物
        /// </summary>
        public void RegisterExternalMonster(AMonster monster, SpawnEnemyType type)
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
        public void UnregisterMonster(AMonster monster)
        {
            if (monster == null) 
                return;

            ActiveMonsters.Remove(monster);
            ActiveBosses.Remove(monster);
            WaveKillCount++;
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
            }
        }

        List<Vector3> sparsePositions = new();
        
        /// <summary>
        /// 获取智能生成位置（基于现有怪物密度）
        /// </summary>
        public Vector3 GetSmartSpawnPosition()
        {
            if (CurWave is not { enableSmartSpawning: true })
            {
                return GetEdgeBiasedRandomPosition();
            }

            sparsePositions.Clear();
            // 查找稀疏区域
            int sampleCount = 20;
            for (int i = 0; i < sampleCount; i++)
            {
                // 问题4修复：使用边界偏移的位置采样
                Vector3 samplePos = GetEdgeBiasedRandomPosition();
                int nearbyCount = CountNearbyMonsters(samplePos, CurWave.denseRadius);
                if (nearbyCount <= CurWave.sparseThreshold)
                {
                    sparsePositions.Add(samplePos);
                }
            }

            // 如果找到足够多的稀疏位置，从中选择一个
            if (sparsePositions.Count > 0)
            {
                return sparsePositions[_spawnRandom.Next(sparsePositions.Count)];
            }

            // 没有稀疏区域，选择边界位置
            return GetEdgeBiasedRandomPosition();
        }

        /// <summary>
        /// 获取特定怪物类型的动态生成权重
        /// </summary>
        public SpawnEnemyType GetWeightedEnemyType()
        {
            if (CurWave == null)
                return SpawnEnemyType.Normal;

            // 获取当前怪物类型分布
            int normalCount = ActiveMonsters.Count(m => m.type == EnemyType.NORMAL && m.IsAlive());
            int eliteCount = ActiveMonsters.Count(m => m.type == EnemyType.ELITE && m.IsAlive());
            int bossCount = ActiveBosses.Count(b => b.IsAlive());

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
        public string SelectMonsterByType(SpawnEnemyType type)
        {
            if (CurWave == null || CurWave.availableMonsters.Count == 0)
                return null;

            var candidates = CurWave.availableMonsters
                .Where(m => m.enemyType == type)
                .ToList();

            if (candidates.Count == 0)
            {
                // 如果没有该类型，回退到所有怪物
                candidates = CurWave.availableMonsters;
            }

            if (candidates.Count == 0)
                return null;

            // 根据权重随机选择
            float totalWeight = candidates.Sum(c => c.spawnWeight);
            float roll = (float)_spawnRandom.NextDouble() * totalWeight;

            foreach (var candidate in candidates)
            {
                roll -= candidate.spawnWeight;
                if (roll <= 0)
                    return candidate.monsterId;
            }

            return candidates[0].monsterId;
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
            Debug.Log("[WaveManager] Clearing map drops...");
        }

        /// <summary>
        /// 重置波次管理器
        /// </summary>
        public void Reset()
        {
            ClearAllActiveMonsters();
            CurLevel = null;
            CurWave = null;
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

            Debug.Log($"[WaveManager] State changed: {oldState} -> {State}");
        }

        void UpdatePreparing(float dt)
        {
            // 准备阶段可以做一些准备工作
            // 例如显示波次信息、播放音效等
            SetState(WaveState.Active);
            OnWaveStart?.Invoke(CurWave);
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
                if (ActiveMonsters[i] == null || ActiveMonsters[i].IsDead())
                {
                    // 问题3修复：当怪物死亡时，更新击杀计数
                    if (ActiveMonsters[i] != null && ActiveMonsters[i].IsDead())
                    {
                        WaveKillCount++;
                    }
                    ActiveMonsters.RemoveAt(i);
                }
            }

            for (int i = ActiveBosses.Count - 1; i >= 0; i--)
            {
                if (ActiveBosses[i] == null || ActiveBosses[i].IsDead())
                {
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

            // 其他失败条件可以在这里添加
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
                SpawnMonster(config.monsterId, config.enemyType);
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
                // 立即生成
                SpawnRandomMonster();
                _spawnTimer = 0f;
            }
            else
            {
                // 根据间隔计时
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
            if (string.IsNullOrEmpty(CurWave.bossMonsterId))
            {
                Debug.LogWarning("[WaveManager] Boss monster ID not configured!");
                return;
            }

            SpawnMonster(CurWave.bossMonsterId, SpawnEnemyType.Boss);
            _bossSpawnedThisWave = true;
            HasBossSpawned = true;
        }

        void SpawnRandomMonster()
        {
            SpawnEnemyType type = GetWeightedEnemyType();
            string monsterId = SelectMonsterByType(type);

            if (!string.IsNullOrEmpty(monsterId))
            {
                SpawnMonster(monsterId, type);
            }
        }

        /// <summary>
        /// 问题4修复：获取偏向边界的随机位置
        /// 根据配置的edgeBias参数决定在边界生成的概率
        /// </summary>
        Vector3 GetEdgeBiasedRandomPosition()
        {
            float areaWidth = _spawnAreaMax.x - _spawnAreaMin.x;
            float areaHeight = _spawnAreaMax.y - _spawnAreaMin.y;

            // 边界区域的宽度/高度（整体的20%）
            float edgeMarginX = areaWidth * 0.2f;
            float edgeMarginY = areaHeight * 0.2f;

            float x, y;

            // 根据配置的edgeBias决定在边界区域生成的概率
            float edgeProbability = CurWave?.edgeBias ?? 0.8f;
            if ((float)_spawnRandom.NextDouble() < edgeProbability)
            {
                // 选择是哪条边：上、下、左、右
                int edge = _spawnRandom.Next(4);
                switch (edge)
                {
                    case 0: // 上边
                        x = Mathf.Lerp(_spawnAreaMin.x + edgeMarginX, _spawnAreaMax.x - edgeMarginX, (float)_spawnRandom.NextDouble());
                        y = _spawnAreaMax.y - edgeMarginY * (float)_spawnRandom.NextDouble();
                        break;
                    case 1: // 下边
                        x = Mathf.Lerp(_spawnAreaMin.x + edgeMarginX, _spawnAreaMax.x - edgeMarginX, (float)_spawnRandom.NextDouble());
                        y = _spawnAreaMin.y + edgeMarginY * (float)_spawnRandom.NextDouble();
                        break;
                    case 2: // 左边
                        x = _spawnAreaMin.x + edgeMarginX * (float)_spawnRandom.NextDouble();
                        y = Mathf.Lerp(_spawnAreaMin.y + edgeMarginY, _spawnAreaMax.y - edgeMarginY, (float)_spawnRandom.NextDouble());
                        break;
                    default: // 右边
                        x = _spawnAreaMax.x - edgeMarginX * (float)_spawnRandom.NextDouble();
                        y = Mathf.Lerp(_spawnAreaMin.y + edgeMarginY, _spawnAreaMax.y - edgeMarginY, (float)_spawnRandom.NextDouble());
                        break;
                }
            }
            else
            {
                // 中间区域
                x = Mathf.Lerp(_spawnAreaMin.x + edgeMarginX, _spawnAreaMax.x - edgeMarginX, (float)_spawnRandom.NextDouble());
                y = Mathf.Lerp(_spawnAreaMin.y + edgeMarginY, _spawnAreaMax.y - edgeMarginY, (float)_spawnRandom.NextDouble());
            }

            return new Vector3(x, y, 0);
        }

        int CountNearbyMonsters(Vector3 position, float radius)
        {
            int count = 0;
            float radiusSq = radius * radius;

            foreach (var monster in ActiveMonsters)
            {
                if (monster == null || monster.IsDead())
                    continue;

                Vector3 monsterPos = monster.getWorldPosition();
                float distSq = (position - monsterPos).sqrMagnitude;

                if (distSq < radiusSq)
                {
                    count++;
                }
            }

            return count;
        }

        AMonster CreateMonster(string monsterId, SpawnEnemyType type, Vector3 position)
        {
            Debug.Log($"[WaveManager] Creating monster: {monsterId}, Type: {type}, Position: {position}");
            var brick = brickManager.acquireBrick(position, new(1, 1));

            return brick;
        }

        void ApplyScalingToMonster(AMonster monster)
        {
            // 应用属性增长到怪物
            if (monster == null) 
                return;

            // TODO: 根据项目的属性系统实现
            // 例如：
            // monster.Stats.SetStat(Character.Stat.HealthMax, baseValue * _scalingData.healthMultiplier);
            // monster.Stats.SetStat(Character.Stat.AD, baseValue * _scalingData.damageMultiplier);
        }

        void CleanupDeadMonsters()
        {
            for (int i = ActiveMonsters.Count - 1; i >= 0; i--)
            {
                if (ActiveMonsters[i] == null || ActiveMonsters[i].IsDead())
                {
                    // 问题3修复：确保死亡时更新击杀计数
                    if (ActiveMonsters[i] != null)
                    {
                        WaveKillCount++;
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

            Debug.Log($"[WaveManager] Wave {WaveNumber} completed! Kills: {WaveKillCount}, Spawns: {WaveSpawnCount}");

            // 清理剩余怪物
            ClearAllActiveMonsters();
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
            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
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

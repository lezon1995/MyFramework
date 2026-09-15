using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Ball x Pit 玩法的刷怪策略（类球球你/类俄罗斯方块）
    /// 特点：
    /// - 砖块在网格内生成
    /// - 支持多种子策略：顶部行、随机行、随机列、随机空位
    /// - 砖块可以有不同的尺寸（1x1, 1x2, 2x1, 2x2等）
    /// - 所有生成的砖块必须在网格范围内
    /// </summary>
    public class BallXPitLikeMonsterSpawnStrategy : MonsterSpawnStrategy
    {
        [Header("子策略配置")]
        [Tooltip("当前使用的子策略类型")]
        public SubSpawnStrategyType currentSubStrategy = SubSpawnStrategyType.TopRow;

        [Tooltip("可用子策略列表")]
        public List<SubSpawnStrategyConfig> subStrategies = new();

        [Header("生成配置")]
        [Tooltip("刷怪间隔（秒）")]
        public float spawnInterval = 2f;

        [Tooltip("每批生成数量")]
        public int spawnCountPerBatch = 3;

        [Tooltip("最大同时存活的砖块数量")]
        public int maxActiveBricks = 20;

        [Tooltip("最小存活砖块数量")]
        public int minActiveBricks = 5;

        [Header("砖块尺寸配置")]
        [Tooltip("可用砖块尺寸及其权重")]
        public List<BrickSizeConfig> brickSizeConfigs = new();

        [Tooltip("是否允许生成多格砖块（1x2, 2x1, 2x2等）")]
        public bool allowMultiCellBricks = true;

        [Header("生成位置约束")]
        [Tooltip("允许生成的最小行（从底部算起）")]
        public int minSpawnRow = 0;

        [Tooltip("允许生成的最大行（从底部算起，-1表示最顶部）")]
        public int maxSpawnRow = -1;

        // 当前活动的子策略实例
        SubSpawnStrategy _currentStrategy;
        Dictionary<SubSpawnStrategyType, SubSpawnStrategy> _strategyCache = new();

        // Grid信息
        int _gridCols;
        int _gridRows;

        public override void Initialize(WaveManager waveManager, WaveConfig waveConfig, WaveLevelConfig levelConfig)
        {
            base.Initialize(waveManager, waveConfig, levelConfig);

            // 初始化Grid信息
            var gridManager = waveManager?.ResolveGridManager();
            if (gridManager != null)
            {
                var grid = gridManager.CurrentGrid();
                _gridCols = grid.Columns;
                _gridRows = grid.Rows;
            }
            else
            {
                _gridCols = 28;
                _gridRows = 16;
            }

            // 设置默认砖块尺寸配置
            if (brickSizeConfigs.Count == 0)
            {
                brickSizeConfigs = new List<BrickSizeConfig>
                {
                    new BrickSizeConfig { size = new Vector2Int(1, 1), weight = 10f },
                    new BrickSizeConfig { size = new Vector2Int(2, 1), weight = 5f },
                    new BrickSizeConfig { size = new Vector2Int(1, 2), weight = 5f },
                    new BrickSizeConfig { size = new Vector2Int(2, 2), weight = 2f },
                };
            }

            // 初始化子策略
            InitializeSubStrategies();

            // 设置当前策略
            SetCurrentStrategy(currentSubStrategy);
        }

        /// <summary>
        /// 初始化所有子策略
        /// </summary>
        void InitializeSubStrategies()
        {
            _strategyCache.Clear();

            foreach (var config in subStrategies)
            {
                SubSpawnStrategy strategy = config.strategyType switch
                {
                    SubSpawnStrategyType.TopRow => CreateTopRowStrategy(config),
                    SubSpawnStrategyType.RandomRow => CreateRandomRowStrategy(config),
                    SubSpawnStrategyType.RandomCol => CreateRandomColStrategy(config),
                    SubSpawnStrategyType.RandomEmpty => CreateRandomEmptyStrategy(config),
                    _ => null
                };

                if (strategy != null)
                {
                    _strategyCache[config.strategyType] = strategy;
                }
            }

            // 如果没有配置任何策略，创建默认策略
            if (_strategyCache.Count == 0)
            {
                var defaultConfig = new SubSpawnStrategyConfig
                {
                    strategyType = SubSpawnStrategyType.TopRow
                };
                _strategyCache[SubSpawnStrategyType.TopRow] = CreateTopRowStrategy(defaultConfig);
            }
        }

        SubSpawnStrategy CreateTopRowStrategy(SubSpawnStrategyConfig config)
        {
            var strategy = new TopRowStrategy();
            strategy.Initialize(_waveManager, _waveConfig, _levelConfig, _gridCols, _gridRows, config, brickSizeConfigs);
            return strategy;
        }

        SubSpawnStrategy CreateRandomRowStrategy(SubSpawnStrategyConfig config)
        {
            var strategy = new RandomRowStrategy();
            strategy.Initialize(_waveManager, _waveConfig, _levelConfig, _gridCols, _gridRows, config, brickSizeConfigs);
            return strategy;
        }

        SubSpawnStrategy CreateRandomColStrategy(SubSpawnStrategyConfig config)
        {
            var strategy = new RandomColStrategy();
            strategy.Initialize(_waveManager, _waveConfig, _levelConfig, _gridCols, _gridRows, config, brickSizeConfigs);
            return strategy;
        }

        SubSpawnStrategy CreateRandomEmptyStrategy(SubSpawnStrategyConfig config)
        {
            var strategy = new RandomEmptyStrategy();
            strategy.Initialize(_waveManager, _waveConfig, _levelConfig, _gridCols, _gridRows, config, brickSizeConfigs);
            return strategy;
        }

        /// <summary>
        /// 设置当前使用的子策略
        /// </summary>
        public void SetCurrentStrategy(SubSpawnStrategyType strategyType)
        {
            currentSubStrategy = strategyType;
            if (_strategyCache.TryGetValue(strategyType, out var strategy))
            {
                _currentStrategy = strategy;
            }
        }

        public override MonsterSpawnStrategyType GetStrategyType()
        {
            return MonsterSpawnStrategyType.BallXPitLike;
        }

        public override void Update(float dt, int activeMonsterCount)
        {
            if (_currentStrategy == null)
                return;

            // 如果没有配置子策略但有默认策略可用，也执行更新
            if (_currentStrategy == null && _strategyCache.Count > 0)
            {
                foreach (var kvp in _strategyCache)
                {
                    _currentStrategy = kvp.Value;
                    break;
                }
            }

            // 检查是否可以生成更多砖块
            if (activeMonsterCount >= maxActiveBricks)
                return;

            // 更新刷怪计时器
            _spawnTimer += dt;

            // 如果当前活跃数量低于最小值，加快刷怪
            float currentInterval = spawnInterval;
            if (activeMonsterCount < minActiveBricks)
            {
                currentInterval = spawnInterval * 0.5f; // 加快刷怪
            }

            if (_spawnTimer >= currentInterval)
            {
                _spawnTimer = 0f;

                // 生成一批砖块
                int batchSize = Mathf.Min(spawnCountPerBatch, maxActiveBricks - activeMonsterCount);
                for (int i = 0; i < batchSize; i++)
                {
                    _currentStrategy?.TrySpawnOne();
                }
            }
        }

        /// <summary>
        /// 从可用尺寸中根据权重随机选择一个尺寸
        /// </summary>
        public Vector2Int GetRandomBrickSize()
        {
            if (brickSizeConfigs.Count == 0)
                return new Vector2Int(1, 1);

            // 计算总权重
            float totalWeight = 0f;
            foreach (var config in brickSizeConfigs)
            {
                totalWeight += config.weight;
            }

            if (totalWeight <= 0)
                return new Vector2Int(1, 1);

            // 根据权重随机选择
            float roll = (float)_spawnRandom.NextDouble() * totalWeight;
            float cumulativeWeight = 0f;

            foreach (var config in brickSizeConfigs)
            {
                cumulativeWeight += config.weight;
                if (roll <= cumulativeWeight)
                {
                    // 如果不允许多格砖块，只返回1x1
                    if (!allowMultiCellBricks && (config.size.x > 1 || config.size.y > 1))
                    {
                        return new Vector2Int(1, 1);
                    }

                    return config.size;
                }
            }

            return brickSizeConfigs[0].size;
        }

        public override void Reset()
        {
            base.Reset();
            _currentStrategy = null;
            _strategyCache.Clear();
        }
    }

    /// <summary>
    /// 子策略配置
    /// </summary>
    [Serializable]
    public class SubSpawnStrategyConfig
    {
        [Tooltip("策略类型")]
        public SubSpawnStrategyType strategyType = SubSpawnStrategyType.TopRow;

        [Tooltip("权重（用于随机选择）")]
        public float weight = 1f;

        [Tooltip("启用此策略")]
        public bool enabled = true;

        [Tooltip("生成数量（-1表示使用默认）")]
        public int spawnCount = -1;

        [Tooltip("生成间隔（-1表示使用默认）")]
        public float spawnInterval = -1f;

        [Header("TopRow 特定配置")]
        [Tooltip("顶部行策略：从第几行开始生成（从顶部算起，0是最顶部）")]
        public int topRowStartOffset;

        [Header("RandomRow/RandomCol 特定配置")]
        [Tooltip("随机行/列策略：每批生成的数量")]
        public int batchSize = 3;

        [Header("RandomEmpty 特定配置")]
        [Tooltip("随机空位策略：是否优先在边缘生成")]
        public bool preferEdge = true;
    }
}
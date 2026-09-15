using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 幸存者玩法的刷怪策略（类吸血鬼幸存者）
    /// 特点：
    /// - 怪物从场地边缘生成
    /// - 持续刷怪，怪物死亡后立即补充
    /// - 支持智能刷怪（基于密度）
    /// - 支持形状生成
    /// </summary>
    public class VampireLikeMonsterSpawnStrategy : MonsterSpawnStrategy
    {
        private Vector2 _spawnAreaMin;
        private Vector2 _spawnAreaMax;
        private int _waveMaxTotalSpawn;
        private float _spawnIntervalOverride;

        public override void Initialize(WaveManager waveManager, WaveConfig waveConfig, WaveLevelConfig levelConfig)
        {
            base.Initialize(waveManager, waveConfig, levelConfig);

            // 设置生成区域
            if (levelConfig != null)
            {
                _spawnAreaMin = new Vector2(levelConfig.spawnAreaLeft, levelConfig.spawnAreaBottom);
                _spawnAreaMax = new Vector2(levelConfig.spawnAreaRight, levelConfig.spawnAreaTop);
            }
            else
            {
                _spawnAreaMin = new Vector2(-10, -10);
                _spawnAreaMax = new Vector2(10, 10);
            }

            // 击败所有怪物策略的最大生成数量
            _waveMaxTotalSpawn = 0;
            if (waveConfig?.clearStrategy == WaveClearStrategy.DefeatAllMonsters)
            {
                _waveMaxTotalSpawn = waveConfig.GetDefeatAllMaxTotalSpawn();
            }

            _spawnIntervalOverride = 0f;
        }

        public override MonsterSpawnStrategyType GetStrategyType()
        {
            return MonsterSpawnStrategyType.VampireLike;
        }

        public override void Update(float dt, int activeMonsterCount)
        {
            if (_waveConfig == null || _waveConfig.availableMonsters.Count == 0)
                return;

            // 持续刷怪模式
            if (_waveConfig.enableContinuousSpawning)
            {
                ProcessContinuousSpawn(dt, activeMonsterCount);
            }
            else
            {
                // 有限刷怪模式
                ProcessLimitedSpawn(dt, activeMonsterCount);
            }
        }

        /// <summary>
        /// 持续刷怪模式 - 怪物死亡后立即补充
        /// </summary>
        private void ProcessContinuousSpawn(float dt, int activeMonsterCount)
        {
            // 获取配置参数（优先使用波次配置，否则使用全局配置）
            float targetCoverage = _waveConfig.targetCoverageRatio;
            float minInterval = _waveConfig.minSpawnInterval;
            float maxInterval = _waveConfig.maxSpawnInterval;
            float sensitivity = _waveConfig.killSpeedSensitivity;

            if (_levelConfig != null)
            {
                if (targetCoverage <= 0) targetCoverage = _levelConfig.globalTargetCoverageRatio;
                if (minInterval <= 0) minInterval = _levelConfig.globalMinSpawnInterval;
                if (maxInterval <= 0) maxInterval = _levelConfig.globalMaxSpawnInterval;
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
                activeMonsterCount,
                targetMonsterCount,
                _killsInLastInterval,
                minInterval,
                maxInterval,
                sensitivity);

            // 检查是否可以生成更多怪物
            int maxMonsters = Mathf.Min(_waveConfig.maxActiveMonsters, 
                _levelConfig?.globalMaxActiveMonsters ?? int.MaxValue);
            maxMonsters = Mathf.Max(maxMonsters, targetMonsterCount); // 确保至少能达到目标数量

            int minCount = Mathf.Max(_waveConfig.minActiveMonsters, 
                _levelConfig?.globalMinActiveMonsters ?? 0);

            // 如果怪物数量低于目标，增加紧迫感
            if (activeMonsterCount < targetMonsterCount)
            {
                currentSpawnInterval = Mathf.Min(currentSpawnInterval, minInterval * 2f);
            }

            // 如果怪物数量远低于目标，使用紧急间隔
            if (activeMonsterCount < minCount)
            {
                currentSpawnInterval = minInterval;
            }

            // 如果有紧急覆盖值，优先使用
            if (_spawnIntervalOverride > 0)
            {
                currentSpawnInterval = _spawnIntervalOverride;
                _spawnIntervalOverride = 0f;
            }

            // 检查是否需要生成
            if (activeMonsterCount < maxMonsters)
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
        private float CalculateDynamicSpawnInterval(
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
            if (killsPerSecond > 0)
            {
                // 每秒击杀超过1个，说明怪物不够用
                float killFactor = Mathf.Min(killsPerSecond * sensitivity * 0.5f, 1f);
                baseInterval = Mathf.Lerp(baseInterval, minInterval, killFactor);
            }

            return baseInterval;
        }

        /// <summary>
        /// 有限刷怪模式
        /// </summary>
        private void ProcessLimitedSpawn(float dt, int activeMonsterCount)
        {
            // 击败所有怪物策略时，检查是否已生成达到上限
            if (_waveMaxTotalSpawn > 0 && _waveCurrentTotalSpawn >= _waveMaxTotalSpawn)
            {
                return;
            }

            // 检查是否可以生成更多怪物
            int maxMonsters = Mathf.Min(_waveConfig.maxActiveMonsters, 
                _levelConfig?.globalMaxActiveMonsters ?? int.MaxValue);
            if (activeMonsterCount >= maxMonsters)
                return;

            // 检查是否需要生成
            if (activeMonsterCount < _waveConfig.minActiveMonsters)
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

        /// <summary>
        /// 生成随机怪物
        /// </summary>
        private void SpawnRandomMonster()
        {
            // 决定是生成形状还是单个砖块
            if (_waveConfig != null && 
                _waveConfig.enableShapeSpawning && 
                _waveManager != null &&
                _waveManager.ShapeDict != null && 
                _waveManager.ShapeDict.Count > 0)
            {
                // 按权重决定是否生成形状
                float shapeRoll = (float)_spawnRandom.NextDouble() * (_waveConfig.shapeSpawnWeight + 100f);
                if (shapeRoll < _waveConfig.shapeSpawnWeight)
                {
                    SpawnRandomShape();
                    return;
                }
            }

            var type = GetWeightedEnemyType();
            if (SelectMonsterByType(type, out var monsterDef, out var pickedConfig))
            {
                SpawnMonster(monsterDef, null, pickedConfig);
                _waveCurrentTotalSpawn++;
            }
        }

        /// <summary>
        /// 生成随机形状
        /// </summary>
        private void SpawnRandomShape()
        {
            if (_waveManager == null || 
                _waveManager.ShapeDict == null || 
                _waveManager.ShapeDict.Count == 0)
            {
                Debug.LogWarning("[VampireLikeStrategy] ShapeLibraries is empty, falling back to single brick spawn.");
                SpawnRandomMonster();
                return;
            }

            // 随机选择这次形状的Cell个数
            var shapeCellCount = _waveManager.ShapeCellCount;
            var cellCount = shapeCellCount[_spawnRandom.Next(shapeCellCount.Count)];

            // 随机挑一个形状
            var shapeEntries = _waveManager.ShapeDict[cellCount];
            var selectedShape = shapeEntries[_spawnRandom.Next(shapeEntries.Count)];

            // 获取形状在世界中的生成位置（使用 edge-biased 逻辑寻找空位）
            Vector2Int emptyCell = default;
            if (_waveManager.GetEdgeBiasedRandomEmptyCell(out var randomEmptyCell, out var _))
            {
                var maxRetries = _waveConfig?.shapeSpawnMaxRetries ?? 20;
                var brickMgr = brickManager;
                var found = brickMgr?.FindEmptyCellForShape(
                    randomEmptyCell, 
                    selectedShape.bricks, 
                    out emptyCell, 
                    maxRetries) ?? false;
                    
                if (!found)
                {
                    Debug.Log($"[VampireLikeStrategy] Could not find empty spot for shape '{selectedShape.name}' after {maxRetries} retries.");
                    return;
                }

                // 生成砖块
                var spawnedBricks = new List<BrickTemplate>();
                var success = brickMgr?.acquireShape(
                    emptyCell, 
                    selectedShape.bricks, 
                    _waveConfig, 
                    ref spawnedBricks) ?? false;
                    
                if (!success || spawnedBricks.Count == 0)
                {
                    Debug.LogWarning($"[VampireLikeStrategy] acquireShape returned empty for shape '{selectedShape.name}'.");
                    return;
                }

                // 在每个砖块上生成怪物
                foreach (var template in spawnedBricks)
                {
                    var type = GetWeightedEnemyType();
                    if (SelectMonsterByType(type, out var monsterDef, out var pickedConfig))
                    {
                        SpawnMonster(monsterDef, template.position, pickedConfig);
                        _waveCurrentTotalSpawn++;
                    }
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            _waveMaxTotalSpawn = 0;
            _spawnIntervalOverride = 0f;
        }
    }
}

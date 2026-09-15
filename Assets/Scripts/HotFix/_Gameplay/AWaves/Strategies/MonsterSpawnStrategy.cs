using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace MoreMountains
{
    /// <summary>
    /// 刷怪策略类型
    /// </summary>
    public enum MonsterSpawnStrategyType
    {
        VampireLike,     // 幸存者玩法（类吸血鬼幸存者）
        BallXPitLike,    // Ball x Pit玩法（类球球你）
    }

    /// <summary>
    /// Ball x Pit 子刷怪策略类型
    /// </summary>
    public enum SubSpawnStrategyType
    {
        TopRow,          // 顶部行生成
        RandomRow,       // 随机行生成
        RandomCol,       // 随机列生成
        RandomEmpty,     // 随机空格子生成
    }

    /// <summary>
    /// 砖块尺寸配置
    /// </summary>
    [Serializable]
    public class BrickSizeConfig
    {
        [Tooltip("砖块尺寸 (宽, 高)")]
        public Vector2Int size = new(1, 1);

        [Tooltip("该尺寸的权重")]
        public float weight = 1f;
    }

    /// <summary>
    /// 刷怪策略接口 - 定义刷怪系统的核心行为
    /// </summary>
    public abstract class MonsterSpawnStrategy
    {
        protected WaveManager _waveManager;
        protected WaveConfig _waveConfig;
        protected WaveLevelConfig _levelConfig;
        protected Random _spawnRandom;
        protected float _spawnTimer;
        protected float _killSpeedTimer;
        protected int _killsInLastInterval;
        protected int _killsThisInterval;
        protected int _waveCurrentTotalSpawn;
        protected Dictionary<MonsterSpawnConfig, int> _spawnedCountByConfig = new();

        public virtual void Initialize(WaveManager waveManager, WaveConfig waveConfig, WaveLevelConfig levelConfig)
        {
            _waveManager = waveManager;
            _waveConfig = waveConfig;
            _levelConfig = levelConfig;
            _spawnRandom = new();

            _spawnTimer = 0f;
            _killSpeedTimer = 0f;
            _killsInLastInterval = 0;
            _killsThisInterval = 0;
            _waveCurrentTotalSpawn = 0;

            _spawnedCountByConfig.Clear();
            if (waveConfig?.availableMonsters != null)
            {
                foreach (var config in waveConfig.availableMonsters)
                {
                    if (config != null)
                        _spawnedCountByConfig[config] = 0;
                }
            }
        }

        /// <summary>
        /// 每帧更新刷怪逻辑
        /// </summary>
        /// <param name="dt">时间增量</param>
        /// <param name="activeMonsterCount">当前活跃怪物数量</param>
        public abstract void Update(float dt, int activeMonsterCount);

        /// <summary>
        /// 通知有怪物被击杀（用于击杀速度追踪）
        /// </summary>
        public virtual void NotifyMonsterKilled()
        {
            _killsThisInterval++;
        }

        /// <summary>
        /// 获取策略类型
        /// </summary>
        public abstract MonsterSpawnStrategyType GetStrategyType();

        /// <summary>
        /// 清理/重置策略状态
        /// </summary>
        public virtual void Reset()
        {
            _spawnTimer = 0f;
            _killSpeedTimer = 0f;
            _killsInLastInterval = 0;
            _killsThisInterval = 0;
            _waveCurrentTotalSpawn = 0;
            _spawnedCountByConfig.Clear();
        }

        /// <summary>
        /// 生成一个怪物
        /// </summary>
        protected Brick SpawnMonster(BrickDef monsterDef, Vector3? position = null, MonsterSpawnConfig originConfig = null)
        {
            if (_waveManager == null || _waveConfig == null)
                return null;

            return _waveManager.SpawnMonster(monsterDef, position, originConfig);
        }

        /// <summary>
        /// 获取智能生成位置
        /// </summary>
        protected bool GetSmartSpawnPosition(out Vector2 spawnPos)
        {
            if (_waveManager != null)
            {
                return _waveManager.GetSmartSpawnPosition(out spawnPos);
            }
            spawnPos = Vector2.zero;
            return false;
        }

        /// <summary>
        /// 获取随机敌人类型
        /// </summary>
        protected SpawnEnemyType GetWeightedEnemyType()
        {
            if (_waveManager != null)
            {
                return _waveManager.GetWeightedEnemyType();
            }
            return SpawnEnemyType.Normal;
        }

        /// <summary>
        /// 根据类型选择怪物
        /// </summary>
        protected bool SelectMonsterByType(SpawnEnemyType type, out BrickDef monsterDef, out MonsterSpawnConfig selectedConfig)
        {
            if (_waveManager != null)
            {
                return _waveManager.SelectMonsterByType(type, out monsterDef, out selectedConfig);
            }
            monsterDef = null;
            selectedConfig = null;
            return false;
        }

        /// <summary>
        /// 记录配置生成计数
        /// </summary>
        protected void RecordConfigSpawn(MonsterSpawnConfig config)
        {
            if (config == null)
                return;
            _spawnedCountByConfig.TryGetValue(config, out var current);
            _spawnedCountByConfig[config] = current + 1;
        }

        /// <summary>
        /// 检查配置配额是否达到
        /// </summary>
        protected bool IsConfigQuotaReached(MonsterSpawnConfig config)
        {
            if (config == null || config.atLeastSpawnCount <= 0)
                return false;
            if (_spawnedCountByConfig.TryGetValue(config, out var spawned))
                return spawned >= config.atLeastSpawnCount;
            return false;
        }

        /// <summary>
        /// 获取动态刷怪间隔
        /// </summary>
        protected float GetDynamicSpawnInterval()
        {
            if (_waveManager != null)
            {
                return _waveManager.GetDynamicSpawnInterval();
            }
            return _levelConfig?.globalBaseSpawnInterval ?? 2f;
        }
    }
}

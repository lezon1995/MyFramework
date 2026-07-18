using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 波次通关策略类型
    /// </summary>
    public enum WaveClearStrategy
    {
        SurviveUntilEnd, //坚持到关卡持续时间的最后
        DefeatAllMonsters, //击败关卡中所有的怪物
        DefeatBoss //击败关卡中的Boss怪（最后一关
    }

    /// <summary>
    /// 敌人类型
    /// </summary>
    public enum SpawnEnemyType
    {
        Normal, // 小怪
        Elite, // 精英怪
        Boss // Boss怪
    }

    /// <summary>
    /// 单个怪物的生成配置
    /// </summary>
    [Serializable]
    public class MonsterSpawnConfig
    {
        [Tooltip("怪物ID或预设路径")] public string monsterId;
        [Tooltip("怪物类型")] public SpawnEnemyType enemyType = SpawnEnemyType.Normal;
        [Tooltip("生成权重（数值越高越容易被选中）")] public float spawnWeight = 1f;
        [Tooltip("该类型怪物的基础生成间隔（秒），实际间隔会动态调整")] public float baseSpawnInterval = 3f;
        [Tooltip("该怪物是否在本波次强制生成一次")] public bool forceSpawnOnce;
    }

    /// <summary>
    /// 单个波次的配置
    /// </summary>
    [Serializable]
    public class WaveConfig
    {
        [Tooltip("波次编号（从1开始）")] public int waveNumber = 1;
        [Tooltip("波次名称（可选）")] public string waveName = "";
        [Tooltip("波次持续时间（秒），0表示无限")] public float duration = 60f;
        [Tooltip("通关策略")] public WaveClearStrategy clearStrategy;
        [Tooltip("该波次可能出现的怪物配置列表")] public List<MonsterSpawnConfig> availableMonsters = new();
        [Tooltip("该波次最大同时存活怪物数量")] public int maxActiveMonsters = 10;
        [Tooltip("该波次最小存活怪物数量（用于控制刷怪频率）")] public int minActiveMonsters = 3;

        [Header("Defeat All Strategy")]
        [Tooltip("击败所有怪物策略时的最大总生成数量，0表示使用默认值(maxActiveMonsters * 3)")]
        public int defeatAllMaxTotalSpawn;

        [Header("Monster Scaling")]
        [Tooltip("怪物属性随波次的增长倍率（血量）")] public float healthScaling = 1f;
        [Tooltip("怪物属性随波次的增长倍率（伤害）")] public float damageScaling = 1f;
        [Tooltip("怪物属性随波次的增长倍率（移动速度）")] public float speedScaling = 1f;
        [Tooltip("怪物属性随波次的增长倍率（防御）")] public float defenseScaling = 1f;

        [Header("Enemy Type Weights")]
        [Tooltip("小怪生成权重")] public float normalMonsterWeight = 70f;
        [Tooltip("精英怪生成权重")] public float eliteMonsterWeight = 25f;
        [Tooltip("Boss怪生成权重")] public float bossMonsterWeight = 5f;

        [Header("Boss Settings")]
        [Tooltip("Boss怪物ID（当clearStrategy为DefeatBoss时使用）")] public string bossMonsterId;
        [Tooltip("Boss出现的时间点（波次开始后的秒数）")] public float bossSpawnTime = 30f;

        [Header("Smart Spawning")]
        [Tooltip("是否允许使用智能刷怪（基于密度）")] public bool enableSmartSpawning = true;
        [Tooltip("智能刷怪时，怪物密集区域的判定半径")] public float denseRadius = 5f;
        [Tooltip("智能刷怪时，稀疏区域的判定半径内最少怪物数")] public int sparseThreshold = 1;

        [Header("Spawn Position")]
        [Tooltip("生成位置偏向边界的概率（0-1），值越大越偏向边界")] [Range(0f, 1f)]
        public float edgeBias = 0.8f;

        /// <summary>
        /// 获取击败所有怪物策略的最大生成总数
        /// </summary>
        public int GetDefeatAllMaxTotalSpawn()
        {
            if (defeatAllMaxTotalSpawn > 0)
                return defeatAllMaxTotalSpawn;
            return maxActiveMonsters * 3;
        }
    }

    /// <summary>
    /// 怪物属性增长数据
    /// </summary>
    [Serializable]
    public class MonsterScalingData
    {
        public float healthMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
        public float defenseMultiplier = 1f;

        public void ApplyWaveScaling(int currentWave, WaveConfig baseConfig, WaveLevelConfig levelConfig)
        {
            // 使用波次配置的增长倍率
            float waveMultiplier = currentWave;
            healthMultiplier = Mathf.Pow(baseConfig.healthScaling * levelConfig.globalHealthScalingPerWave, waveMultiplier - 1);
            damageMultiplier = Mathf.Pow(baseConfig.damageScaling * levelConfig.globalDamageScalingPerWave, waveMultiplier - 1);
            speedMultiplier = Mathf.Pow(baseConfig.speedScaling * levelConfig.globalSpeedScalingPerWave, waveMultiplier - 1);
            defenseMultiplier = Mathf.Pow(baseConfig.defenseScaling * levelConfig.globalDefenseScalingPerWave, waveMultiplier - 1);
        }

        public void Reset()
        {
            healthMultiplier = 1f;
            damageMultiplier = 1f;
            speedMultiplier = 1f;
            defenseMultiplier = 1f;
        }
    }
}
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
        [Tooltip("怪物ID或预设路径")]
        public BrickDef monsterDef;

        [Tooltip("生成权重（数值越高越容易被选中）")]
        public float spawnWeight = 1f;

        [Tooltip("该类型怪物的基础生成间隔（秒），实际间隔会动态调整")]
        public float baseSpawnInterval = 3f;

        [Tooltip("该怪物是否在本波次强制生成一次")]
        public bool forceSpawnOnce;

        [Tooltip("本波次该怪物至少生成个数（0 表示不限制）。在持续刷怪/有限刷怪模式下，仍会保证该配置累计生成达到该值之前不会被跳过。")]
        [Min(0)]
        public int atLeastSpawnCount;
    }

    /// <summary>
    /// 单个波次的配置
    /// </summary>
    [Serializable]
    public class WaveConfig
    {
        [Tooltip("波次编号（从1开始）")]
        public int waveNumber = 1;

        [Tooltip("波次名称（可选）")]
        public string waveName = "";

        [Tooltip("波次持续时间（秒），0表示无限")]
        public float duration = 60f;

        [Tooltip("通关策略")]
        public WaveClearStrategy clearStrategy;

        [Tooltip("该波次可能出现的怪物配置列表")]
        public List<MonsterSpawnConfig> availableMonsters = new();

        [Tooltip("该波次最大同时存活怪物数量")]
        public int maxActiveMonsters = 10;

        [Tooltip("该波次最小存活怪物数量（用于控制刷怪频率）")]
        public int minActiveMonsters = 3;

        [Header("Defeat All Strategy")]
        [Tooltip("击败所有怪物策略时的最大总生成数量，0表示使用默认值(maxActiveMonsters * 3)")]
        public int defeatAllMaxTotalSpawn;

        [Header("Monster Scaling")]
        [Tooltip("怪物属性随波次的增长倍率（血量）")]
        public float healthScaling = 1f;

        [Tooltip("怪物属性随波次的增长倍率（伤害）")]
        public float damageScaling = 1f;

        [Tooltip("怪物属性随波次的增长倍率（移动速度）")]
        public float speedScaling = 1f;

        [Tooltip("怪物属性随波次的增长倍率（防御）")]
        public float defenseScaling = 1f;

        [Header("Enemy Type Weights")]
        [Tooltip("小怪生成权重")]
        public float normalMonsterWeight = 70f;

        [Tooltip("精英怪生成权重")]
        public float eliteMonsterWeight = 25f;

        [Tooltip("Boss怪生成权重")]
        public float bossMonsterWeight = 5f;

        [Header("Boss Settings")]
        [Tooltip("Boss怪物ID（当clearStrategy为DefeatBoss时使用）")]
        public BrickDef bossMonsterId;

        [Tooltip("Boss出现的时间点（波次开始后的秒数）")]
        public float bossSpawnTime = 30f;

        [Header("Smart Spawning")]
        [Tooltip("是否允许使用智能刷怪（基于密度）")]
        public bool enableSmartSpawning = true;

        [Tooltip("智能刷怪时，怪物密集区域的判定半径")]
        public float denseRadius = 5f;

        [Tooltip("智能刷怪时，稀疏区域的判定半径内最少怪物数")]
        public int sparseThreshold = 1;

        [Header("Dynamic Continuous Spawning")]
        [Tooltip("是否启用持续刷怪模式（怪物死亡后立即补充）")]
        public bool enableContinuousSpawning = true;

        [Tooltip("持续刷怪时，地图覆盖率目标（0-1），怪物数量会据此动态调整")]
        [Range(0f, 1f)]
        public float targetCoverageRatio = 0.3f;

        [Tooltip("持续刷怪时，最小刷怪间隔（秒）")]
        public float minSpawnInterval = 0.3f;

        [Tooltip("持续刷怪时，最大刷怪间隔（秒）")]
        public float maxSpawnInterval = 2f;

        [Tooltip("持续刷怪时，根据击杀速度调整间隔的灵敏度（越大反应越快）")]
        public float killSpeedSensitivity = 2f;

        [Header("Spawn Position")]
        [Tooltip("生成位置偏向边界的概率（0-1），值越大越偏向边界")]
        [Range(0f, 1f)]
        public float edgeBiasProbability = 0.5f;

        [Tooltip("生成位置偏向边界的百分比（0-1），值越大越偏向边界")] 
        [Range(0f, 1f)]
        public float edgeBiasPercent = 0.7f;

        [Range(0f, 1f)]
        public float edgeBiasPercentAmplitude = 0.15f;

        [Header("Shape Spawning")]
        [Tooltip("是否启用形状生成（从 ShapesLibrary 中随机选取 ShapeEntry 生成砖块组合）")]
        public bool enableShapeSpawning;

        [Tooltip("形状生成权重（数值越高越容易被选中），设为0时只生成单个砖块")]
        [Range(0f, 100f)]
        public float shapeSpawnWeight = 30f;

        [Tooltip("形状生成时，最多尝试找到空置位置的次数")]
        [Range(1, 50)]
        public int shapeSpawnMaxRetries = 20;

        [Tooltip("形状生成后，是否在形状上随机放置怪物（取代单独刷怪）")]
        public bool spawnMonstersOnShape = true;

        public int KillCount { get; set; }
        public int SpawnCount { get; set; }

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
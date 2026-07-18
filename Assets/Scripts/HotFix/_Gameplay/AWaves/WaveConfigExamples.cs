/*
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 波次配置示例 - 展示如何创建波次关卡配置
    /// 创建方法：在Unity中右键 -> MoreMountains -> WaveLevelConfig
    /// </summary>
    public class WaveConfigExamples : MonoBehaviour
    {
        [Header("Quick Start Example")]
        [Tooltip("使用示例配置快速开始")]
        public bool useExampleConfig;

        [Header("Example Level Config")]
        [SerializeField] WaveLevelConfig exampleLevelConfig;

        void Start()
        {
            if (useExampleConfig)
            {
                CreateExampleLevelConfig();
            }
        }

        /// <summary>
        /// 创建一个示例关卡配置
        /// </summary>
        public static WaveLevelConfig CreateExampleLevelConfig()
        {
            var config = ScriptableObject.CreateInstance<WaveLevelConfig>();

            config.levelName = "Example Level";
            config.levelDescription = "A sample wave level with 5 waves";
            config.globalMaxActiveMonsters = 15;
            config.globalMinActiveMonsters = 3;
            config.globalBaseSpawnInterval = 2f;

            // 全局属性增长
            config.globalHealthScalingPerWave = 1.15f;
            config.globalDamageScalingPerWave = 1.12f;
            config.globalSpeedScalingPerWave = 1.05f;
            config.globalDefenseScalingPerWave = 1.10f;

            // 生成区域
            config.spawnAreaLeft = -10f;
            config.spawnAreaRight = 10f;
            config.spawnAreaTop = 8f;
            config.spawnAreaBottom = -5f;

            // 波次1：新手波次 - 纯小怪
            var wave1 = new WaveConfig
            {
                waveNumber = 1,
                waveName = "First Wave",
                duration = 45f,
                clearStrategy = WaveClearStrategy.SurviveUntilEnd,
                maxActiveMonsters = 5,
                minActiveMonsters = 2,
                normalMonsterWeight = 100f,
                eliteMonsterWeight = 0f,
                bossMonsterWeight = 0f,
                healthScaling = 1f,
                damageScaling = 1f,
                enableSmartSpawning = true,
                denseRadius = 4f,
                sparseThreshold = 1,
                availableMonsters = new()
                {
                    new() { monsterId = "Slime", enemyType = SpawnEnemyType.Normal, spawnWeight = 1f, baseSpawnInterval = 3f },
                    new() { monsterId = "Goblin", enemyType = SpawnEnemyType.Normal, spawnWeight = 0.8f, baseSpawnInterval = 3.5f }
                }
            };
            config.waves.Add(wave1);

            // 波次2：增加精英怪
            var wave2 = new WaveConfig
            {
                waveNumber = 2,
                waveName = "Elite Wave",
                duration = 60f,
                clearStrategy = WaveClearStrategy.SurviveUntilEnd,
                maxActiveMonsters = 8,
                minActiveMonsters = 3,
                normalMonsterWeight = 70f,
                eliteMonsterWeight = 30f,
                bossMonsterWeight = 0f,
                healthScaling = 1.1f,
                damageScaling = 1.1f,
                enableSmartSpawning = true,
                denseRadius = 5f,
                sparseThreshold = 1,
                availableMonsters = new()
                {
                    new() { monsterId = "Slime", enemyType = SpawnEnemyType.Normal, spawnWeight = 1f, baseSpawnInterval = 2.5f },
                    new() { monsterId = "Goblin", enemyType = SpawnEnemyType.Normal, spawnWeight = 1f, baseSpawnInterval = 2.5f },
                    new() { monsterId = "Orc", enemyType = SpawnEnemyType.Elite, spawnWeight = 0.5f, baseSpawnInterval = 5f }
                }
            };
            config.waves.Add(wave2);

            // 波次3：击败所有怪物
            var wave3 = new WaveConfig
            {
                waveNumber = 3,
                waveName = "Clear Them All",
                duration = 0f, // 无限时间
                clearStrategy = WaveClearStrategy.DefeatAllMonsters,
                maxActiveMonsters = 10,
                minActiveMonsters = 3,
                normalMonsterWeight = 60f,
                eliteMonsterWeight = 35f,
                bossMonsterWeight = 5f,
                healthScaling = 1.15f,
                damageScaling = 1.15f,
                enableSmartSpawning = true,
                denseRadius = 5f,
                sparseThreshold = 2,
                availableMonsters = new()
                {
                    new() { monsterId = "Slime", enemyType = SpawnEnemyType.Normal, spawnWeight = 1f, baseSpawnInterval = 2f },
                    new() { monsterId = "Goblin", enemyType = SpawnEnemyType.Normal, spawnWeight = 1f, baseSpawnInterval = 2f },
                    new() { monsterId = "Wolf", enemyType = SpawnEnemyType.Normal, spawnWeight = 0.7f, baseSpawnInterval = 2.5f },
                    new() { monsterId = "Orc", enemyType = SpawnEnemyType.Elite, spawnWeight = 0.5f, baseSpawnInterval = 4f, forceSpawnOnce = true },
                    new() { monsterId = "Troll", enemyType = SpawnEnemyType.Elite, spawnWeight = 0.3f, baseSpawnInterval = 6f }
                }
            };
            config.waves.Add(wave3);

            // 波次4：困难波次
            var wave4 = new WaveConfig
            {
                waveNumber = 4,
                waveName = "Survival Challenge",
                duration = 90f,
                clearStrategy = WaveClearStrategy.SurviveUntilEnd,
                maxActiveMonsters = 12,
                minActiveMonsters = 4,
                normalMonsterWeight = 50f,
                eliteMonsterWeight = 40f,
                bossMonsterWeight = 10f,
                healthScaling = 1.2f,
                damageScaling = 1.2f,
                speedScaling = 1.1f,
                defenseScaling = 1.15f,
                enableSmartSpawning = true,
                denseRadius = 6f,
                sparseThreshold = 2,
                availableMonsters = new()
                {
                    new() { monsterId = "Slime", enemyType = SpawnEnemyType.Normal, spawnWeight = 1f, baseSpawnInterval = 1.8f },
                    new() { monsterId = "Goblin", enemyType = SpawnEnemyType.Normal, spawnWeight = 1f, baseSpawnInterval = 1.8f },
                    new() { monsterId = "Wolf", enemyType = SpawnEnemyType.Normal, spawnWeight = 0.8f, baseSpawnInterval = 2f },
                    new() { monsterId = "Orc", enemyType = SpawnEnemyType.Elite, spawnWeight = 0.6f, baseSpawnInterval = 4f },
                    new() { monsterId = "Troll", enemyType = SpawnEnemyType.Elite, spawnWeight = 0.4f, baseSpawnInterval = 5f },
                    new() { monsterId = "Demon", enemyType = SpawnEnemyType.Elite, spawnWeight = 0.3f, baseSpawnInterval = 6f, forceSpawnOnce = true }
                }
            };
            config.waves.Add(wave4);

            // 波次5：最终Boss波次
            var wave5 = new WaveConfig
            {
                waveNumber = 5,
                waveName = "Final Boss",
                duration = 0f,
                clearStrategy = WaveClearStrategy.DefeatBoss,
                maxActiveMonsters = 8,
                minActiveMonsters = 2,
                normalMonsterWeight = 40f,
                eliteMonsterWeight = 30f,
                bossMonsterWeight = 30f,
                healthScaling = 1.25f,
                damageScaling = 1.25f,
                speedScaling = 1.15f,
                defenseScaling = 1.2f,
                bossMonsterId = "DragonBoss",
                bossSpawnTime = 20f,
                enableSmartSpawning = true,
                denseRadius = 5f,
                sparseThreshold = 1,
                availableMonsters = new()
                {
                    new()  { monsterId = "Demon", enemyType = SpawnEnemyType.Normal, spawnWeight = 1f, baseSpawnInterval = 2f },
                    new()  { monsterId = "Troll", enemyType = SpawnEnemyType.Elite, spawnWeight = 0.5f, baseSpawnInterval = 4f },
                    new()  { monsterId = "DemonLord", enemyType = SpawnEnemyType.Elite, spawnWeight = 0.3f, baseSpawnInterval = 5f }
                }
            };
            config.waves.Add(wave5);

            Debug.Log($"[WaveConfigExamples] Created example level config with {config.waves.Count} waves");
            return config;
        }

        /// <summary>
        /// 开始示例游戏
        /// </summary>
        public void StartExampleGame()
        {
            var config = CreateExampleLevelConfig();
            if (WaveGameMode.Instance)
            {
                WaveGameMode.Instance.StartGame(config);
            }
            else
            {
                var bootstrap = FindObjectOfType<WaveSystemBootstrap>();
                if (bootstrap)
                {
                    bootstrap.StartWaveGame(config);
                }
                else
                {
                    Debug.LogError("[WaveConfigExamples] No WaveSystemBootstrap found in scene!");
                }
            }
        }
    }
}
*/

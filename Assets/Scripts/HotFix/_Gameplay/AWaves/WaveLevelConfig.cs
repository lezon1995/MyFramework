using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 关卡配置（包含多个波次）
    /// </summary>
    [CreateAssetMenu(fileName = "WaveLevelConfig", menuName = "MoreMountains/WaveLevelConfig", order = 1)]
    public class WaveLevelConfig : ScriptableObject
    {
        [Tooltip("关卡名称")] public string levelName = "Default Level";
        [Tooltip("关卡描述")] public string levelDescription = "";
        [Tooltip("波次配置列表")] public List<WaveConfig> waves = new();
        [Tooltip("全局最大同时存活怪物数量上限")] public int globalMaxActiveMonsters = 20;
        [Tooltip("全局最小存活怪物数量下限")] public int globalMinActiveMonsters = 5;
        [Tooltip("全局基础刷怪间隔（秒）")] public float globalBaseSpawnInterval = 2f;
        [Tooltip("怪物属性基础增长倍率（每波次）")] public float globalHealthScalingPerWave = 1.15f;
        [Tooltip("怪物伤害基础增长倍率（每波次）")] public float globalDamageScalingPerWave = 1.12f;
        [Tooltip("怪物速度基础增长倍率（每波次）")] public float globalSpeedScalingPerWave = 1.05f;
        [Tooltip("怪物防御基础增长倍率（每波次）")] public float globalDefenseScalingPerWave = 1.10f;
        [Tooltip("怪物生成区域边界（左）")] public float spawnAreaLeft = -10f;
        [Tooltip("怪物生成区域边界（右）")] public float spawnAreaRight = 10f;
        [Tooltip("怪物生成区域边界（上）")] public float spawnAreaTop = 10f;
        [Tooltip("怪物生成区域边界（下）")] public float spawnAreaBottom = -10f;
        [Tooltip("持续刷怪时，地图覆盖率目标（0-1），怪物数量会据此动态调整")]
        [Range(0f, 1f)]
        public float globalTargetCoverageRatio = 0.3f;
        [Tooltip("持续刷怪时，全局最小刷怪间隔（秒）")] public float globalMinSpawnInterval = 0.3f;
        [Tooltip("持续刷怪时，全局最大刷怪间隔（秒）")] public float globalMaxSpawnInterval = 2f;

        /// <summary>
        /// 获取指定波次的配置
        /// </summary>
        public WaveConfig GetWaveConfig(int waveNumber)
        {
            if (waveNumber <= 0 || waveNumber > waves.Count)
            {
                // 如果请求的波次超出范围，返回最后一个波次的配置（无尽模式）
                if (waves.Count > 0)
                {
                    return waves[^1];
                }

                return null;
            }

            return waves[waveNumber - 1];
        }

        /// <summary>
        /// 获取总波次数量
        /// </summary>
        public int MaxWave => waves.Count;

        /// <summary>
        /// 是否是最后一波
        /// </summary>
        public bool IsLastWave(int waveNumber)
        {
            return waveNumber >= waves.Count;
        }

        /// <summary>
        /// 获取最终的Boss波次配置
        /// </summary>
        public WaveConfig GetFinalBossWave()
        {
            if (waves.Count > 0)
            {
                return waves[^1];
            }

            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 刷怪策略使用示例
    /// 展示如何在 WaveManager 中集成刷怪策略系统
    /// </summary>
    public class MonsterSpawnStrategyExample : MonoBehaviour
    {
        [Header("策略配置")]
        [Tooltip("当前使用的刷怪策略类型")]
        public MonsterSpawnStrategyType strategyType = MonsterSpawnStrategyType.VampireLike;

        [Tooltip("Ball x Pit 子策略类型")]
        public SubSpawnStrategyType subStrategyType = SubSpawnStrategyType.TopRow;

        [Tooltip("WaveManager 引用")]
        public WaveManager waveManager;

        [Tooltip("当前波次配置")]
        public WaveConfig curWaveConfig;

        [Tooltip("当前关卡配置")]
        public WaveLevelConfig curLevelConfig;

        // 策略管理器
        private MonsterSpawnStrategyManager _strategyManager;

        private void Awake()
        {
            // 初始化策略管理器
            _strategyManager = new MonsterSpawnStrategyManager();
            _strategyManager.Initialize(waveManager, curWaveConfig, curLevelConfig);
            _strategyManager.SetStrategy(strategyType);

            // 如果是 Ball x Pit 玩法，设置子策略
            if (strategyType == MonsterSpawnStrategyType.BallXPitLike)
            {
                _strategyManager.SetSubStrategy(subStrategyType);
            }
        }

        private void Update()
        {
            if (waveManager == null || !waveManager.IsPlaying)
                return;

            // 更新策略
            _strategyManager.Update(
                Time.deltaTime,
                waveManager.ActiveMonsterCount
            );
        }

        /// <summary>
        /// 切换刷怪策略类型
        /// </summary>
        public void SwitchStrategy(MonsterSpawnStrategyType newType)
        {
            strategyType = newType;
            _strategyManager?.SetStrategy(newType);

            // 切换到 Ball x Pit 时应用子策略
            if (newType == MonsterSpawnStrategyType.BallXPitLike)
            {
                _strategyManager.SetSubStrategy(subStrategyType);
            }
        }

        /// <summary>
        /// 切换 Ball x Pit 子策略
        /// </summary>
        public void SwitchSubStrategy(SubSpawnStrategyType newSubType)
        {
            subStrategyType = newSubType;
            _strategyManager?.SetSubStrategy(newSubType);
        }

        /// <summary>
        /// 当怪物被击杀时调用
        /// </summary>
        public void OnMonsterKilled()
        {
            _strategyManager?.NotifyMonsterKilled();
        }

        /// <summary>
        /// 配置 Ball x Pit 玩法
        /// </summary>
        public void ConfigureBallXPitLike(
            float spawnInterval = 2f,
            int spawnCountPerBatch = 3,
            int maxActiveBricks = 20,
            bool allowMultiCellBricks = true)
        {
            _strategyManager?.UpdateBallXPitLikeConfig(strategy =>
            {
                strategy.spawnInterval = spawnInterval;
                strategy.spawnCountPerBatch = spawnCountPerBatch;
                strategy.maxActiveBricks = maxActiveBricks;
                strategy.allowMultiCellBricks = allowMultiCellBricks;
            });
        }
    }
}

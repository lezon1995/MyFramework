using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 刷怪策略工厂类
    /// 用于创建和管理不同类型的刷怪策略
    /// </summary>
    public static class MonsterSpawnStrategyFactory
    {
        /// <summary>
        /// 根据策略类型创建刷怪策略实例
        /// </summary>
        public static MonsterSpawnStrategy CreateStrategy(MonsterSpawnStrategyType strategyType)
        {
            return strategyType switch
            {
                MonsterSpawnStrategyType.VampireLike => new VampireLikeMonsterSpawnStrategy(),
                MonsterSpawnStrategyType.BallXPitLike => new BallXPitLikeMonsterSpawnStrategy(),
                _ => null
            };
        }

        /// <summary>
        /// 创建并初始化刷怪策略
        /// </summary>
        public static MonsterSpawnStrategy CreateAndInitialize(
            MonsterSpawnStrategyType strategyType,
            WaveManager waveManager,
            WaveConfig waveConfig,
            WaveLevelConfig levelConfig)
        {
            var strategy = CreateStrategy(strategyType);
            if (strategy != null)
            {
                strategy.Initialize(waveManager, waveConfig, levelConfig);
            }
            return strategy;
        }

        /// <summary>
        /// 根据策略类型名称创建策略
        /// </summary>
        public static MonsterSpawnStrategy CreateStrategyByName(string strategyTypeName)
        {
            if (Enum.TryParse<MonsterSpawnStrategyType>(strategyTypeName, true, out var type))
            {
                return CreateStrategy(type);
            }
            return null;
        }

        /// <summary>
        /// 获取策略类型的显示名称
        /// </summary>
        public static string GetStrategyDisplayName(MonsterSpawnStrategyType strategyType)
        {
            return strategyType switch
            {
                MonsterSpawnStrategyType.VampireLike => "幸存者玩法 (VampireLike)",
                MonsterSpawnStrategyType.BallXPitLike => "球球你玩法 (BallXPitLike)",
                _ => strategyType.ToString()
            };
        }

        /// <summary>
        /// 获取子策略类型的显示名称
        /// </summary>
        public static string GetSubStrategyDisplayName(SubSpawnStrategyType strategyType)
        {
            return strategyType switch
            {
                SubSpawnStrategyType.TopRow => "顶部行生成 (TopRow)",
                SubSpawnStrategyType.RandomRow => "随机行生成 (RandomRow)",
                SubSpawnStrategyType.RandomCol => "随机列生成 (RandomCol)",
                SubSpawnStrategyType.RandomEmpty => "随机空位生成 (RandomEmpty)",
                _ => strategyType.ToString()
            };
        }
    }

    /// <summary>
    /// 刷怪策略管理器
    /// 负责管理当前激活的刷怪策略和策略切换
    /// </summary>
    public class MonsterSpawnStrategyManager
    {
        private MonsterSpawnStrategy _currentStrategy;
        private MonsterSpawnStrategyType _currentStrategyType;
        private WaveManager _waveManager;
        private WaveConfig _waveConfig;
        private WaveLevelConfig _levelConfig;

        /// <summary>
        /// 当前激活的策略
        /// </summary>
        public MonsterSpawnStrategy CurrentStrategy => _currentStrategy;

        /// <summary>
        /// 当前策略类型
        /// </summary>
        public MonsterSpawnStrategyType CurrentStrategyType => _currentStrategyType;

        /// <summary>
        /// 初始化策略管理器
        /// </summary>
        public void Initialize(WaveManager waveManager, WaveConfig waveConfig, WaveLevelConfig levelConfig)
        {
            _waveManager = waveManager;
            _waveConfig = waveConfig;
            _levelConfig = levelConfig;
        }

        /// <summary>
        /// 设置当前使用的刷怪策略
        /// </summary>
        public void SetStrategy(MonsterSpawnStrategyType strategyType)
        {
            if (_currentStrategyType == strategyType && _currentStrategy != null)
                return;

            // 清理旧策略
            _currentStrategy?.Reset();

            // 创建新策略
            _currentStrategy = MonsterSpawnStrategyFactory.CreateAndInitialize(
                strategyType,
                _waveManager,
                _waveConfig,
                _levelConfig);

            _currentStrategyType = strategyType;
        }

        /// <summary>
        /// 更新当前策略
        /// </summary>
        public void Update(float dt, int activeMonsterCount)
        {
            _currentStrategy?.Update(dt, activeMonsterCount);
        }

        /// <summary>
        /// 通知怪物被击杀
        /// </summary>
        public void NotifyMonsterKilled()
        {
            _currentStrategy?.NotifyMonsterKilled();
        }

        /// <summary>
        /// 重置策略
        /// </summary>
        public void Reset()
        {
            _currentStrategy?.Reset();
        }

        /// <summary>
        /// 更新策略配置（仅对BallXPitLike策略有效）
        /// </summary>
        public void UpdateBallXPitLikeConfig(Action<BallXPitLikeMonsterSpawnStrategy> configAction)
        {
            if (_currentStrategy is BallXPitLikeMonsterSpawnStrategy ballXPitStrategy)
            {
                configAction?.Invoke(ballXPitStrategy);
            }
        }

        /// <summary>
        /// 设置子策略（仅对BallXPitLike策略有效）
        /// </summary>
        public void SetSubStrategy(SubSpawnStrategyType subStrategyType)
        {
            if (_currentStrategy is BallXPitLikeMonsterSpawnStrategy ballXPitStrategy)
            {
                ballXPitStrategy.SetCurrentStrategy(subStrategyType);
            }
        }
    }
}

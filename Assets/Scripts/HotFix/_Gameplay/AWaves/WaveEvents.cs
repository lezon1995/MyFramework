using UnityEngine;

namespace MoreMountains
{
    #region Wave Events

    /// <summary>
    /// 波次开始事件
    /// </summary>
    public struct OnWaveStart
    {
        public int WaveNumber;
        public WaveConfig Config;
        public float Duration;

        public OnWaveStart(WaveConfig config)
        {
            WaveNumber = config.waveNumber;
            Config = config;
            Duration = config.duration;
        }
    }

    /// <summary>
    /// 波次完成事件
    /// </summary>
    public struct OnWaveComplete
    {
        public WaveConfig Config;
        public int WaveNumber;
        public int KillCount;
        public int SpawnCount;
        public float ElapsedTime;

        public OnWaveComplete(WaveConfig config)
        {
            Config = config;
            WaveNumber = config.waveNumber;
            KillCount = config.KillCount;
            SpawnCount = config.SpawnCount;
            ElapsedTime = config.duration;
        }
    }

    /// <summary>
    /// 波次失败事件
    /// </summary>
    public struct OnWaveFailed
    {
        public int WaveNumber;
        public WaveConfig Config;
        public GameResult Result;
        public string Reason;

        public OnWaveFailed(WaveConfig config, GameResult result, string reason = "")
        {
            Config = config;
            WaveNumber = config.waveNumber;
            Result = result;
            Reason = reason;
        }
    }

    /// <summary>
    /// 关卡开始事件
    /// </summary>
    public struct OnLevelStart
    {
        public WaveLevelConfig Config;
        public int TotalWaves;

        public OnLevelStart(WaveLevelConfig config)
        {
            Config = config;
            TotalWaves = config?.MaxWave ?? 0;
        }
    }

    /// <summary>
    /// 关卡完成事件
    /// </summary>
    public struct OnLevelComplete
    {
        public WaveLevelConfig Config;
        public int CompletedWaves;
        public int TotalKills;
        public GameResult Result;

        public OnLevelComplete(WaveLevelConfig config, int completedWaves, int totalKills, GameResult result)
        {
            Config = config;
            CompletedWaves = completedWaves;
            TotalKills = totalKills;
            Result = result;
        }
    }

    /// <summary>
    /// 游戏结束事件
    /// </summary>
    public struct OnGameEnd
    {
        public GameResult Result;
        public int TotalWavesCompleted;
        public int TotalKills;
        public float PlayTime;

        public OnGameEnd(GameResult result, int totalWaves, int totalKills, float playTime)
        {
            Result = result;
            TotalWavesCompleted = totalWaves;
            TotalKills = totalKills;
            PlayTime = playTime;
        }
    }

    #endregion

    #region Monster Spawn Events

    /// <summary>
    /// 怪物生成事件
    /// </summary>
    public struct OnMonsterSpawned
    {
        public Brick Monster;
        public SpawnEnemyType Type;
        public Vector3 Position;
        public int WaveNumber;

        public OnMonsterSpawned(Brick monster, SpawnEnemyType type, Vector3 position, int waveNumber)
        {
            Monster = monster;
            Type = type;
            Position = position;
            WaveNumber = waveNumber;
        }
    }

    /// <summary>
    /// 怪物死亡事件（波次系统追踪）
    /// </summary>
    public struct OnMonsterKilled_Wave
    {
        public Brick Monster;
        public SpawnEnemyType Type;
        public int WaveNumber;
        public float TimeSinceWaveStart;

        public OnMonsterKilled_Wave(Brick monster, SpawnEnemyType type, int waveNumber, float elapsedTime)
        {
            Monster = monster;
            Type = type;
            WaveNumber = waveNumber;
            TimeSinceWaveStart = elapsedTime;
        }
    }

    /// <summary>
    /// Boss生成事件
    /// </summary>
    public struct OnBossSpawned
    {
        public Brick Boss;
        public int WaveNumber;
        public string BossId;

        public OnBossSpawned(Brick boss, int waveNumber, string bossId)
        {
            Boss = boss;
            WaveNumber = waveNumber;
            BossId = bossId;
        }
    }

    /// <summary>
    /// Boss死亡事件
    /// </summary>
    public struct OnBossDefeated
    {
        public Brick Boss;
        public int WaveNumber;

        public OnBossDefeated(Brick boss, int waveNumber)
        {
            Boss = boss;
            WaveNumber = waveNumber;
        }
    }

    #endregion

    #region State Events

    /// <summary>
    /// 波次状态改变事件
    /// </summary>
    public struct OnWaveStateChanged
    {
        public WaveState OldState;
        public WaveState NewState;
        public int WaveNumber;

        public OnWaveStateChanged(WaveState oldState, WaveState newState, int waveNumber)
        {
            OldState = oldState;
            NewState = newState;
            WaveNumber = waveNumber;
        }
    }

    /// <summary>
    /// 波次时间更新事件
    /// </summary>
    public struct OnWaveTimeUpdate
    {
        public float TimeRemaining;
        public float TotalTime;
        public float Progress; // 0-1

        public OnWaveTimeUpdate(float timeRemaining, float totalTime)
        {
            TimeRemaining = timeRemaining;
            TotalTime = totalTime;
            Progress = totalTime > 0 ? 1f - (timeRemaining / totalTime) : 0f;
        }
    }

    #endregion
}

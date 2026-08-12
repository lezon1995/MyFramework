using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    // 波次游戏模式 - 整合所有波次系统的主控制器
    public class WaveGameMode : MonoBehaviour
    {
        public WaveManager waveManager;
        public WaveRewardManager waveRewardManager;

        #region Properties
        // 当前关卡配置
        public WaveLevelConfig CurrentLevelConfig { get; set; }

        // 是否正在进行波次游戏
        public bool IsPlaying => waveManager?.IsPlaying ?? false;

        // 当前波次编号
        public int CurWave => waveManager?.WaveNumber ?? 0;

        // 总波次数
        public int MaxWave => CurrentLevelConfig?.MaxWave ?? 0;

        // 游戏总时长
        public float TotalPlayTime { get; set; }

        // 总击杀数
        public int TotalKills { get; set; }

        public WaveState CurWaveState => waveManager.State;
        #endregion

        #region Events
        public event Action OnGameStarted;
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action OnRewardPhaseStarted;
        public event Action OnRewardPhaseEnded;
        #endregion

        void Awake()
        {
            RegisterEventListeners();
        }

        void RegisterEventListeners()
        {
            // 注册波次管理器事件
            if (waveManager)
            {
                waveManager.OnWaveStart += HandleWaveStart;
                waveManager.OnWaveComplete += HandleWaveComplete;
                waveManager.OnWaveFailed += HandleWaveFailed;
                waveManager.OnLevelStart += HandleLevelStart;
                waveManager.OnLevelComplete += HandleLevelComplete;
                waveManager.OnGameEnd += HandleGameEnd;
                waveManager.OnMonsterSpawned += HandleMonsterSpawned;
                waveManager.OnMonsterKilled += HandleMonsterKilled;
                waveManager.OnStateChanged += HandleStateChanged;

                // 问题1修复：监听奖励选择相关事件
                waveManager.OnRewardSelectionStarted += HandleRewardSelectionStarted;
                waveManager.OnRewardSelectionEnded += HandleRewardSelectionEnded;
            }

            // 注册奖励管理器事件
            if (waveRewardManager)
            {
                waveRewardManager.OnRewardSelectionComplete += HandleRewardManagerSelectionComplete;
            }
        }

        void UnregisterEventListeners()
        {
            if (waveManager)
            {
                waveManager.OnWaveStart -= HandleWaveStart;
                waveManager.OnWaveComplete -= HandleWaveComplete;
                waveManager.OnWaveFailed -= HandleWaveFailed;
                waveManager.OnLevelStart -= HandleLevelStart;
                waveManager.OnLevelComplete -= HandleLevelComplete;
                waveManager.OnGameEnd -= HandleGameEnd;
                waveManager.OnMonsterSpawned -= HandleMonsterSpawned;
                waveManager.OnMonsterKilled -= HandleMonsterKilled;
                waveManager.OnStateChanged -= HandleStateChanged;

                // 取消监听奖励选择相关事件
                waveManager.OnRewardSelectionStarted -= HandleRewardSelectionStarted;
                waveManager.OnRewardSelectionEnded -= HandleRewardSelectionEnded;
            }

            if (waveRewardManager)
            {
                waveRewardManager.OnRewardSelectionComplete -= HandleRewardManagerSelectionComplete;
            }
        }

        public void Update()
        {
            var elapsedTime = Time.deltaTime;

            if (IsPlaying)
            {
                TotalPlayTime += elapsedTime;
            }
        }

        #region Public Methods

        // 开始一个新的波次游戏
        public void StartGame(WaveLevelConfig levelConfig)
        {
            if (levelConfig == null)
            {
                Debug.LogError("[WaveGameMode] Cannot start game: level config is null!");
                return;
            }

            CurrentLevelConfig = levelConfig;
            TotalPlayTime = 0f;
            TotalKills = 0;

            // 初始化并启动波次管理器
            waveManager.StartLevel(levelConfig);

            OnGameStarted?.Invoke();

            Debug.Log($"[WaveGameMode] Game started with level: {levelConfig.levelName}");
        }

        public void StartNextWave()
        {
            waveManager.StartNextWave();
        }

        // 暂停游戏
        public void PauseGame()
        {
            if (!IsPlaying) 
                return;

            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
            Debug.Log("[WaveGameMode] Game paused");
        }

        // 恢复游戏
        public void ResumeGame()
        {
            Time.timeScale = 1f;
            OnGameResumed?.Invoke();
            Debug.Log("[WaveGameMode] Game resumed");
        }

        // 退出游戏
        public void QuitGame()
        {
            waveManager.Reset();
            waveRewardManager.EndRewardSelection();
            TotalPlayTime = 0f;
            TotalKills = 0;
            Time.timeScale = 1f;

            Debug.Log("[WaveGameMode] Game quit");
        }

        /// <summary>
        /// 进入奖励选择阶段 - 由UI或外部调用
        /// </summary>
        public void EnterRewardPhase()
        {
            if (waveManager == null || !waveManager.IsInRewardSelection)
            {
                Debug.LogWarning("[WaveGameMode] Cannot enter reward phase: not in reward selection state.");
                return;
            }

            OnRewardPhaseStarted?.Invoke();

            // 生成奖励选项
            int waveNumber = CurWave;
            waveRewardManager.GenerateRewards(waveNumber, 3);

            Debug.Log($"[WaveGameMode] Showing reward options for wave {waveNumber}");
        }

        /// <summary>
        /// 确认奖励选择并离开奖励阶段 - 由UI调用
        /// </summary>
        public void ConfirmRewardSelection()
        {
            if (waveManager == null || !waveManager.IsInRewardSelection)
            {
                Debug.LogWarning("[WaveGameMode] Cannot confirm reward: not in reward selection state.");
                return;
            }

            // 完成奖励选择
            waveRewardManager.EndRewardSelection();

            // 通知WaveManager离开奖励选择阶段，开始下一波
            waveManager.ExitRewardSelection();
        }

        /// <summary>
        /// 跳过奖励并继续 - 由UI调用
        /// </summary>
        public void SkipRewardAndContinue()
        {
            if (waveManager == null || !waveManager.IsInRewardSelection)
            {
                Debug.LogWarning("[WaveGameMode] Cannot skip reward: not in reward selection state.");
                return;
            }

            // 跳过奖励选择
            waveRewardManager.SkipRewardSelection();

            // 通知WaveManager离开奖励选择阶段，开始下一波
            waveManager.ExitRewardSelection();
        }

        // 获取游戏进度（0-1）
        public float GetProgress()
        {
            if (CurrentLevelConfig == null || CurrentLevelConfig.MaxWave == 0)
                return 0f;

            return (float)CurWave / CurrentLevelConfig.MaxWave;
        }

        // 获取当前波次进度（0-1）
        public float GetCurrentWaveProgress()
        {
            if (waveManager?.CurWave == null)
                return 0f;

            if (waveManager.CurWave.duration <= 0)
                return 0f;

            return 1f - (waveManager.WaveTimeRemaining / waveManager.CurWave.duration);
        }

        #endregion

        #region Event Handlers

        void HandleWaveStart(WaveConfig config)
        {
            // 触发UI更新
            new OnWaveStart(config).trigger();

            // 播放波次开始音效/特效
            PlayWaveStartEffects(config);
        }

        void HandleWaveComplete(WaveConfig config)
        {
            // WaveManager已经进入RewardSelecting状态
            // 这里生成奖励选项让玩家选择
            EnterRewardPhase();
        }

        void HandleWaveFailed(WaveConfig config, GameResult result)
        {
            Debug.Log($"[WaveGameMode] Wave failed: {result}");
            // 可以在这里显示失败UI
        }

        void HandleLevelStart(WaveLevelConfig config)
        {
            new OnLevelStart(config).trigger();
        }

        void HandleLevelComplete(WaveLevelConfig config)
        {
            new OnLevelComplete(config, CurWave, TotalKills, GameResult.Victory).trigger();
        }

        void HandleGameEnd(GameResult result)
        {
            var gameEndEvent = new OnGameEnd(result, CurWave, TotalKills, TotalPlayTime);
            gameEndEvent.trigger();

            switch (result)
            {
                case GameResult.Victory:
                    HandleVictory();
                    break;
                case GameResult.Defeat:
                    HandleDefeat();
                    break;
            }
        }

        void HandleMonsterSpawned(Brick monster)
        {
            if (monster && waveManager)
            {
                new OnMonsterSpawned(
                    monster,
                    GetSpawnType(monster),
                    monster.getWorldPosition(),
                    waveManager.WaveNumber
                ).trigger();
            }
        }

        void HandleMonsterKilled(Brick monster)
        {
            if (monster)
            {
                TotalKills++;
                if (waveManager)
                {
                    new OnMonsterKilled_Wave(
                        monster,
                        GetSpawnType(monster),
                        waveManager.WaveNumber,
                        waveManager.WaveTimeElapsed
                    ).trigger();
                }
            }
        }

        void HandleStateChanged(WaveState state)
        {
            if (waveManager)
            {
                new OnWaveStateChanged(WaveState.Idle, state, waveManager.WaveNumber).trigger();
            }
        }

        // 问题1修复：处理奖励选择开始
        void HandleRewardSelectionStarted()
        {
            Debug.Log("[WaveGameMode] Reward selection started, waiting for player...");
            // UI应该在这里显示奖励选择界面
        }

        // 问题1修复：处理奖励选择结束
        void HandleRewardSelectionEnded()
        {
            OnRewardPhaseEnded?.Invoke();
            Debug.Log("[WaveGameMode] Reward selection ended.");
        }

        // 处理奖励管理器完成选择
        void HandleRewardManagerSelectionComplete()
        {
            Debug.Log("[WaveGameMode] Reward manager selection complete.");
        }

        #endregion

        #region Private Methods

        SpawnEnemyType GetSpawnType(Brick monster)
        {
            if (monster == null) 
                return SpawnEnemyType.Normal;

            return monster.type switch
            {
                EnemyType.NORMAL => SpawnEnemyType.Normal,
                EnemyType.ELITE => SpawnEnemyType.Elite,
                EnemyType.BOSS => SpawnEnemyType.Boss,
                _ => SpawnEnemyType.Normal
            };
        }

        void PlayWaveStartEffects(WaveConfig config)
        {
            // TODO: 播放波次开始音效和特效
            // 可以根据波次类型播放不同的效果
        }

        void HandleVictory()
        {
            Debug.Log("[WaveGameMode] Victory!");
            // 显示胜利UI
            // 播放胜利音效
        }

        void HandleDefeat()
        {
            Debug.Log("[WaveGameMode] Defeat!");
            // 显示失败UI
            // 播放失败音效
        }

        #endregion

        public void OnDestroy()
        {
            UnregisterEventListeners();
        }
    }
}

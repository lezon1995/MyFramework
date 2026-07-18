using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoreMountains
{
    /// <summary>
    /// 波次UI组件 - 用于在游戏中显示波次相关信息
    /// </summary>
    public class WaveUI : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI waveNumberText;
        public TextMeshProUGUI waveTimeText;
        public TextMeshProUGUI monsterCountText;
        public Slider waveProgressSlider;
        public GameObject bossWarningPanel;
        public TextMeshProUGUI bossWarningText;

        [Header("Wave Info Panel")]
        public GameObject waveInfoPanel;
        public TextMeshProUGUI waveNameText;
        public TextMeshProUGUI waveStrategyText;

        [Header("Settings")]
        public float bossWarningTime = 5f;
        public bool showDebugInfo;
        public WaveManager  waveManager;

        void Start()
        {
            RegisterEventListeners();
            InitializeUI();
        }

        void OnDestroy()
        {
            UnregisterEventListeners();
        }

        void Update()
        {
            UpdateWaveInfo();
            UpdateBossWarning();
        }

        void RegisterEventListeners()
        {
            if (waveManager != null)
            {
                waveManager.OnWaveStart += OnWaveStart;
                waveManager.OnWaveComplete += OnWaveComplete;
                waveManager.OnWaveFailed += OnWaveFailed;
                waveManager.OnLevelStart += OnLevelStart;
                waveManager.OnLevelComplete += OnLevelComplete;
                waveManager.OnGameEnd += OnGameEnd;
                waveManager.OnStateChanged += OnStateChanged;
                waveManager.OnWaveTimeUpdate += OnWaveTimeUpdate;
            }
        }

        void UnregisterEventListeners()
        {
            if (waveManager != null)
            {
                waveManager.OnWaveStart -= OnWaveStart;
                waveManager.OnWaveComplete -= OnWaveComplete;
                waveManager.OnWaveFailed -= OnWaveFailed;
                waveManager.OnLevelStart -= OnLevelStart;
                waveManager.OnLevelComplete -= OnLevelComplete;
                waveManager.OnGameEnd -= OnGameEnd;
                waveManager.OnStateChanged -= OnStateChanged;
                waveManager.OnWaveTimeUpdate -= OnWaveTimeUpdate;
            }
        }

        void InitializeUI()
        {
            if (bossWarningPanel != null)
                bossWarningPanel.SetActive(false);

            if (waveInfoPanel != null)
                waveInfoPanel.SetActive(false);
        }

        #region Event Handlers

        void OnWaveStart(WaveConfig config)
        {
            if (waveInfoPanel != null)
                waveInfoPanel.SetActive(true);

            if (waveNameText != null)
                waveNameText.text = config.waveName ?? $"Wave {config.waveNumber}";

            if (waveStrategyText != null)
                waveStrategyText.text = GetStrategyText(config.clearStrategy);

            if (waveNumberText != null)
                waveNumberText.text = $"Wave {config.waveNumber}";

            UpdateWaveProgress();
        }

        void OnWaveComplete(WaveConfig config)
        {
            if (waveInfoPanel != null)
                waveInfoPanel.SetActive(false);

            if (bossWarningPanel != null)
                bossWarningPanel.SetActive(false);
        }

        void OnWaveFailed(WaveConfig config, GameResult result)
        {
            Debug.Log($"Wave Failed: {result}");
        }

        void OnLevelStart(WaveLevelConfig config)
        {
            Debug.Log($"Level Started: {config.levelName}");
        }

        void OnLevelComplete(WaveLevelConfig config)
        {
            Debug.Log($"Level Complete!");
        }

        void OnGameEnd(GameResult result)
        {
            if (waveInfoPanel != null)
                waveInfoPanel.SetActive(false);

            if (bossWarningPanel != null)
                bossWarningPanel.SetActive(false);

            Debug.Log($"Game End: {result}");
        }

        void OnStateChanged(WaveState state)
        {
            Debug.Log($"Wave State Changed: {state}");
        }

        void OnWaveTimeUpdate(float timeRemaining)
        {
            if (waveTimeText != null)
            {
                int minutes = Mathf.FloorToInt(timeRemaining / 60);
                int seconds = Mathf.FloorToInt(timeRemaining % 60);
                waveTimeText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        #endregion

        void UpdateWaveInfo()
        {
            if (waveManager == null || !waveManager.IsPlaying)
                return;

            if (monsterCountText != null)
            {
                monsterCountText.text = $"Monsters: {waveManager.ActiveMonsterCount}";
            }

            UpdateWaveProgress();
        }

        void UpdateWaveProgress()
        {
            if (waveManager?.CurWave == null || waveProgressSlider == null)
                return;

            if (waveManager.CurWave.duration > 0)
            {
                float progress = 1f - (waveManager.WaveTimeRemaining / waveManager.CurWave.duration);
                waveProgressSlider.value = progress;
            }
        }

        void UpdateBossWarning()
        {
            if (waveManager == null || !waveManager.IsPlaying)
                return;

            var config = waveManager.CurWave;
            if (config == null || config.clearStrategy != WaveClearStrategy.DefeatBoss)
                return;

            // 检查是否即将生成Boss
            float timeToBoss = config.bossSpawnTime - waveManager.WaveTimeElapsed;

            if (timeToBoss > 0 && timeToBoss <= bossWarningTime)
            {
                if (bossWarningPanel != null && !bossWarningPanel.activeSelf)
                    bossWarningPanel.SetActive(true);

                if (bossWarningText != null)
                    bossWarningText.text = $"BOSS INCOMING!\n{Mathf.CeilToInt(timeToBoss)}s";
            }
            else if (timeToBoss <= -5)
            {
                if (bossWarningPanel != null)
                    bossWarningPanel.SetActive(false);
            }
        }

        string GetStrategyText(WaveClearStrategy strategy)
        {
            return strategy switch
            {
                WaveClearStrategy.SurviveUntilEnd => "坚持到倒计时结束",
                WaveClearStrategy.DefeatAllMonsters => "击败所有怪物",
                WaveClearStrategy.DefeatBoss => "击败Boss通关",
                _ => "未知策略"
            };
        }

        #region Debug

        void OnGUI()
        {
            var m = waveManager;
            if (!showDebugInfo || m == null)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.BeginVertical("box");

            GUILayout.Label($"=== Wave Debug Info ===");
            GUILayout.Label($"State: {m.State}");
            GUILayout.Label($"Wave: {m.WaveNumber}/{m.CurLevel?.MaxWave ?? 0}");
            GUILayout.Label($"Active Monsters: {m.ActiveMonsterCount}");
            GUILayout.Label($"Active Bosses: {m.ActiveBossCount}");
            GUILayout.Label($"Time Remaining: {m.WaveTimeRemaining:F1}s");
            GUILayout.Label($"Time Elapsed: {m.WaveTimeElapsed:F1}s");
            GUILayout.Label($"Kill Count: {m.WaveKillCount}");
            GUILayout.Label($"Spawn Count: {m.WaveSpawnCount}");

            GUILayout.Space(10);
            GUILayout.Label($"=== Scaling ===");
            var scaling = m.GetScalingData();
            GUILayout.Label($"Health: x{scaling.healthMultiplier:F2}");
            GUILayout.Label($"Damage: x{scaling.damageMultiplier:F2}");
            GUILayout.Label($"Speed: x{scaling.speedMultiplier:F2}");
            GUILayout.Label($"Defense: x{scaling.defenseMultiplier:F2}");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        #endregion
    }
}

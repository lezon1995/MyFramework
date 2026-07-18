/*using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 波次系统整合管理器 - 在GameplayManager或其他启动点初始化所有波次相关系统
    /// </summary>
    public class WaveSystemBootstrap : MonoBehaviour
    {
        [Header("Level Configuration")] [Tooltip("默认关卡配置")]
        public WaveLevelConfig defaultLevelConfig;

        [Header("Settings")] [Tooltip("游戏启动时自动开始")]
        public bool autoStart;

        void Awake()
        {
            InitializeWaveSystems();
        }

        void Start()
        {
            if (autoStart && defaultLevelConfig)
            {
                StartWaveGame();
            }
        }

        /// <summary>
        /// 初始化所有波次相关系统
        /// </summary>
        public void InitializeWaveSystems()
        {
            Debug.Log("[WaveSystemBootstrap] Initializing Wave Systems...");

            // 确保WaveManager已初始化
            if (WaveManager.Instance == null)
            {
                var waveManagerObj = new GameObject("WaveManager");
                waveManagerObj.AddComponent<WaveManagerPlaceholder>();
                Debug.Log("[WaveSystemBootstrap] WaveManager initialized");
            }

            // 确保WaveRewardManager已初始化
            if (WaveRewardManager.Instance == null)
            {
                var rewardManagerObj = new GameObject("WaveRewardManager");
                rewardManagerObj.AddComponent<WaveRewardManagerPlaceholder>();
                Debug.Log("[WaveSystemBootstrap] WaveRewardManager initialized");
            }

            // 确保WaveGameMode已初始化
            if (WaveGameMode.Instance == null)
            {
                var gameModeObj = new GameObject("WaveGameMode");
                gameModeObj.AddComponent<WaveGameModePlaceholder>();
                Debug.Log("[WaveSystemBootstrap] WaveGameMode initialized");
            }

            Debug.Log("[WaveSystemBootstrap] All Wave Systems initialized successfully!");
        }

        /// <summary>
        /// 开始波次游戏
        /// </summary>
        public void StartWaveGame()
        {
            if (WaveGameMode.Instance && defaultLevelConfig)
            {
                WaveGameMode.Instance.StartGame(defaultLevelConfig);
            }
            else
            {
                Debug.LogError("[WaveSystemBootstrap] Cannot start game: WaveGameMode or LevelConfig is null!");
            }
        }

        /// <summary>
        /// 开始指定关卡的波次游戏
        /// </summary>
        public void StartWaveGame(WaveLevelConfig levelConfig)
        {
            if (WaveGameMode.Instance && levelConfig)
            {
                WaveGameMode.Instance.StartGame(levelConfig);
            }
            else
            {
                Debug.LogError("[WaveSystemBootstrap] Cannot start game: WaveGameMode or LevelConfig is null!");
            }
        }
    }

    /// <summary>
    /// 占位组件用于初始化WaveManager
    /// </summary>
    public class WaveManagerPlaceholder : MonoBehaviour
    {
        WaveManager _waveManager;

        void Awake()
        {
            _waveManager = gameObject.AddComponent<WaveManager>();
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// 占位组件用于初始化WaveRewardManager
    /// </summary>
    public class WaveRewardManagerPlaceholder : MonoBehaviour
    {
        WaveRewardManager _rewardManager;

        void Awake()
        {
            _rewardManager = gameObject.AddComponent<WaveRewardManager>();
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// 占位组件用于初始化WaveGameMode
    /// </summary>
    public class WaveGameModePlaceholder : MonoBehaviour
    {
        WaveGameMode _gameMode;

        void Awake()
        {
            _gameMode = gameObject.AddComponent<WaveGameMode>();
            DontDestroyOnLoad(gameObject);
        }
    }
}*/
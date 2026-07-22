using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains
{
    /// <summary>
    /// 金币状态
    /// </summary>
    public enum CoinState
    {
        Idle, // 空闲/未激活
        Dropping, // 掉落动画中（不可拾取）
        Grounded, // 落地静止（可拾取）
        BeingCollected, // 被玩家吸引中（拾取动画中）
        Collected // 已收集完成
    }

    /// <summary>
    /// 金币管理器 - 管理所有金币的生成、掉落、拾取
    /// 使用对象池避免频繁创建销毁
    /// </summary>
    public class CoinManager : MonoBehaviour
    {
        public Coin CoinPrefab;

        #region Singleton

        public static CoinManager Instance { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// 玩家拾取范围（默认5个单位）
        /// </summary>
        public float PickupRange { get; set; } = 5f;

        /// <summary>
        /// 拾取延迟时间（掉落动画结束后额外等待多久才能被拾取）
        /// </summary>
        public float PickupDelay { get; set; } = 0f;

        /// <summary>
        /// 是否启用自动拾取
        /// </summary>
        public bool AutoPickupEnabled { get; set; } = true;

        /// <summary>
        /// 掉落动画配置
        /// </summary>
        public CoinDropConfig DropConfig { get; set; }

        /// <summary>
        /// 拾取动画配置
        /// </summary>
        public CoinPickupConfig PickupConfig { get; set; }

        /// <summary>
        /// 默认金币预制体路径
        /// </summary>
        public string DefaultCoinPrefabPath { get; set; } = $"{GAMEPLAY_PATH}/Coin/Coin_0.prefab";


        /// <summary>
        /// 金币挂载的父对象Transform
        /// </summary>
        public Transform CoinParent { get; protected set; }

        #endregion

        #region Private Fields

        SafeDictionary<int, Coin> activeCoins = new();
        Dictionary<Type, ObjectPool<Coin>> coinPools = new();

        // 临时列表用于更新（避免修改集合时迭代）
        List<Coin> _coinsToUpdate = new();

        // 拾取者引用（用于接收拾取事件）
        APicker _activePicker;

        #endregion

        #region Events

        public event Action<Coin> OnCoinSpawned;
        public event Action<Coin> OnCoinLanded;
        public event Action<Coin> OnCoinCollected;
        public event Action<int> OnGoldCollected; // 总金币数变化

        #endregion

        public void Awake()
        {
            Instance = this;

            // 初始化默认配置
            DropConfig = CoinDropConfig.Default;
            PickupConfig = CoinPickupConfig.Default;

            // 创建金币挂载父对象
            var parentObj = new GameObject("CoinParent");
            parentObj.transform.SetParent(transform, false);
            CoinParent = parentObj.transform;
        }

        public void OnDestroy()
        {
            using var _ = new SafeDictionaryReader<int, Coin>(activeCoins, out var reader);
            foreach (var (_, coin) in reader)
                releaseCoin(coin);

            activeCoins.clear();
            coinPools.Clear();

            if (CoinParent != null)
            {
                UnityEngine.Object.Destroy(CoinParent.gameObject);
                CoinParent = null;
            }

            Instance = null;
        }

        public void Update()
        {
            if (activeCoins.count() == 0)
                return;

            var dt = Time.deltaTime;

            // 更新所有活跃金币
            _coinsToUpdate.Clear();
            _coinsToUpdate.AddRange(activeCoins.Values);

            foreach (var coin in _coinsToUpdate)
            {
                if (coin != null)
                    updateCoin(coin, dt);
            }
        }

        #region Public Methods - 掉落金币

        /// <summary>
        /// 在指定位置掉落金币
        /// </summary>
        /// <param name="position">掉落起点（DropPoint, 怪物位置）</param>
        /// <param name="dropDirection">掉落方向（任意方向；Coin 内部用射线与椭圆边界交点计算落点）</param>
        /// <param name="value">金币价值</param>
        /// <param name="config">掉落配置（可选）</param>
        /// <returns>生成的金币对象</returns>
        public Coin DropCoin(Vector2 position, Vector2 dropDirection, int value = 1, CoinDropConfig config = null)
        {
            config ??= DropConfig;

            var coin = acquireCoin();
            if (coin == null)
                return null;

            coin.Initialize(value, position, dropDirection, config, PickupConfig);

            activeCoins[coin.instanceID] = coin;
            OnCoinSpawned?.Invoke(coin);

            return coin;
        }

        /// <summary>
        /// 在指定位置掉落多个金币（每个金币方向随机偏移一个小角度）
        /// 落点 = 各方向射线与椭圆边界的交点
        /// </summary>
        public void DropCoins(Vector2 position, Vector2 dropDirection, int count, int valuePerCoin = 1, CoinDropConfig config = null)
        {
            if (count <= 0 || valuePerCoin <= 0)
                return;

            config ??= DropConfig;

            float spread = config.DirectionSpreadAngle;
            Vector2 baseDir = dropDirection.sqrMagnitude > 0.0001f ? dropDirection.normalized : Vector2.up;

            for (int i = 0; i < count; i++)
            {
                float angle = UnityEngine.Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector2 scatteredDir = rotation * baseDir;

                DropCoin(position, scatteredDir, valuePerCoin, config);
            }
        }

        /// <summary>
        /// 一次性掉落指定总价值的金币（自动拆分，每个方向随机偏移一个小角度）
        /// </summary>
        public void DropCoinBurst(Vector2 position, Vector2 dropDirection, int totalValue, int coinsPerDrop = 1, CoinDropConfig config = null)
        {
            if (totalValue <= 0 || coinsPerDrop <= 0)
                return;

            config ??= DropConfig;

            int remaining = totalValue;
            int coinValue = Mathf.Max(1, totalValue / coinsPerDrop);
            float spread = config.DirectionSpreadAngle;
            Vector2 baseDir = dropDirection.sqrMagnitude > 0.0001f ? dropDirection.normalized : Vector2.up;

            for (int i = 0; i < coinsPerDrop; i++)
            {
                float angle = UnityEngine.Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector2 scatteredDir = rotation * baseDir;

                int value = (i == coinsPerDrop - 1) ? remaining : coinValue;
                remaining -= value;

                DropCoin(position, scatteredDir, value, config);
            }
        }

        /// <summary>
        /// 根据怪物掉落配置掉落金币（每个金币方向随机偏移一个小角度）
        /// </summary>
        public void DropCoinsByMonsterConfig(Vector2 monsterPos, Vector2 playerPos, MonsterCoinDropConfig config)
        {
            if (config == null)
                return;

            int coinCount = config.RollCoinCount();
            if (coinCount <= 0)
                return;

            Vector2 direction = config.GetDropDirection(monsterPos, playerPos);
            var dropCfg = config.dropConfigOverride ?? DropConfig;
            var pickupCfg = config.pickupConfigOverride ?? PickupConfig;
            float spread = dropCfg.DirectionSpreadAngle;

            Vector2 baseDir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;

            for (int i = 0; i < coinCount; i++)
            {
                float angle = UnityEngine.Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector2 scatteredDir = rotation * baseDir;

                DropCoin(monsterPos, scatteredDir, config.coinValue, dropCfg);
            }
        }

        #endregion

        #region Public Methods - 拾取金币

        /// <summary>
        /// 注册当前活跃的拾取者（接收拾取通知）
        /// </summary>
        public void RegisterPicker(APicker picker)
        {
            _activePicker = picker;
        }

        /// <summary>
        /// 注销拾取者
        /// </summary>
        public void UnregisterPicker(APicker picker)
        {
            if (_activePicker == picker)
                _activePicker = null;
        }

        /// <summary>
        /// 设置玩家拾取范围
        /// </summary>
        public void SetPickupRange(float range)
        {
            PickupRange = Mathf.Max(0f, range);
        }

        /// <summary>
        /// 获取范围内可拾取的金币
        /// </summary>
        public void GetCoinsInPickupRange(Vector2 center, ref List<Coin> result, float rangeOverride = -1f)
        {
            result.Clear();
            float range = rangeOverride > 0 ? rangeOverride : PickupRange;
            float rangeSq = range * range;

            using var _ = new SafeDictionaryReader<int, Coin>(activeCoins, out var reader);
            foreach (var (_, coin) in reader)
            {
                if (coin == null)
                    continue;

                if (coin.State == CoinState.Grounded)
                {
                    Vector2 diff = coin.Position - center;
                    if (diff.sqrMagnitude <= rangeSq)
                    {
                        result.Add(coin);
                    }
                }
            }
        }

        /// <summary>
        /// 尝试拾取范围内的金币（只启动拾取动画，金币实际到账在动画结束后）
        /// </summary>
        /// <returns>启动拾取的金币总价值（仅作通知用，实际到账在动画完成后）</returns>
        public int TryPickupCoinsInRange(Transform target)
        {
            return TryPickupCoinsInRange(target, PickupRange);
        }

        /// <summary>
        /// 尝试拾取范围内的金币（指定范围）
        /// </summary>
        public int TryPickupCoinsInRange(Transform targetTransform, float range)
        {
            if (!AutoPickupEnabled)
                return 0;

            int totalValue = 0;
            using var _ = new ListScope<Coin>(out var coinsToPickup);
            GetCoinsInPickupRange(targetTransform.position, ref coinsToPickup, range);

            foreach (var coin in coinsToPickup)
            {
                if (coin && coin.TryStartPickup(targetTransform))
                {
                    totalValue += coin.Value;
                }
            }

            return totalValue;
        }

        /// <summary>
        /// 手动拾取单个金币
        /// </summary>
        public bool PickupCoin(Coin coin, Transform target)
        {
            if (coin == null || coin.State != CoinState.Grounded)
                return false;

            return coin.TryStartPickup(target);
        }

        #endregion

        #region Public Methods - 其他

        /// <summary>
        /// 清理所有金币
        /// </summary>
        public void ClearAllCoins()
        {
            using var _ = new SafeDictionaryReader<int, Coin>(activeCoins, out var reader);
            foreach (var (_, coin) in reader)
                releaseCoin(coin);

            activeCoins.clear();
        }

        /// <summary>
        /// 获取当前活跃金币数量
        /// </summary>
        public int GetActiveCoinCount()
        {
            return activeCoins.count();
        }

        #endregion

        #region Private Methods

        void updateCoin(Coin coin, float elapsedTime)
        {
            if (coin == null)
                return;

            switch (coin.State)
            {
                case CoinState.Dropping:
                    coin.UpdateDropping(elapsedTime);
                    if (coin.State == CoinState.Grounded)
                    {
                        OnCoinLanded?.Invoke(coin);
                    }

                    break;

                case CoinState.BeingCollected:
                    coin.UpdatePickup(elapsedTime);
                    if (coin.State == CoinState.Collected)
                    {
                        OnCoinCollected?.Invoke(coin);
                        // 通知拾取者实际到账
                        _activePicker?.OnGoldCollected(coin.Value);
                        OnGoldCollected?.Invoke(coin.Value);
                        // 触发事件
                        new OnCoinCollected_S(coin, coin.Position, coin.Value, _activePicker).trigger();
                        // 释放到对象池
                        releaseCoin(coin);
                    }

                    break;
            }
        }

        Coin acquireCoin()
        {
            if (!coinPools.TryGetValue(typeof(Coin), out var pool))
            {
                pool = new ObjectPool<Coin>(
                    createFunc: createCoin,
                    actionOnGet: coin =>
                    {
                        coin.gameObject.SetActive(true);
                        activeCoins[coin.instanceID] = coin;
                    },
                    actionOnRelease: coin =>
                    {
                        activeCoins.remove(coin.instanceID);
                        coin.OnRelease();
                        coin.gameObject.SetActive(false);
                    },
                    actionOnDestroy: destroyCoin,
                    collectionCheck: true,
                    defaultCapacity: 50,
                    maxSize: 300);

                coinPools.add(typeof(Coin), pool);
            }

            return pool.Get();
        }

        void releaseCoin(Coin coin)
        {
            if (coin == null)
                return;

            if (coinPools.TryGetValue(typeof(Coin), out var pool))
            {
                pool.Release(coin);
            }
        }

        Coin createCoin()
        {
            Coin coin;
            if (prefabPool != null)
            {
                coin = prefabPool.createObject(DefaultCoinPrefabPath, true, CoinParent ? CoinParent.gameObject : gameObject).GetComponent<Coin>();
            }
            else
            {
                coin = Instantiate(CoinPrefab, CoinParent ? CoinParent : transform);
            }

            if (coin == null)
            {
                logError("Failed to create coin prefab. Please ensure the prefab exists at: " + DefaultCoinPrefabPath);
                return null;
            }

            coin.Acquire();
            return coin;
        }

        void destroyCoin(Coin coin)
        {
            if (coin == null)
                return;

            activeCoins.remove(coin.instanceID);
            if (prefabPool != null)
            {
                prefabPool.destroyObject(coin.gameObject, false);
            }
            else
            {
                Destroy(coin.gameObject);
            }
        }

        #endregion
    }
}
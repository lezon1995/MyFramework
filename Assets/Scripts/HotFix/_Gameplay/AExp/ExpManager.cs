using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains
{
    /// <summary>
    /// 经验值物品管理器 - 管理所有经验值物品的生成、掉落、拾取
    /// 使用对象池避免频繁创建销毁
    ///
    /// 拾取逻辑：
    ///   1) 玩家站在拾取范围内时，自动对所有落地的经验值物品启动拾取动画
    ///   2) 拾取动画为两段式：先飞离玩家一小段距离，再飞向玩家
    ///   3) 经验值物品真正到达玩家位置后才视为拾取到账，调用 IExpPicker.OnExpCollected
    /// </summary>
    public class ExpManager : MonoBehaviour
    {
        public ExpOrb ExpPrefab;

        #region Singleton

        public static ExpManager Instance { get; private set; }

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
        public ExpDropConfig DropConfig { get; set; }

        /// <summary>
        /// 拾取动画配置
        /// </summary>
        public ExpPickupConfig PickupConfig { get; set; }

        /// <summary>
        /// 默认经验值物品预制体路径
        /// </summary>
        public string DefaultExpPrefabPath { get; set; } = $"{GAMEPLAY_PATH}/Exp/Experence.prefab";

        /// <summary>
        /// 经验值物品挂载的父对象 Transform
        /// </summary>
        public Transform ExpParent { get; protected set; }

        #endregion

        #region Private Fields

        SafeDictionary<int, ExpOrb> activeExps = new();
        Dictionary<Type, ObjectPool<ExpOrb>> expPools = new();

        // 临时列表用于更新（避免修改集合时迭代）
        List<ExpOrb> _expsToUpdate = new();

        // 拾取者引用（用于接收拾取事件）
        IExpPicker _activePicker;

        // 全局拾取目标 Transform（玩家 Transform）
        // 拾取过程中物品需要实时跟踪玩家位置
        Transform _pickupTargetCache;

        #endregion

        #region Events

        public event Action<ExpOrb> OnExpSpawned;
        public event Action<ExpOrb> OnExpLanded;
        public event Action<ExpOrb> OnExpCollected; // 单个经验值物品动画完成
        public event Action<int> OnExpTotalCollected; // 经验值总和

        #endregion

        public void Awake()
        {
            Instance = this;

            // 初始化默认配置
            DropConfig = ExpDropConfig.Default;
            PickupConfig = ExpPickupConfig.Default;

            // 创建经验值物品挂载父对象
            var parentObj = new GameObject("ExpParent");
            parentObj.transform.SetParent(transform, false);
            ExpParent = parentObj.transform;
        }

        public void OnDestroy()
        {
            using var _ = new SafeDictionaryReader<int, ExpOrb>(activeExps, out var reader);
            foreach (var (_, exp) in reader)
                releaseExp(exp);

            activeExps.clear();
            expPools.Clear();

            if (ExpParent != null)
            {
                UnityEngine.Object.Destroy(ExpParent.gameObject);
                ExpParent = null;
            }

            Instance = null;
        }

        public void Update()
        {
            if (activeExps.count() == 0)
                return;

            var dt = Time.deltaTime;

            // 更新所有活跃经验值物品
            _expsToUpdate.Clear();
            _expsToUpdate.AddRange(activeExps.Values);

            foreach (var exp in _expsToUpdate)
            {
                if (exp != null)
                    updateExp(exp, dt);
            }
        }

        #region Public Methods - 掉落经验值

        /// <summary>
        /// 在指定位置掉落经验值物品
        /// </summary>
        /// <param name="position">掉落起点（DropPoint，怪物位置）</param>
        /// <param name="dropDirection">掉落方向（任意方向；ExpOrb 内部用射线与椭圆边界交点计算落点）</param>
        /// <param name="value">经验值物品价值</param>
        /// <param name="config">掉落配置（可选）</param>
        /// <returns>生成的经验值物品对象</returns>
        public ExpOrb DropExp(Vector2 position, Vector2 dropDirection, int value = 1, ExpDropConfig config = null)
        {
            config ??= DropConfig;

            var exp = acquireExp();
            if (exp == null)
                return null;

            exp.Initialize(value, position, dropDirection, config, PickupConfig);

            activeExps[exp.instanceID] = exp;
            OnExpSpawned?.Invoke(exp);

            return exp;
        }

        /// <summary>
        /// 在指定位置掉落多个经验值物品（每个物品方向随机偏移一个小角度）
        /// </summary>
        public void DropExps(Vector2 position, Vector2 dropDirection, int count, int valuePerExp = 1, ExpDropConfig config = null)
        {
            if (count <= 0 || valuePerExp <= 0)
                return;

            config ??= DropConfig;

            float spread = config.DirectionSpreadAngle;
            Vector2 baseDir = dropDirection.sqrMagnitude > 0.0001f ? dropDirection.normalized : Vector2.up;

            for (int i = 0; i < count; i++)
            {
                float angle = UnityEngine.Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector2 scatteredDir = rotation * baseDir;

                DropExp(position, scatteredDir, valuePerExp, config);
            }
        }

        /// <summary>
        /// 一次性掉落指定总经验值的物品（自动拆分）
        /// </summary>
        public void DropExpBurst(Vector2 position, Vector2 dropDirection, int totalValue, int expsPerDrop = 1, ExpDropConfig config = null)
        {
            if (totalValue <= 0 || expsPerDrop <= 0)
                return;

            config ??= DropConfig;

            int remaining = totalValue;
            int expValue = Mathf.Max(1, totalValue / expsPerDrop);
            float spread = config.DirectionSpreadAngle;
            Vector2 baseDir = dropDirection.sqrMagnitude > 0.0001f ? dropDirection.normalized : Vector2.up;

            for (int i = 0; i < expsPerDrop; i++)
            {
                float angle = UnityEngine.Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector2 scatteredDir = rotation * baseDir;

                int value = (i == expsPerDrop - 1) ? remaining : expValue;
                remaining -= value;

                DropExp(position, scatteredDir, value, config);
            }
        }

        /// <summary>
        /// 根据怪物经验值掉落配置掉落经验值物品
        /// </summary>
        public void DropExpsByMonsterConfig(Vector2 monsterPos, Vector2 playerPos, MonsterExpDropConfig config)
        {
            if (config == null)
                return;

            int expCount = config.RollExpCount();
            if (expCount <= 0)
                return;

            Vector2 direction = config.GetDropDirection(monsterPos, playerPos);
            var dropCfg = config.dropConfigOverride ?? DropConfig;
            float spread = dropCfg.DirectionSpreadAngle;

            Vector2 baseDir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;

            for (int i = 0; i < expCount; i++)
            {
                float angle = UnityEngine.Random.Range(-spread, spread);
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector2 scatteredDir = rotation * baseDir;

                DropExp(monsterPos, scatteredDir, config.expValue, dropCfg);
            }
        }

        #endregion

        #region Public Methods - 拾取经验值

        /// <summary>
        /// 注册当前活跃的拾取者（接收拾取通知）
        /// 同时缓存玩家 Transform，拾取过程中物品需要跟踪玩家位置
        /// </summary>
        public void RegisterPicker(IExpPicker picker)
        {
            _activePicker = picker;
            // 缓存玩家 Transform（如果 picker 实现了 MonoBehaviour 也能拿到）
            if (picker is MonoBehaviour mb)
                _pickupTargetCache = mb.transform;
        }

        /// <summary>
        /// 注销拾取者
        /// </summary>
        public void UnregisterPicker(IExpPicker picker)
        {
            if (_activePicker == picker)
            {
                _activePicker = null;
                _pickupTargetCache = null;
            }
        }

        /// <summary>
        /// 设置玩家拾取范围
        /// </summary>
        public void SetPickupRange(float range)
        {
            PickupRange = Mathf.Max(0f, range);
        }

        /// <summary>
        /// 获取范围内可拾取的经验值物品
        /// </summary>
        public void GetExpsInPickupRange(Vector2 center, ref List<ExpOrb> result, float rangeOverride = -1f)
        {
            result.Clear();
            float range = rangeOverride > 0 ? rangeOverride : PickupRange;
            float rangeSq = range * range;

            using var _ = new SafeDictionaryReader<int, ExpOrb>(activeExps, out var reader);
            foreach (var (_, exp) in reader)
            {
                if (exp == null)
                    continue;

                if (exp.State == ExpOrbState.Grounded)
                {
                    Vector2 diff = exp.Position - center;
                    if (diff.sqrMagnitude <= rangeSq)
                    {
                        result.Add(exp);
                    }
                }
            }
        }

        /// <summary>
        /// 尝试拾取范围内的经验值物品（只启动拾取动画，经验值实际到账在动画结束后）
        /// </summary>
        /// <returns>启动拾取的经验值物品总价值（仅作通知用，实际到账在动画完成后）</returns>
        public int TryPickupExpsInRange(Transform target)
        {
            return TryPickupExpsInRange(target, PickupRange);
        }

        /// <summary>
        /// 尝试拾取范围内的经验值物品（指定范围）
        /// 经验值物品的拾取动画会随着玩家移动实时跟踪玩家位置
        /// </summary>
        public int TryPickupExpsInRange(Transform targetTransform, float range)
        {
            if (!AutoPickupEnabled || targetTransform == null)
                return 0;

            int totalValue = 0;
            using var _ = new ListScope<ExpOrb>(out var expsToPickup);
            GetExpsInPickupRange(targetTransform.position, ref expsToPickup, range);

            // 每帧都更新 _pickupTargetCache（确保最新的玩家 Transform）
            _pickupTargetCache = targetTransform;

            foreach (var exp in expsToPickup)
            {
                if (exp && exp.TryStartPickup(targetTransform))
                {
                    totalValue += exp.Value;
                }
            }

            return totalValue;
        }

        /// <summary>
        /// 手动拾取单个经验值物品
        /// </summary>
        public bool PickupExp(ExpOrb exp, Transform target)
        {
            if (exp == null || exp.State != ExpOrbState.Grounded)
                return false;

            _pickupTargetCache = target;
            return exp.TryStartPickup(target);
        }

        #endregion

        #region Public Methods - 其他

        /// <summary>
        /// 清理所有经验值物品
        /// </summary>
        public void ClearAllExps()
        {
            using var _ = new SafeDictionaryReader<int, ExpOrb>(activeExps, out var reader);
            foreach (var (_, exp) in reader)
                releaseExp(exp);

            activeExps.clear();
        }

        /// <summary>
        /// 获取当前活跃经验值物品数量
        /// </summary>
        public int GetActiveExpCount()
        {
            return activeExps.count();
        }

        #endregion

        #region Private Methods

        void updateExp(ExpOrb exp, float elapsedTime)
        {
            if (exp == null)
                return;

            switch (exp.State)
            {
                case ExpOrbState.Dropping:
                    exp.UpdateDropping(elapsedTime);
                    if (exp.State == ExpOrbState.Grounded)
                    {
                        OnExpLanded?.Invoke(exp);
                    }

                    break;

                case ExpOrbState.BeingCollected:
                    exp.UpdatePickup(elapsedTime);
                    if (exp.State == ExpOrbState.Collected)
                    {
                        OnExpCollected?.Invoke(exp);

                        // 通知拾取者实际到账（动画真正到达玩家位置才到账）
                        _activePicker?.OnExpCollected(exp.Value);
                        OnExpTotalCollected?.Invoke(exp.Value);

                        // 触发事件
                        new OnExpCollected_S(exp, exp.Position, exp.Value, _activePicker).trigger();

                        // 释放到对象池
                        releaseExp(exp);
                    }

                    break;
            }
        }

        ExpOrb acquireExp()
        {
            if (!expPools.TryGetValue(typeof(ExpOrb), out var pool))
            {
                pool = new ObjectPool<ExpOrb>(
                    createFunc: createExp,
                    actionOnGet: exp =>
                    {
                        exp.gameObject.SetActive(true);
                        activeExps[exp.instanceID] = exp;
                    },
                    actionOnRelease: exp =>
                    {
                        activeExps.remove(exp.instanceID);
                        exp.OnRelease();
                        exp.gameObject.SetActive(false);
                    },
                    actionOnDestroy: destroyExp,
                    collectionCheck: true,
                    defaultCapacity: 50,
                    maxSize: 300);

                expPools.add(typeof(ExpOrb), pool);
            }

            return pool.Get();
        }

        void releaseExp(ExpOrb exp)
        {
            if (exp == null)
                return;

            if (expPools.TryGetValue(typeof(ExpOrb), out var pool))
            {
                pool.Release(exp);
            }
        }

        ExpOrb createExp()
        {
            ExpOrb exp;
            if (prefabPool != null)
            {
                exp = prefabPool.createObject(DefaultExpPrefabPath, true, ExpParent ? ExpParent.gameObject : gameObject).GetComponent<ExpOrb>();
            }
            else
            {
                exp = Instantiate(ExpPrefab, ExpParent ? ExpParent : transform);
            }

            if (exp == null)
            {
                logError("Failed to create exp prefab. Please ensure the prefab exists at: " + DefaultExpPrefabPath);
                return null;
            }

            exp.Acquire();
            return exp;
        }

        void destroyExp(ExpOrb exp)
        {
            if (exp == null)
                return;

            activeExps.remove(exp.instanceID);
            if (prefabPool != null)
            {
                prefabPool.destroyObject(exp.gameObject, false);
            }
            else
            {
                Destroy(exp.gameObject);
            }
        }

        #endregion
    }
}

using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 玩家金币拾取适配器 - 将APlayer适配到APicker接口
    /// 挂载到玩家身上，自动接收金币拾取事件
    /// </summary>
    public class CoinPickerAdapter : PlayerAbility, ICoinPicker
    {
        public CoinManager coinManager;

        #region Properties

        /// <summary>
        /// 拾取范围覆盖（小于0则使用全局设置）
        /// </summary>
        public float PickupRangeOverride = 2f;

        /// <summary>
        /// 自动检测间隔（秒）- 玩家位置每帧都检测，但触发拾取的最小间隔
        /// </summary>
        public float AutoPickupInterval = 0.1f;

        /// <summary>
        /// 是否启用自动拾取
        /// </summary>
        public bool AutoPickupEnabled = true;

        /// <summary>
        /// 当前已拾取金币总数（只读统计）
        /// </summary>
        public int TotalGoldCollected { get; protected set; }

        #endregion

        #region Private Fields

        float _autoPickupTimer;

        #endregion

        #region Events

        public event Action<int> OnGoldCollectedEvent;

        #endregion

        #region Lifecycle

        protected override void OnEnable()
        {
            base.OnEnable();
            if (coinManager)
                coinManager.RegisterPicker(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (coinManager)
                coinManager.UnregisterPicker(this);
        }

        protected override void Start()
        {
            base.Start();
            // 延迟注册，确保CoinManager已初始化
            if (coinManager)
                coinManager.RegisterPicker(this);
        }

        public override void OnUpdate(float dt)
        {
            if (!AutoPickupEnabled || coinManager == null || _player == null)
                return;

            _autoPickupTimer += dt;
            if (_autoPickupTimer >= AutoPickupInterval)
            {
                _autoPickupTimer = 0f;

                float range = PickupRangeOverride > 0 ? PickupRangeOverride : coinManager.PickupRange;
                coinManager.TryPickupCoinsInRange(_player.transform, range);
            }
        }

        #endregion

        #region APicker Implementation

        public Vector3 Position => _player ? _player.transform.position : transform.position;

        public void OnGoldCollected(int amount)
        {
            TotalGoldCollected += amount;
            _player.gainGold(amount);

            OnGoldCollectedEvent?.Invoke(amount);
            new OnGoldPickedUp_S(this, amount, Position).trigger();
            new GainCoinTextEvent(amount, transform).trigger();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 手动拾取范围内的金币
        /// </summary>
        public void TryPickupCoinsInRange()
        {
            if (coinManager == null || _player == null)
                return;

            float range = PickupRangeOverride > 0 ? PickupRangeOverride : coinManager.PickupRange;
            coinManager.TryPickupCoinsInRange(_player.transform, range);
        }

        /// <summary>
        /// 设置拾取范围
        /// </summary>
        public void SetPickupRange(float range)
        {
            PickupRangeOverride = range;
        }

        #endregion

        public void SetCoinManager(CoinManager mgr)
        {
            coinManager = mgr;
            if (mgr)
            {
                mgr.RegisterPicker(this);
            }
        }
    }
}
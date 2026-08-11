using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 玩家经验值拾取适配器 - 将 APlayer 适配到 IExpPicker 接口
    /// 挂载到玩家身上，自动接收经验值拾取事件
    ///
    /// 拾取流程：
    ///   1) 每帧检查玩家位置周围的可拾取经验值物品
    ///   2) 启动经验值物品的两段式拾取动画（先远离玩家，再飞向玩家）
    ///   3) 经验值物品真正到达玩家位置后（ExpManager 已自动到账），本适配器会调用玩家原生的 gainExp 接口
    /// </summary>
    public class ExpPickerAdapter : PlayerAbility, IExpPicker
    {
        public ExpManager expManager;

        #region Properties

        /// <summary>
        /// 拾取范围覆盖（小于0则使用全局设置）
        /// </summary>
        public float PickupRangeOverride = 5f;

        /// <summary>
        /// 自动检测间隔（秒）- 玩家位置每帧都检测，但触发拾取的最小间隔
        /// </summary>
        public float AutoPickupInterval = 0.1f;

        /// <summary>
        /// 是否启用自动拾取
        /// </summary>
        public bool AutoPickupEnabled = true;

        /// <summary>
        /// 当前已拾取经验值总数（只读统计）
        /// </summary>
        public int TotalExpCollected { get; protected set; }

        #endregion

        #region Private Fields

        float _autoPickupTimer;

        #endregion

        #region Events

        public event Action<int> OnExpCollectedEvent;

        #endregion

        #region Lifecycle

        protected override void OnEnable()
        {
            base.OnEnable();
            if (expManager)
                expManager.RegisterPicker(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (expManager)
                expManager.UnregisterPicker(this);
        }

        protected override void Start()
        {
            base.Start();
            // 延迟注册，确保 ExpManager 已初始化
            if (expManager)
                expManager.RegisterPicker(this);
        }

        public override void OnUpdate(float dt)
        {
            if (!AutoPickupEnabled || expManager == null || _player == null)
                return;

            _autoPickupTimer += dt;
            if (_autoPickupTimer >= AutoPickupInterval)
            {
                _autoPickupTimer = 0f;

                float range = PickupRangeOverride > 0 ? PickupRangeOverride : expManager.PickupRange;
                expManager.TryPickupExpsInRange(_player.transform, range);
            }
        }

        #endregion

        #region IExpPicker Implementation

        public Vector3 Position => _player ? _player.transform.position : transform.position;

        /// <summary>
        /// 当经验值拾取动画真正到达玩家位置时被 ExpManager 调用（此时经验值才真正到账）
        /// </summary>
        public void OnExpCollected(int amount)
        {
            TotalExpCollected += amount;
            _player.gainExp(amount);

            OnExpCollectedEvent?.Invoke(amount);
            new OnExpPickedUp_S(this, amount, Position).trigger();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 手动拾取范围内的经验值物品
        /// </summary>
        public void TryPickupExpsInRange()
        {
            if (expManager == null || _player == null)
                return;

            float range = PickupRangeOverride > 0 ? PickupRangeOverride : expManager.PickupRange;
            expManager.TryPickupExpsInRange(_player.transform, range);
        }

        /// <summary>
        /// 设置拾取范围
        /// </summary>
        public void SetPickupRange(float range)
        {
            PickupRangeOverride = range;
        }

        #endregion

        public void SetExpManager(ExpManager mgr)
        {
            // 解绑旧的
            if (expManager && expManager != mgr)
                expManager.UnregisterPicker(this);

            expManager = mgr;
            if (mgr)
            {
                mgr.RegisterPicker(this);
            }
        }
    }
}

using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 球管理系统 —— APlayer 上的能力组件（继承自 PlayerAbility），基类已经把 _player 自动赋值。
    /// 职责：
    ///   • 持 BallSlotGroup：发射槽位
    ///   • 持 BallUpgradeService / BallMergeService / BallShopService
    ///   • 提供装备 / 卸下 / 扩容 槽位的便利 API
    ///   • 把自己的 slots 注册到 InventoryLocate，便于跨容器查找
    ///
    /// 不持 BallBag —— 背包归 InventorySystem 拥有，本系统通过 IInventoryHolder 接口访问。
    /// </summary>
    public class BallManagementSystem : PlayerAbility
    {
        [Header("Slot")]
        [Tooltip("发射槽位数（默认 3，可运行时扩容）")]
        [SerializeField] int slotCount = 3;

        [Tooltip("扩容上限（防越界）")]
        [SerializeField] int maxSlotCount = 8;

        [Header("Upgrade")]
        [Tooltip("升级 X 合 1（默认 2）")]
        [SerializeField] int upgradeCombineCount = 2;

        [Tooltip("升级是否扣金币（默认 0）")]
        [SerializeField] int upgradeGoldCost;

        [Header("Refine")]
        [Tooltip("出售时回收比例，百分数（默认 50%）")]
        [SerializeField, Range(0, 100)] int sellRefundRate = 50;

        public int SlotCount          => _slots?.Capacity          ?? 0;
        public int MaxSlotCount       => maxSlotCount;
        public int UpgradeCombineCount => upgradeCombineCount;
        public int UpgradeGoldCost    => upgradeGoldCost;
        public int SellRefundRate     => sellRefundRate;

        BallSlotGroup _slots;
        BallInstanceService _instance;
        BallUpgradeService _upgrade;
        BallMergeService _merge;
        BallShopService _shop;

        public BallSlotGroup Slots => _slots;
        public BallInstanceService Instance => _instance;
        public BallUpgradeService Upgrade => _upgrade;
        public BallMergeService Merge => _merge;
        public BallShopService Shop => _shop;

        bool _systemReadyRaised;

        protected override void Initialization()
        {
            base.Initialization();
            int cnt = Mathf.Max(1, slotCount);
            _slots   = new(cnt);
            _instance = new(this);
            _upgrade = new(this);
            _merge   = new(this);
            _shop    = new(this);

            // 注册到定位器，让"球在哪"有一个统一查询入口
            InventoryLocate.Clear();
            InventoryLocate.Register(_slots);
            EnsureBallBagRegistered();

            if (!_systemReadyRaised)
            {
                _systemReadyRaised = true;
                BallEvents.RaiseSystemReady();
            }
        }

        protected override void OnDestroy()
        {
            if (_slots != null)
                InventoryLocate.Unregister(_slots);
            if (_systemReadyRaised)
            {
                _systemReadyRaised = false;
                BallEvents.RaiseSystemDestroy();
            }
        }

        void EnsureBallBagRegistered()
        {
            // BallBag 由 InventorySystem 在 Initialization() 中创建。
            // PlayerAbility 之间的初始化顺序在同帧内不严格，订阅一次 Readyx 事件即可。
            if (_player?.Inventory?.BallBag != null)
            {
                InventoryLocate.Register(_player.Inventory.BallBag);
                return;
            }

            Action<InventorySystem> handler = null;
            handler = s =>
            {
                if (s != null && s.BallBag != null)
                    InventoryLocate.Register(s.BallBag);
                InventoryEvents.OnSystemReady -= handler;
            };
            InventoryEvents.OnSystemReady += handler;
        }

        // -------- 便利 API（供 Command / Action / UI 直接调） --------

        public bool EquipBallAtInitialization(BallItem item)
        {
            if (item == null || _slots == null)
                return false;

            var success = _slots.TryPlaceFirstEmpty(item, out _);
            if (success)
            {
                var ball = _instance.acquireBall(item.Type);
                _instance.enqueueBallToShootQueue(ball);
            }

            return success;
        }
        
        /// <summary>把球装到第一个空槽。返回是否成功（背包里同名球会被一并尝试移走）。</summary>
        public bool EquipBall(BallItem item)
        {
            if (item == null || _slots == null)
                return false;

            // 从背包里拿出（如果还在）
            _player?.Inventory?.BallBag?.Remove(item);
            var success = _slots.TryPlaceFirstEmpty(item, out _);
            if (success)
            {
                var ball = _instance.acquireBall(item.Type);
                _instance.enqueueBallToShootQueue(ball);
            }
            
            return success;
        }

        /// <summary>把球装备到指定槽位。返回是否成功。</summary>
        public bool EquipBall(BallItem item, int slotIndex)
        {
            if (item == null || _slots == null)
                return false;

            _player.Inventory.BallBag.Remove(item);
            var success = _slots.TryPlaceAt(slotIndex, item);
            if (success)
            {
                var ball = _instance.acquireBall(item.Type);
                _instance.enqueueBallToShootQueue(ball);
            }

            return success;
        }

        /// <summary>从槽位卸下球到球背包。返回是否成功（背包满则拒绝）。</summary>
        public bool UnequipBall(int slotIndex)
        {
            if (_slots == null)
                return false;

            if (_player == null || _player.Inventory == null || !_player.Inventory.CanAddBall())
                return false;

            var success = _slots.PullFrom(slotIndex, out var item);
            if (success)
            {
                // _instance.dequeueBallFromShootQueue(ball);
            }

            return success && _player.Inventory.AddBall(item);
        }

        public bool SwapSlots(int a, int b)
        {
            if (_slots == null) 
                return false;

            _slots.Swap(a, b);
            return true;
        }

        /// <summary>扩容发射槽。返回是否成功。</summary>
        public bool ExpandSlots(int delta)
        {
            if (_slots == null || delta <= 0)
                return false;

            int target = _slots.Capacity + delta;
            if (target > maxSlotCount)
            {
                Debug.LogWarning("BallManagementSystem.ExpandSlots: exceeds MaxSlotCount");
                return false;
            }

            _slots.Expand(delta);
            return true;
        }
    }
}

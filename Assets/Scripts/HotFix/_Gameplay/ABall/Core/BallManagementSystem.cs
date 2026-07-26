using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 球管理系统 —— 唯一对外的球服务入口。
    /// 持有：
    ///   • BallSlotGroup：发射槽位
    ///   • BallUpgradeService / BallMergeService / BallShopService
    /// 不直接持有 BallBag —— 球背包归 InventorySystem 拥有，本系统通过 IInventoryHolder 接口操作。
    /// </summary>
    public sealed class BallManagementSystem : FrameSystem
    {
        public static BallManagementSystem Instance { get; private set; }

        BallSlotGroup _slots;
        BallUpgradeService _upgrade;
        BallMergeService _merge;
        BallShopService _shop;

        public BallSlotGroup Slots => _slots;
        public BallUpgradeService Upgrade => _upgrade;
        public BallMergeService Merge => _merge;
        public BallShopService Shop => _shop;

        public override void init()
        {
            base.init();
            Instance = this;

            var cfg = BallSystemConfig.Instance;
            int slotCount = cfg != null ? Mathf.Max(1, cfg.SlotCount) : 3;
            _slots = new BallSlotGroup(slotCount);

            _upgrade = new BallUpgradeService(this);
            _merge = new BallMergeService(this);
            _shop = new BallShopService(this);

            // 把当前对玩家生效的 holder 注册到定位器
            InventoryLocate.Clear();
            InventoryLocate.Register(_slots);
            // BallBag 会在 InventorySystem.init() 之后注册进来；这里我们用延迟注册：监听 InventorySystem.OnSystemReady
            InventorySystemReadinessWaiter();

            BallEvents.RaiseSystemReady();
        }

        public override void willDestroy()
        {
            base.willDestroy();
            InventoryLocate.Unregister(_slots);
            BallEvents.RaiseSystemDestroy();
            if (Instance == this) Instance = null;
            _slots = null;
        }

        // BallBag 的注册需要在 InventorySystem.init() 之后；
        // 这里用一次性的事件订阅，确保 BallBag 出现后能注册到定位器。
        void InventorySystemReadinessWaiter()
        {
            if (InventorySystem.Instance != null && InventorySystem.Instance.BallBag != null)
            {
                InventoryLocate.Register(InventorySystem.Instance.BallBag);
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

        // -------- 便利 API（供外部 Command / Action 直接调） --------

        /// <summary>把球装备到指定槽位。返回是否成功。</summary>
        public bool EquipBall(BallInstance ball, int slotIndex)
        {
            if (ball == null || _slots == null) return false;
            // 从背包里拿出（如果还在）
            var bag = InventorySystem.Instance?.BallBag;
            bag?.Remove(ball);
            return _slots.TryPlaceAt(slotIndex, ball);
        }

        /// <summary>从槽位卸下球到球背包。返回是否成功。</summary>
        public bool UnequipBall(int slotIndex)
        {
            if (_slots == null) return false;
            if (InventorySystem.Instance == null || !InventorySystem.Instance.CanAddBall()) return false;
            var ball = _slots.PullFrom(slotIndex);
            return ball != null && InventorySystem.Instance.AddBall(ball);
        }

        public bool ExpandSlots(int delta)
        {
            if (_slots == null || delta <= 0) return false;
            var cfg = BallSystemConfig.Instance;
            int max = cfg != null ? cfg.MaxSlotCount : 8;
            int target = _slots.Capacity + delta;
            if (target > max)
            {
                logError("BallManagementSystem.ExpandSlots: exceeds MaxSlotCount");
                return false;
            }

            _slots.Expand(delta);
            return true;
        }
    }
}
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
    public class BallManagementSystem : PlayerAbility
    {
        [Header("Slot")]
        [Tooltip("发射槽位数（默认 3，可运行时扩容）")]
        public int SlotCount = 3;

        [Tooltip("扩容上限（防越界）")]
        public int MaxSlotCount = 8;

        [Header("Level & Upgrade")]
        [Tooltip("球最大等级（默认 3）")]
        public int DefaultMaxLevel = 3;

        [Tooltip("升级 X 合 1（默认 2）")]
        public int UpgradeCombineCount = 2;

        [Tooltip("升级是否扣金币（默认 0）")]
        public int UpgradeGoldCost;

        [Header("Price")]
        [Tooltip("出售时回收比例，百分数（默认 50%）")]
        [Range(0, 100)]
        public int SellRefundRate = 50;
        
        BallSlotGroup _slots;
        BallUpgradeService _upgrade;
        BallMergeService _merge;
        BallShopService _shop;

        public BallSlotGroup Slots => _slots;
        public BallUpgradeService Upgrade => _upgrade;
        public BallMergeService Merge => _merge;
        public BallShopService Shop => _shop;

        protected override void Initialization()
        {
            base.Initialization();
            int slotCount = Mathf.Max(1, SlotCount);
            _slots = new(slotCount);

            _upgrade = new(this);
            _merge = new(this);
            _shop = new(this);

            // 把当前对玩家生效的 holder 注册到定位器
            InventoryLocate.Clear();
            InventoryLocate.Register(_slots);
            // BallBag 会在 InventorySystem.init() 之后注册进来；这里我们用延迟注册：监听 InventorySystem.OnSystemReady
            InventorySystemReadinessWaiter();

            BallEvents.RaiseSystemReady();
        }

        void OnDestroy()
        {
            InventoryLocate.Unregister(_slots);
            BallEvents.RaiseSystemDestroy();
            _slots = null;
        }

        // BallBag 的注册需要在 InventorySystem.init() 之后；
        // 这里用一次性的事件订阅，确保 BallBag 出现后能注册到定位器。
        void InventorySystemReadinessWaiter()
        {
            if (_player.Inventory.BallBag != null)
            {
                InventoryLocate.Register(_player.Inventory.BallBag);
                return;
            }

            Action<InventorySystem> handler = null;
            handler = s =>
            {
                if (s is { BallBag: not null })
                    InventoryLocate.Register(s.BallBag);
                InventoryEvents.OnSystemReady -= handler;
            };
            InventoryEvents.OnSystemReady += handler;
        }

        // -------- 便利 API（供外部 Command / Action 直接调） --------

        /// <summary>把球装备到指定槽位。返回是否成功。</summary>
        public bool EquipBall(BallInstance ball, int slotIndex)
        {
            if (ball == null || _slots == null) 
                return false;

            // 从背包里拿出（如果还在）
            _player.Inventory.BallBag.Remove(ball);
            return _slots.TryPlaceAt(slotIndex, ball);
        }

        /// <summary>从槽位卸下球到球背包。返回是否成功。</summary>
        public bool UnequipBall(int slotIndex)
        {
            if (_slots == null) 
                return false;

            if (!_player.Inventory.CanAddBall()) 
                return false;

            var ball = _slots.PullFrom(slotIndex);
            return ball != null && _player.Inventory.AddBall(ball);
        }

        public bool ExpandSlots(int delta)
        {
            if (_slots == null || delta <= 0) 
                return false;

            int max = MaxSlotCount;
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
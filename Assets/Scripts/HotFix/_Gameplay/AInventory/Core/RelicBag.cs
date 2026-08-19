using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 遗物背包 —— 容量受 InventorySystemConfig.RelicBagCapacity 控制。
    /// 当前项目里 ARelic 是 abstract partial class，本类用 dynamic 边界以避免强耦合到它的具体子类层级。
    /// 实际放入 RelicBag 的可以是任何继承 ARelic 的对象（需要保证它实现了 IInventoryItem）。
    /// 内部固定数量 RelicInventorySlot,Slot.Item == null 表示空格子。
    /// </summary>
    public sealed class RelicBag : InventoryBag<RelicItem, RelicInventorySlot>
    {
        /// <summary>
        /// 同步维护的 ARelic 镜像集合。
        /// 与背包 slot 中的 RelicItem.UnderlyingRelic 一一对应:
        ///   • Add / AddAt 成功放入 item 时,若 item.UnderlyingRelic != null,加入此处。
        ///   • Remove / RemoveAt / Clear 移除非空 item 时,从此处移除对应 relic。
        ///   • 集合内不允许出现 null(UnderlyingRelic == null 时也不会被加入)。
        /// </summary>
        readonly List<ARelic> relics;

        /// <summary>对外只读访问,供业务层(如 RelicService)遍历当前激活的遗物。</summary>
        public IReadOnlyList<ARelic> Relics => relics;

        public RelicBag(APlayer p, int capacity, int maxCapacity) : base(p, capacity, maxCapacity, "RelicBag")
        {
            relics = new List<ARelic>(MaxCapacity);
        }

        protected override RelicInventorySlot CreateSlot(int index) => new(index);
        protected override ItemKind GetBagKind() => ItemKind.Relic;

        public override void Add(RelicItem item)
        {
            if (item == null)
            {
                logError($"{BagName}: cannot add null");
                return;
            }

            if (!FindEmptySlot(out int idx))
                throw new InventoryFullException(GetBagKind());

            Slots[idx].Set(item);
            RegisterRelic(item);
            RaiseAdded(item);
        }

        public override bool AddAt(int slotIndex, RelicItem item)
        {
            if (item == null)
            {
                logError($"{BagName}: cannot add null");
                return false;
            }
            if (slotIndex < 0 || slotIndex >= Slots.Count)
            {
                logError($"{BagName}: AddAt index out of range {slotIndex}");
                return false;
            }

            var existing = Slots[slotIndex].Item;
            if (existing != null && !ReferenceEquals(existing, item))
            {
                Slots[slotIndex].Set(null);
                UnregisterRelic(existing);
                RaiseRemoved(existing);
            }

            Slots[slotIndex].Set(item);
            RegisterRelic(item);
            RaiseAdded(item);
            return true;
        }

        public override bool Remove(RelicItem item)
        {
            if (item == null)
                return false;

            if (!FindIndex(item, out int idx))
                return false;

            Slots[idx].Set(null);
            UnregisterRelic(item);
            RaiseRemoved(item);
            return true;
        }

        public override bool RemoveAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Slots.Count)
            {
                logError($"{BagName}: RemoveAt index out of range {slotIndex}");
                return false;
            }

            var item = Slots[slotIndex].Item;
            if (item == null)
                return false;

            Slots[slotIndex].Set(null);
            UnregisterRelic(item);
            RaiseRemoved(item);
            return true;
        }

        public override void Clear()
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                var item = Slots[i].Item;
                if (item != null)
                {
                    Slots[i].Set(null);
                    UnregisterRelic(item);
                    RaiseRemoved(item);
                }
            }
        }

        protected override void RaiseAdded(RelicItem item)
        {
            base.RaiseAdded(item);
            
            UnlockTracker.markRelicAsSeen(item.UnderlyingRelic.relicId);
        }

        protected override void RaiseRemoved(RelicItem item)
        {
            base.RaiseRemoved(item);
            
            RelicItem.Release(ref item);
        }

        /// <summary>把 item.UnderlyingRelic 加入镜像集合(忽略 null 与重复)。</summary>
        void RegisterRelic(RelicItem item)
        {
            var r = item != null ? item.UnderlyingRelic : null;
            if (r == null) 
                return;

            if (!relics.Contains(r))
            {
                relics.Add(r);
                r.onEquip(_player);
            }
        }

        /// <summary>从镜像集合移除与该 item 关联的 relic。</summary>
        void UnregisterRelic(RelicItem item)
        {
            var r = item != null ? item.UnderlyingRelic : null;
            if (r == null) 
                return;

            r.onUnequip(_player);
            relics.Remove(r);
        }
    }
}

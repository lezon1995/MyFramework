using System;

namespace MoreMountains
{
    /// <summary>
    /// 单个发射槽位。
    /// Current == null 表示空。
    /// 装备 / 卸下都通过这里，外部不能直接动 Current。
    /// </summary>
    public sealed class BallSlot : IInventorySlot<BallItem>
    {
        public int Index { get; }
        public BallItem Item { get; set; }
        public bool IsEmpty => Item == null;
        public bool IsOccupied => Item != null;

        public event Action<IInventorySlot<BallItem>> OnSlotChanged;

        public BallSlot(int index)
        {
            Index = index;
        }
        
        public BallItem Set(BallItem item)
        {
            if (ReferenceEquals(Item, item))
                return Item;

            BallItem previous = Item;
            Item = item;
            OnSlotChanged?.Invoke(this);
            return previous;
        }

        public bool TrySet(BallItem ball)
        {
            if (!IsEmpty)
                return false;

            Item = ball;
            OnSlotChanged?.Invoke(this);
            BallEvents.RaiseEquipped(ball, Index);
            return true;
        }

        public bool Replace(BallItem ball)
        {
            var old = Item;
            Item = ball;
            OnSlotChanged?.Invoke(this);
            if (old != null)
                BallEvents.RaiseUnequipped(old, Index);

            if (ball != null)
                BallEvents.RaiseEquipped(ball, Index);

            return true;
        }

        public BallItem Clear()
        {
            var old = Item;
            Item = null;
            OnSlotChanged?.Invoke(this);
            if (old != null)
                BallEvents.RaiseUnequipped(old, Index);

            return old;
        }
    }
}
using System;

namespace MoreMountains
{
    /// <summary>
    /// 球背包中的一个格子。固定数量的格子由 BallBag 一上来就生成 N 个，
    /// 玩家获得 / 失去球时只是把 Item 在 slot 之间挪进挪出。
    /// </summary>
    public sealed class BallInventorySlot : IInventorySlot<BallItem>
    {
        public int Index { get; }
        public BallItem Item { get; private set; }
        public bool IsEmpty => Item == null;
        public bool IsOccupied => Item != null;

        public event Action<IInventorySlot<BallItem>> OnSlotChanged;

        public BallInventorySlot(int index)
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

        public bool TrySet(BallItem item)
        {
            if (IsOccupied)
                return false;

            Item = item;
            OnSlotChanged?.Invoke(this);
            return true;
        }

        public override string ToString()
        {
            return $"BallSlot#{Index}({(IsEmpty ? "empty" : Item?.ToString())})";
        }
    }
}
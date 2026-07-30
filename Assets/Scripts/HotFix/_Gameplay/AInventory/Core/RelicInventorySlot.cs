using System;

namespace MoreMountains
{
    /// <summary>
    /// 遗物背包中的一个格子。固定数量的格子由 RelicBag 一上来就生成 N 个。
    /// </summary>
    public sealed class RelicInventorySlot : IInventorySlot<RelicItem>
    {
        public int Index { get; }
        public RelicItem Item { get; private set; }
        public bool IsEmpty => Item == null;
        public bool IsOccupied => Item != null;

        public event Action<IInventorySlot<RelicItem>> OnSlotChanged;

        public RelicInventorySlot(int index)
        {
            Index = index;
        }

        public RelicItem Set(RelicItem item)
        {
            if (ReferenceEquals(Item, item))
                return Item;

            RelicItem previous = Item;
            Item = item;
            OnSlotChanged?.Invoke(this);
            return previous;
        }

        public override string ToString()
        {
            return $"RelicSlot#{Index}({(IsEmpty ? "empty" : Item?.ToString())})";
        }
    }
}
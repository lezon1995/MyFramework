using System;

namespace MoreMountains
{
    /// <summary>
    /// 背包中"一个格子"的抽象：固定结构、随时可空。
    /// 当 <see cref="Item"/> 为 null 时表示该格子为空；UI 走格子视图时按 Item 是否为 null 决定显示状态。
    /// </summary>
    public interface IInventorySlot<T> where T : class, IInventoryItem
    {
        int Index { get; }
        T Item { get; }
        bool IsEmpty { get; }
        bool IsOccupied { get; }

        /// <summary>设置 Item；返回该 slot 原先的 Item（null 表示原来空着）。</summary>
        T Set(T item);
        bool TrySet(T item);

        event Action<IInventorySlot<T>> OnSlotChanged;
    }
}
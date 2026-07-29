using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 背包格子集合 —— 通用、带容量上限、可扩容。
    /// 球背包与遗物背包都基于它。
    /// </summary>
    public abstract class InventoryBag<T> : IInventoryHolder<T> where T : class, IInventoryItem
    {
        protected List<T> Items;
        protected int CapacityValue;

        public string BagName { get; }
        public int MaxCapacity { get; }

        public List<T> AllItems => Items;

        public int Count => Items.Count;

        public int FreeSlots => Math.Max(0, CapacityValue - Items.Count);

        public bool IsFull => Items.Count >= CapacityValue;

        public event Action<T> OnItemAdded;
        public event Action<T> OnItemRemoved;
        public event Action OnBagChanged;

        protected InventoryBag(int capacity, int maxCapacity, string bagName)
        {
            BagName = bagName;
            CapacityValue = Math.Max(0, capacity);
            MaxCapacity = Math.Max(CapacityValue, maxCapacity);
            Items = new List<T>(MaxCapacity);
        }

        public int Capacity => CapacityValue;

        public virtual bool CanAdd(T item = null)
        {
            return !IsFull;
        }

        /// <summary>
        /// 默认追加到末尾。容量满抛 InventoryFullException。
        /// </summary>
        public virtual void Add(T item)
        {
            if (item == null)
            {
                logError($"{BagName}: cannot add null");
                return;
            }

            if (IsFull)
                throw new InventoryFullException(GetBagKind());
            Items.Add(item);
            RaiseAdded(item);
        }

        /// <summary>
        /// 插入到指定位置，原内容挤到末尾。
        /// </summary>
        public virtual void AddAt(int index, T item)
        {
            if (item == null)
            {
                logError($"{BagName}: cannot add null");
                return;
            }

            if (IsFull)
                throw new InventoryFullException(GetBagKind());
            if (index < 0 || index > Items.Count)
            {
                logError($"{BagName}: AddAt index out of range {index}");
                return;
            }

            Items.Insert(index, item);
            RaiseAdded(item);
        }

        public virtual bool Remove(T item)
        {
            if (item == null)
                return false;

            int idx = Items.IndexOf(item);
            return RemoveAt(idx);
        }

        public virtual bool RemoveAt(int index)
        {
            if (index < 0 || index >= Items.Count)
            {
                logError($"{BagName}: RemoveAt index out of range {index}");
                return false;
            }

            T removed = Items[index];
            Items.RemoveAt(index);
            RaiseRemoved(removed);
            return true;
        }

        public virtual void Swap(int a, int b)
        {
            if (a < 0 || a >= Items.Count || b < 0 || b >= Items.Count || a == b)
                return;

            (Items[a], Items[b]) = (Items[b], Items[a]);
            OnBagChanged?.Invoke();
        }

        /// <summary>扩容。要求总容量不超过 MaxCapacity。</summary>
        public virtual void Expand(int delta)
        {
            if (delta <= 0)
                return;

            int target = CapacityValue + delta;
            if (target > MaxCapacity)
                throw new InventoryExpansionLimitException(BagName, target, MaxCapacity);
            CapacityValue = target;
            OnBagChanged?.Invoke();
        }

        /// <summary>缩容。要求尾部空位 ≥ delta，否则抛 InventoryShrinkInvalidException。</summary>
        public virtual void Shrink(int delta)
        {
            if (delta <= 0)
                return;

            int available = FreeSlots;
            if (available < delta)
                throw new InventoryShrinkInvalidException(BagName, delta, available);

            CapacityValue -= delta;
            OnBagChanged?.Invoke();
        }

        public virtual void Clear()
        {
            if (Items.Count == 0)
                return;

            Items.Clear();
            OnBagChanged?.Invoke();
        }

        protected abstract ItemKind GetBagKind();

        protected void RaiseAdded(T item)
        {
            OnItemAdded?.Invoke(item);
            OnBagChanged?.Invoke();
        }

        protected void RaiseRemoved(T item)
        {
            OnItemRemoved?.Invoke(item);
            OnBagChanged?.Invoke();
        }

        // ---- 实现 IInventoryHolder 所需：供其它系统增删 ----

        public bool TryRemoveByInstance(T item) => Remove(item);

        public bool TryInsert(T item)
        {
            try
            {
                Add(item);
                return true;
            }
            catch (InventoryFullException)
            {
                return false;
            }
        }

        public bool FindIndex(T item, out int index)
        {
            index = Items.IndexOf(item);
            if (index < 0)
                return false;

            return true;
        }

        public string Name => BagName;
    }

    public interface IInventoryHolder
    {
        string Name { get; }
    }

    /// <summary>
    /// 任何"能装东西"的容器都实现这个接口，
    /// 让球管理系统 / 升级服务只面对接口，不感知是背包还是槽位。
    /// </summary>
    public interface IInventoryHolder<in T> : IInventoryHolder where T : IInventoryItem
    {
        bool TryRemoveByInstance(T item);
        bool TryInsert(T item);
        bool FindIndex(T item, out int index);
    }
}
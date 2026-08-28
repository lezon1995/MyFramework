using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 背包格子集合 —— 通用、带容量上限、可扩容。
    ///
    /// 数据模型（重构后）：
    ///   • 一上来就生成 Capacity 个固定 slot（slot.Item 可能是 null）。
    ///   • Bag 永远有 N 个 slot,不会因为没装满而"缩短"。
    ///   • Add：找第一个 IsEmpty 的 slot 填入；找不到抛 InventoryFullException。
    ///   • Remove：通过 Item 找到所在 slot 并 Set(null)。
    ///   • Expand：往尾部追加新 slot（默认 Item=null）。
    ///
    /// 对外 API：
    ///   • SlotList：所有 slot 列表（长度 == 当前 Capacity,含空）。
    ///   • AllItems：仅非空 slot 里的 Item 列表（旧调用方按"已装的 item"遍历）。
    ///   • Count：非空 slot 数。
    ///
    /// 事件：
    ///   • OnItemAdded / OnItemRemoved：item 粒度,业务系统订阅。
    ///   • OnSlotChanged(slot)：slot 粒度,UI 可精确定位变更。
    ///   • OnBagChanged：粒度最粗,UI 整体 Rebuild 用。
    /// </summary>
    public abstract class InventoryBag<TItem, TSlot> : IInventoryHolder<TItem>
        where TItem : class, IInventoryItem
        where TSlot : IInventorySlot<TItem>
    {
        protected APlayer _player;
        protected List<TSlot> Slots;
        protected int CapacityValue;

        public string BagName { get; }
        public int MaxCapacity { get; }

        /// <summary>所有 slot（长度 == 当前 Capacity,空 slot 的 Item == null）。</summary>
        public List<TSlot> SlotList => Slots;

        /// <summary>仅非空 slot 中的 item（供旧调用方按"已装 item"遍历）。</summary>
        public List<TItem> AllItems
        {
            get
            {
                var list = new List<TItem>(Slots.Count);
                for (int i = 0; i < Slots.Count; i++)
                {
                    var item = Slots[i].Item;
                    if (item != null)
                        list.Add(item);
                }

                return list;
            }
        }

        public int Count
        {
            get
            {
                int n = 0;
                for (int i = 0; i < Slots.Count; i++)
                    if (Slots[i].Item != null)
                        n++;
                return n;
            }
        }

        public int FreeSlots => Math.Max(0, CapacityValue - Count);

        public bool IsFull => Count >= CapacityValue;

        public int Capacity => CapacityValue;

        public event Action<TItem> OnItemAdded;
        public event Action<TItem> OnItemRemoved;
        public Action<TSlot> OnSlotChanged;
        public event Action OnBagChanged;

        protected InventoryBag(APlayer p, int capacity, int maxCapacity, string bagName)
        {
            _player = p;
            BagName = bagName;
            CapacityValue = Math.Max(0, capacity);
            MaxCapacity = Math.Max(CapacityValue, maxCapacity);
            Slots = new(MaxCapacity);
            for (int i = 0; i < CapacityValue; i++)
            {
                var s = CreateSlot(i);
                s.OnSlotChanged += RaiseSlotChanged;
                Slots.Add(s);
            }
        }

        /// <summary>子类决定如何实例化一个 slot。</summary>
        protected abstract TSlot CreateSlot(int index);

        public virtual bool CanAdd(TItem item = null)
        {
            return !IsFull;
        }

        /// <summary>
        /// 默认追加到第一个空 slot。容量满抛 InventoryFullException。
        /// </summary>
        public virtual void Add(TItem item)
        {
            if (item == null)
            {
                logError($"{BagName}: cannot add null");
                return;
            }

            if (!FindEmptySlot(out int idx))
                throw new InventoryFullException(GetBagKind());

            Slots[idx].Set(item);
            RaiseAdded(item);
        }

        /// <summary>
        /// 放到指定 slot 索引。如果该 slot 已占用则覆盖原 item（覆盖前的 item 不会自动塞回别处）。
        /// </summary>
        public virtual bool AddAt(int slotIndex, TItem item)
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
                RaiseRemoved(existing);
            }

            Slots[slotIndex].Set(item);
            RaiseAdded(item);
            return true;
        }

        public virtual bool Remove(TItem item)
        {
            if (item == null)
                return false;

            if (!FindIndex(item, out int idx))
                return false;

            Slots[idx].Set(null);
            RaiseRemoved(item);
            return true;
        }

        public virtual bool RemoveAt(int slotIndex)
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
            RaiseRemoved(item);
            return true;
        }

        public virtual bool Swap(int a, int b)
        {
            if (a < 0 || a >= Slots.Count || b < 0 || b >= Slots.Count || a == b)
                return false;

            var tmp = Slots[a].Item;
            Slots[a].Set(Slots[b].Item);
            Slots[b].Set(tmp);
            OnBagChanged?.Invoke();
            return true;
        }

        /// <summary>扩容。往尾部追加新 slot。</summary>
        public virtual void Expand(int delta)
        {
            if (delta <= 0)
                return;

            int target = CapacityValue + delta;
            if (target > MaxCapacity)
                throw new InventoryExpansionLimitException(BagName, target, MaxCapacity);

            for (int i = 0; i < delta; i++)
            {
                var s = CreateSlot(CapacityValue + i);
                s.OnSlotChanged += RaiseSlotChanged;
                Slots.Add(s);
            }

            CapacityValue = target;
            OnBagChanged?.Invoke();
        }

        /// <summary>缩容。要求尾部 delta 个 slot 都是空。</summary>
        public virtual void Shrink(int delta)
        {
            if (delta <= 0)
                return;

            for (int i = Slots.Count - delta; i < Slots.Count; i++)
            {
                if (Slots[i].Item != null)
                    throw new InventoryShrinkInvalidException(BagName, delta, FreeSlots);
            }

            for (int i = 0; i < delta; i++)
            {
                var last = Slots[^1];
                last.OnSlotChanged -= RaiseSlotChanged;
                Slots.RemoveAt(Slots.Count - 1);
            }

            CapacityValue -= delta;
            OnBagChanged?.Invoke();
        }

        public virtual void Clear()
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                var item = Slots[i].Item;
                if (item != null)
                {
                    Slots[i].Set(null);
                    RaiseRemoved(item);
                }
            }
        }

        protected abstract ItemKind GetBagKind();

        protected bool FindEmptySlot(out int index)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].IsEmpty)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        protected void RaiseSlotChanged(IInventorySlot<TItem> slot)
        {
            OnSlotChanged?.Invoke((TSlot)slot);
        }

        protected virtual void RaiseAdded(TItem item)
        {
            OnItemAdded?.Invoke(item);
            OnBagChanged?.Invoke();
        }

        protected virtual void RaiseRemoved(TItem item)
        {
            OnItemRemoved?.Invoke(item);
            OnBagChanged?.Invoke();
        }

        // ---- 实现 IInventoryHolder 所需：供其它系统增删 ----

        public bool TryRemoveByItem(TItem item)
        {
            return Remove(item);
        }

        public bool TryInsert(TItem item)
        {
            if (item == null)
                return false;

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

        public bool TryInsertAt(TItem item, int index)
        {
            if (item == null)
                return false;

            try
            {
                if (Slots[index].TrySet(item))
                {
                    RaiseAdded(item);
                    return true;
                }

                return false;
            }
            catch (InventoryFullException)
            {
                return false;
            }
        }

        public bool FindIndex(TItem item, out int index)
        {
            index = -1;
            if (item == null)
                return false;

            for (int i = 0; i < Slots.Count; i++)
            {
                if (ReferenceEquals(Slots[i].Item, item))
                {
                    index = i;
                    return true;
                }
            }

            return false;
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
        bool TryRemoveByItem(T item);
        bool TryInsert(T item);
        bool TryInsertAt(T item, int index);
        bool FindIndex(T item, out int index);
    }
}
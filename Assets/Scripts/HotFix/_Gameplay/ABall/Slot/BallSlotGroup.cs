using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 玩家全部发射槽位的集合。
    /// 默认 3 个，可运行时扩容（接口 Expand()）。
    /// 实现 IInventoryHolder：升级 / 融合流程不感知它，只看到接口。
    /// </summary>
    public sealed class BallSlotGroup : IInventoryHolder<BallItem>
    {
        List<BallSlot> _slots;

        public string Name => "SlotGroup";
        public int Capacity => _slots.Count;
        public List<BallSlot> Slots => _slots;

        public int OccupiedCount
        {
            get
            {
                int n = 0;
                foreach (var s in _slots)
                    if (!s.IsEmpty)
                        n++;
                return n;
            }
        }

        public int FreeSlotCount => Capacity - OccupiedCount;

        public event Action OnSlotsChanged;

        public BallSlotGroup(int initialCapacity)
        {
            _slots = new(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
                _slots.Add(new(i));
        }

        public BallSlot GetSlot(int index)
        {
            return (index >= 0 && index < _slots.Count) ? _slots[index] : null;
        }

        /// <summary>找到第一个空槽位；找不到返回 -1。</summary>
        public bool FindEmptySlotIndex(out int index)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        /// <summary>尝试把球装备到指定槽位。占用中或越界返回 false。</summary>
        public bool TryPlaceAt(int slotIndex, BallItem ball)
        {
            var slot = GetSlot(slotIndex);
            if (slot == null)
                return false;

            if (!slot.TrySet(ball))
                return false;

            OnSlotsChanged?.Invoke();
            return true;
        }

        /// <summary>装备到第一个空槽位。返回 -1 表示失败。</summary>
        public bool TryPlaceFirstEmpty(BallItem ball, out int index)
        {
            if (FindEmptySlotIndex(out index))
            {
                _slots[index].TrySet(ball);
                OnSlotsChanged?.Invoke();
                return true;
            }

            return false;
        }

        public BallItem PullFrom(int slotIndex)
        {
            var slot = GetSlot(slotIndex);
            if (slot == null)
                return null;

            var b = slot.Clear();
            OnSlotsChanged?.Invoke();
            return b;
        }

        public bool MoveTo(int src, int dst)
        {
            var sSrc = GetSlot(src);
            var sDst = GetSlot(dst);
            if (sSrc == null || sDst == null)
                return false;

            if (sSrc.IsEmpty || !sDst.IsEmpty)
                return false;

            var ball = sSrc.Clear();
            sDst.TrySet(ball);
            OnSlotsChanged?.Invoke();
            return true;
        }

        public bool Swap(int a, int b)
        {
            var sA = GetSlot(a);
            var sB = GetSlot(b);
            if (sA == null || sB == null)
                return false;

            (sA.Current, sB.Current) = (sB.Current, sA.Current);
            OnSlotsChanged?.Invoke();
            return true;
        }

        /// <summary>替换指定索引位置的球。会触发 BallSlot 的 OnSlotChanged 与相应的 Equipped/Unequipped 事件。</summary>
        public bool ReplaceAt(int slotIndex, BallItem ball)
        {
            var slot = GetSlot(slotIndex);
            if (slot == null)
                return false;

            slot.Replace(ball);
            OnSlotsChanged?.Invoke();
            return true;
        }

        public BallItem FindBall(BallItem ball)
        {
            if (ball == null)
                return null;

            foreach (var s in _slots)
            {
                if (ReferenceEquals(s.Current, ball))
                    return ball;
            }

            return null;
        }

        /// <summary>扩容：往末尾追加新槽位。</summary>
        public void Expand(int delta)
        {
            if (delta <= 0)
                return;

            int baseCount = _slots.Count;
            for (int i = 0; i < delta; i++)
                _slots.Add(new(baseCount + i));

            OnSlotsChanged?.Invoke();
        }

        // -------- IInventoryHolder --------

        public bool TryRemoveByInstance(BallItem item)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (ReferenceEquals(_slots[i].Current, item))
                {
                    _slots[i].Clear();
                    OnSlotsChanged?.Invoke();
                    return true;
                }
            }

            return false;
        }

        public bool TryInsert(BallItem item)
        {
            return TryPlaceFirstEmpty(item, out _);
        }

        public bool FindIndex(BallItem item, out int index)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (ReferenceEquals(_slots[i].Current, item))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }
    }
}
using System;

namespace MoreMountains
{
    /// <summary>
    /// 单个发射槽位。
    /// Current == null 表示空。
    /// 装备 / 卸下都通过这里，外部不能直接动 Current。
    /// </summary>
    public sealed class BallSlot
    {
        public int Index { get; }
        public BallInstance Current { get; set; }
        public bool IsEmpty => Current == null;

        public event Action<BallSlot> OnSlotChanged;

        public BallSlot(int index)
        {
            Index = index;
        }

        public bool TrySet(BallInstance ball)
        {
            if (!IsEmpty)
                return false;

            Current = ball;
            OnSlotChanged?.Invoke(this);
            BallEvents.RaiseEquipped(ball, Index);
            return true;
        }

        public bool Replace(BallInstance ball)
        {
            var old = Current;
            Current = ball;
            OnSlotChanged?.Invoke(this);
            if (old != null)
                BallEvents.RaiseUnequipped(old, Index);

            if (ball != null)
                BallEvents.RaiseEquipped(ball, Index);

            return true;
        }

        public BallInstance Clear()
        {
            var old = Current;
            Current = null;
            OnSlotChanged?.Invoke(this);
            if (old != null)
                BallEvents.RaiseUnequipped(old, Index);

            return old;
        }
    }
}
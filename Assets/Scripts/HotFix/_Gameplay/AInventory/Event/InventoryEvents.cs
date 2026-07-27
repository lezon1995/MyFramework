using System;

namespace MoreMountains
{
    /// <summary>
    /// 背包系统事件总线。Bag 增删 → 触发单条事件；任意变更也会发 "Changed"。
    /// 玩家钱包/球管理/商店/存档 都订阅这里。
    /// </summary>
    public static class InventoryEvents
    {
        // BallBag
        public static event Action<BallItem> OnBallAdded;
        public static event Action<BallItem> OnBallRemoved;
        public static event Action OnBallBagChanged;

        // RelicBag
        public static event Action<RelicItem> OnRelicAdded;
        public static event Action<RelicItem> OnRelicRemoved;
        public static event Action OnRelicBagChanged;

        // System lifecycle
        public static event Action<InventorySystem> OnSystemReady;
        public static event Action<InventorySystem> OnSystemDestroy;

        // ---- 内部分发 ----
        internal static void RaiseBallAdded(BallItem b)
        {
            OnBallAdded?.Invoke(b);
            OnBallBagChanged?.Invoke();
        }

        internal static void RaiseBallRemoved(BallItem b)
        {
            OnBallRemoved?.Invoke(b);
            OnBallBagChanged?.Invoke();
        }

        internal static void RaiseBallBagChanged()
        {
            OnBallBagChanged?.Invoke();
        }

        internal static void RaiseRelicAdded(RelicItem r)
        {
            OnRelicAdded?.Invoke(r);
            OnRelicBagChanged?.Invoke();
        }

        internal static void RaiseRelicRemoved(RelicItem r)
        {
            OnRelicRemoved?.Invoke(r);
            OnRelicBagChanged?.Invoke();
        }

        internal static void RaiseRelicBagChanged()
        {
            OnRelicBagChanged?.Invoke();
        }

        internal static void RaiseSystemReady(InventorySystem s)
        {
            OnSystemReady?.Invoke(s);
        }

        internal static void RaiseSystemDestroy(InventorySystem s)
        {
            OnSystemDestroy?.Invoke(s);
        }
    }
}
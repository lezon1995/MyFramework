using System;

namespace MoreMountains
{
    /// <summary>
    /// 球系统事件总线。
    /// UI / 商店 / 钱包 等都订阅这里，而不是直接访问 BallManagementSystem。
    /// </summary>
    public static class BallEvents
    {
        public static event Action<BallItem> OnBallCreated;
        public static event Action<BallItem> OnBallDestroyed;

        public static event Action<BallItem, int /*slotIdx*/> OnBallEquipped;
        public static event Action<BallItem, int /*slotIdx*/> OnBallUnequipped;

        public static event Action<BallItem /*from*/, BallItem /*to*/> OnBallUpgraded;
        public static event Action<BallItem /*a*/, BallItem /*b*/, BallItem /*merged*/> OnBallMerged;

        public static event Action<BallItem> OnBallPurchased;
        public static event Action<BallItem, int /*goldRefund*/> OnBallSold;

        public static event Action OnSystemReady;
        public static event Action OnSystemDestroy;

        internal static void RaiseCreated(BallItem b) => OnBallCreated?.Invoke(b);
        internal static void RaiseDestroyed(BallItem b) => OnBallDestroyed?.Invoke(b);

        internal static void RaiseEquipped(BallItem b, int slot) => OnBallEquipped?.Invoke(b, slot);
        internal static void RaiseUnequipped(BallItem b, int slot) => OnBallUnequipped?.Invoke(b, slot);

        internal static void RaiseUpgraded(BallItem from, BallItem to) => OnBallUpgraded?.Invoke(from, to);
        internal static void RaiseMerged(BallItem a, BallItem b, BallItem m) => OnBallMerged?.Invoke(a, b, m);

        internal static void RaisePurchased(BallItem b) => OnBallPurchased?.Invoke(b);
        internal static void RaiseSold(BallItem b, int gold) => OnBallSold?.Invoke(b, gold);

        internal static void RaiseSystemReady() => OnSystemReady?.Invoke();
        internal static void RaiseSystemDestroy() => OnSystemDestroy?.Invoke();
    }
}
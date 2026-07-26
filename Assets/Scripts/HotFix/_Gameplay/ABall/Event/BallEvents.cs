using System;

namespace MoreMountains
{
    /// <summary>
    /// 球系统事件总线。
    /// UI / 商店 / 钱包 等都订阅这里，而不是直接访问 BallManagementSystem。
    /// </summary>
    public static class BallEvents
    {
        public static event Action<BallInstance> OnBallCreated;
        public static event Action<BallInstance> OnBallDestroyed;

        public static event Action<BallInstance, int /*slotIdx*/> OnBallEquipped;
        public static event Action<BallInstance, int /*slotIdx*/> OnBallUnequipped;

        public static event Action<BallInstance /*from*/, BallInstance /*to*/> OnBallUpgraded;
        public static event Action<BallInstance /*a*/, BallInstance /*b*/, BallInstance /*merged*/> OnBallMerged;

        public static event Action<BallInstance> OnBallPurchased;
        public static event Action<BallInstance, int /*goldRefund*/> OnBallSold;

        public static event Action OnSystemReady;
        public static event Action OnSystemDestroy;

        internal static void RaiseCreated(BallInstance b) => OnBallCreated?.Invoke(b);
        internal static void RaiseDestroyed(BallInstance b) => OnBallDestroyed?.Invoke(b);

        internal static void RaiseEquipped(BallInstance b, int slot) => OnBallEquipped?.Invoke(b, slot);
        internal static void RaiseUnequipped(BallInstance b, int slot) => OnBallUnequipped?.Invoke(b, slot);

        internal static void RaiseUpgraded(BallInstance from, BallInstance to) => OnBallUpgraded?.Invoke(from, to);
        internal static void RaiseMerged(BallInstance a, BallInstance b, BallInstance m) => OnBallMerged?.Invoke(a, b, m);

        internal static void RaisePurchased(BallInstance b) => OnBallPurchased?.Invoke(b);
        internal static void RaiseSold(BallInstance b, int gold) => OnBallSold?.Invoke(b, gold);

        internal static void RaiseSystemReady() => OnSystemReady?.Invoke();
        internal static void RaiseSystemDestroy() => OnSystemDestroy?.Invoke();
    }
}
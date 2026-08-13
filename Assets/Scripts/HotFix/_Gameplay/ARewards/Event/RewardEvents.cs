using System;

namespace MoreMountains
{
    /// <summary>
    /// 升级奖励系统事件总线。
    /// </summary>
    public static class RewardEvents
    {
        public static event Action OnRewardOpened;
        public static event Action OnRewardClosed;
        public static event Action<RewardBoardKind> OnBoardOpened;
        public static event Action<RewardBoardKind> OnBoardRerolled;
        public static event Action<IPurchasable> OnOfferSold;
        public static event Action<IInventoryItem> OnSoldFromBag;
        public static event Action OnSystemReady;
        public static event Action OnSystemDestroy;

        internal static void RaiseRewardOpened() => OnRewardOpened?.Invoke();
        internal static void RaiseRewardClosed() => OnRewardClosed?.Invoke();
        internal static void RaiseBoardOpened(RewardBoardKind k) => OnBoardOpened?.Invoke(k);
        internal static void RaiseBoardRerolled(RewardBoardKind k) => OnBoardRerolled?.Invoke(k);
        internal static void RaiseOfferSold(IPurchasable o) => OnOfferSold?.Invoke(o);
        internal static void RaiseSoldFromBag(IInventoryItem it) => OnSoldFromBag?.Invoke(it);

        internal static void RaiseSystemReady() => OnSystemReady?.Invoke();
        internal static void RaiseSystemDestroy() => OnSystemDestroy?.Invoke();
    }
}
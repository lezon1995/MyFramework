using System;
using static FrameBaseUtility;

namespace MoreMountains
{
    /// <summary>
    /// 商店系统事件总线。
    /// </summary>
    public static class ShopEvents
    {
        public static event Action                                         OnShopOpened;
        public static event Action                                         OnShopClosed;
        public static event Action<ShopBoardKind>                          OnBoardOpened;
        public static event Action<ShopBoardKind>                          OnBoardRerolled;
        public static event Action<IPurchasable>                           OnOfferSold;
        public static event Action<IInventoryItem>                         OnSoldFromBag;
        public static event Action                                         OnSystemReady;
        public static event Action                                         OnSystemDestroy;
        public static event Action<int /*gold*/, string /*reason*/>        OnGoldSpent;
        public static event Action<int /*gold*/, string /*reason*/>        OnGoldEarned;

        internal static void RaiseShopOpened()    => OnShopOpened?.Invoke();
        internal static void RaiseShopClosed()    => OnShopClosed?.Invoke();
        internal static void RaiseBoardOpened(ShopBoardKind k)    => OnBoardOpened?.Invoke(k);
        internal static void RaiseBoardRerolled(ShopBoardKind k)  => OnBoardRerolled?.Invoke(k);
        internal static void RaiseOfferSold(IPurchasable o)        => OnOfferSold?.Invoke(o);
        internal static void RaiseSoldFromBag(IInventoryItem it)  => OnSoldFromBag?.Invoke(it);

        internal static void RaiseSystemReady()   => OnSystemReady?.Invoke();
        internal static void RaiseSystemDestroy() => OnSystemDestroy?.Invoke();

        internal static void RaiseGoldSpent (int n, string reason) => OnGoldSpent ?.Invoke(n, reason);
        internal static void RaiseGoldEarned(int n, string reason) => OnGoldEarned?.Invoke(n, reason);
    }
}

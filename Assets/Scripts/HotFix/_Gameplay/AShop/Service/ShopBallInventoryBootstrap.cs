using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 一站式注册 helper —— 在 GameHotFix.initFrameSystem 中
    /// 调用 ShopBallInventoryBootstrap.RegisterAll() 即可。
    ///
    /// 注册顺序很关键：
    ///   1) PlayerWallet          → 金币出入库最先
    ///   2) InventorySystemConfig → SO 自动 Resources 找
    ///   3) BallDefLibrary        → 球 def 注册要先
    ///   4) InventorySystem       → BallBag 才会被注册到 InventoryLocate
    ///   5) BallManagementSystem  → BallSlotGroup 注册到 InventoryLocate（监听 InventoryEvents 自动追加 BallBag）
    ///   6) ShopSystem
    /// </summary>
    public static class ShopBallInventoryBootstrap
    {
        public static void RegisterAll(
            Action<object /*PlayerWallet*/> setWallet = null,
            Action<object /*BallDefLibrary*/> setLibrary = null,
            Action<object /*InventorySystem*/> setInventory = null,
            Action<object /*BallManagementSystem*/> setBalls = null,
            Action<object /*ShopSystem*/> setShop = null,
            IEnumerable<BallDef> ballDefs = null,
            Func<int, ARelic> relicFactory = null)
        {
            // var wallet = new PlayerWallet();
            // wallet.init();
            // setWallet?.Invoke(wallet);
            // var lib = new BallDefLibrary();
            // lib.init();
            // if (ballDefs != null) 
            //     lib.RegisterAll(ballDefs as BallDef[] ?? new List<BallDef>(ballDefs).ToArray());

            // setLibrary?.Invoke(lib);

            // var inv = new InventorySystem();
            // inv.init();
            // setInventory?.Invoke(inv);

            // var balls = new BallManagementSystem();
            // balls.init();
            // setBalls?.Invoke(balls);

            // var shop = new ShopSystem();
            // shop.init();
            // setShop?.Invoke(shop);
        }
    }
}
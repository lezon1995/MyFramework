using UnityEngine;
using static FrameBaseUtility;

namespace MoreMountains
{
    /// <summary>
    /// 商店系统 FrameSystem（主入口）。
    /// 让 WaveSystem 等通过 Instance 进入。
    /// 不在 init() 中自动开 Shop，它是被外部 EnterShop() 触发的。
    /// </summary>
    public sealed class ShopSystem : FrameSystem
    {
        public static ShopSystem Instance { get; private set; }

        ShopController _ctrl;

        public ShopController Controller => _ctrl;

        public override void init()
        {
            base.init();
            Instance = this;
            _ctrl = new ShopController();
            ShopEvents.RaiseSystemReady();
        }

        public override void willDestroy()
        {
            base.willDestroy();
            ShopEvents.RaiseSystemDestroy();
            if (Instance == this) Instance = null;
            _ctrl = null;
        }

        public void EnterShop()
        {
            if (_ctrl == null)
            {
                logError("ShopSystem: Controller not initialized");
                return;
            }
            _ctrl.EnterShop();
        }
    }
}

namespace MoreMountains
{
    /// <summary>
    /// 商店系统 FrameSystem（主入口）。
    /// 让 WaveSystem 等通过 Instance 进入。
    /// 不在 init() 中自动开 Shop，它是被外部 EnterShop() 触发的。
    /// </summary>
    public sealed class ShopSystem : PlayerAbility
    {
        ShopController _ctrl;

        public ShopController Controller => _ctrl;

        protected override void Initialization()
        {
            base.Initialization();
            _ctrl = new ShopController(this, null);
            ShopEvents.RaiseSystemReady();
        }

        void OnDestroy()
        {
            ShopEvents.RaiseSystemDestroy();
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
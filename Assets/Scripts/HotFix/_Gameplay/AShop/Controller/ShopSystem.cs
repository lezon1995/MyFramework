namespace MoreMountains
{
    /// <summary>
    /// 商店系统组件 —— 继承自 PlayerAbility（基类自动赋值 _player）。
    /// 在 APlayer 上 AddComponent；外部通过 _player.Shop 访问。
    /// 不在 Initialization 中自动开 Shop，由外部 EnterShop() 触发。
    /// </summary>
    public sealed class ShopSystem : PlayerAbility
    {
        ShopController _ctrl;

        public ShopController Controller => _ctrl;

        bool _systemReadyRaised;

        protected override void Initialization()
        {
            base.Initialization();
            _ctrl = new ShopController(this, null);

            if (!_systemReadyRaised)
            {
                _systemReadyRaised = true;
                ShopEvents.RaiseSystemReady();
            }
        }

        protected override void OnDestroy()
        {
            if (_systemReadyRaised)
            {
                _systemReadyRaised = false;
                ShopEvents.RaiseSystemDestroy();
            }
            _ctrl = null;
        }

        public void EnterShop(int waveNumber)
        {
            if (_ctrl == null)
            {
                logError("ShopSystem: Controller not initialized");
                return;
            }

            _ctrl.EnterShop(waveNumber);
        }
    }
}
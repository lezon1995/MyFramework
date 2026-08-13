namespace MoreMountains
{
    /// <summary>
    /// 商店系统组件 —— 继承自 PlayerAbility（基类自动赋值 _player）。
    /// 在 APlayer 上 AddComponent；外部通过 _player.RewardSystem 访问。
    /// 不在 Initialization 中自动开 Reward，由外部 EnterReward() 触发。
    /// </summary>
    public sealed class RewardSystem : PlayerAbility
    {
        RewardController _ctrl;

        public RewardController Controller => _ctrl;

        bool _systemReadyRaised;

        protected override void Initialization()
        {
            base.Initialization();
            _ctrl = new(this, null);

            if (!_systemReadyRaised)
            {
                _systemReadyRaised = true;
                RewardEvents.RaiseSystemReady();
            }
        }

        protected override void OnDestroy()
        {
            if (_systemReadyRaised)
            {
                _systemReadyRaised = false;
                RewardEvents.RaiseSystemDestroy();
            }
            _ctrl = null;
        }

        public void EnterReward(int waveNumber)
        {
            if (_ctrl == null)
            {
                logError("RewardSystem: Controller not initialized");
                return;
            }

            _ctrl.EnterReward(waveNumber);
        }
    }
}
namespace MoreMountains
{
    /// <summary>
    /// 命中时获得1+0.2AP点护盾，最大获得护盾不超过最大生命值的25%。
    /// </summary>
    public class Ball_Shield : Ball
    {
        public override BallType BallType => BallType.Shield;

        public float[] baseShields = { 1, 1.25F, 1.5F, 1.75F };
        public float[] gainShieldRates = { 0.1F, 0.15F, 0.2F, 0.25F };
        public float[] maxShields = { 10, 20F, 30F, 40F };

        float baseShield;
        float gainShieldRate;
        float maxShield;

        protected override void onLevelSetup(int lv)
        {
            base.onLevelSetup(lv);
            baseShields.tryGet(levelIndex, out baseShield);
            gainShieldRates.tryGet(levelIndex, out gainShieldRate);
            maxShields.tryGet(levelIndex, out maxShield);
        }

        public override void OnFixedUpdate(float dt)
        {
            base.OnFixedUpdate(dt);
        }

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            var currentShield = _player.Health.Shield.CurrentShield;
            var currentRatio = currentShield / (float)_player.Health.maximumHealth;
            if (currentRatio > 0.25F)
                return;

            var shield = baseShield + gainShieldRate * _player.ap;
            var value = shield.toInt();
            var targetShield = currentShield + value;
            var targetRatio = targetShield / (float)_player.Health.maximumHealth;
            if (targetRatio < 0.25F)
            {
                if (value > 0)
                {
                    _player.Health.Shield.AddShield(value);
                }
            }
            else
            {
                var maxShield = (0.25F * _player.Health.maximumHealth).toInt();
                var deltaShield = maxShield - currentShield;
                if (deltaShield > 0)
                {
                    _player.Health.Shield.AddShield(deltaShield);
                }
            }
        }
    }
}
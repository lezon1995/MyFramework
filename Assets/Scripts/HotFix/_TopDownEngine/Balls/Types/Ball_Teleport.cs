using UniStats;

namespace MoreMountains
{
    public class Ball_Teleport : Ball
    {
        public override BallType BallType => BallType.Teleport;
        const string mod_key = nameof(Ball_Teleport);
        public float speedPctPerBounce = 0.1F;

        public override void onAcquire()
        {
            base.onAcquire();
        }

        protected override void onBounceFinished()
        {
            fx.play(FxDefine.STAR_FLASH_Blue, getWorldPosition());
            var position = _player.getWorldPosition();
            var range = levelManager.getBorderSize();
            Collision2DUtils.FindRandomEmptyPosition(position, range, circleCollider, BRICK_LAYER_MASK, out var teleportPos);

            fx.play(FxDefine.STAR_FLASH, teleportPos);
            setTeleportPosition(teleportPos);

            if (GetStat(Stat.BallisticSpeed, out var stat))
            {
                if (stat.BonusPct.GetMod(mod_key, out var mod))
                    mod.Value += speedPctPerBounce;
                else
                    stat.BonusPct.AddFlat(speedPctPerBounce, mod_key);
            }
        }
    }
}
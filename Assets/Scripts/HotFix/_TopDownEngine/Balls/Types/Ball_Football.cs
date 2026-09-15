using UniStats;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 冲刺时碰到足球会大幅提高足球的速度，速度+200%，随后逐步衰减，速度越快伤害越高。
    /// </summary>
    public class Ball_Football : Ball
    {
        public override BallType BallType => BallType.Football;

        public float BurstSpeedMultiplier = 3F;
        public float SlowdownMultiplier = 0.99F;
        public AnimationCurve SlowdownCurve;
        UniStats.Stat statBallisticSpeed;
        const string mod_key = nameof(Ball_Football);

        public override void onAcquire()
        {
            base.onAcquire();

            addPower<BallSpeedDmgPower>().with(0.01F, 0.02F);
            GetStat(Stat.BallisticSpeed, out statBallisticSpeed);
        }
        
        protected override void playHitBrickSfx(Brick brick)
        {
            sound.play(SoundDefine.FOOTBALL_HIT);
        }

        protected override void playHitBrickVfx(Brick brick, Vector2 normal)
        {
            fx.play(FxDefine.FOOTBALL_HIT, curPos);
        }

        public override void OnFixedUpdate(float dt)
        {
            base.OnFixedUpdate(dt);

            if (statBallisticSpeed.BonusPct.GetMod(mod_key, out var mod))
            {
                mod.Value *= SlowdownMultiplier;
                if (mod.Value.isZero())
                {
                    statBallisticSpeed.BonusPct.RemoveMod(mod_key);
                }
            }
        }

        public override bool onPlayerDashHit(Vector3 position, Vector3 direction)
        {
            base.onPlayerDashHit(position, direction);
            sound.play(SoundDefine.FOOTBALL_HIT);
            var impactDir = direction;
            SetDirection(impactDir, Quaternion.identity);
            if (statBallisticSpeed.BonusPct.GetMod(mod_key, out var mod))
            {
                mod.Value = BurstSpeedMultiplier;
            }
            else
            {
                statBallisticSpeed.BonusPct.AddFlat(BurstSpeedMultiplier, mod_key);
            }

            return true;
        }
    }
}
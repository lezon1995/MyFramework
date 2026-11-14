using UnityEngine;

namespace MoreMountains
{
    public class Ball_ChainSaw : Ball
    {
        public override BallType BallType => BallType.ChainSaw;

        public int DamageGap = 3;
        int _dmgGap;

        public int dmgGap
        {
            get => _dmgGap;
            set => _dmgGap = value.clampMin(1);
        }

        Countdown countdown;

        public override void onAcquire()
        {
            base.onAcquire();
            dmgGap = DamageGap;
            countdown = dmgGap;
        }

        public override bool onImpactHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            return base.onImpactHitEnter(brick, normal, out triggerRegularHit);
        }
        
        public override bool IsTheBrickBeingIgnoredToHit(Brick brick) => false;

        protected override void OnFixedUpdateOverlappingBrick(Brick brick, float dt)
        {
            if (countdown.update())
            {
                countdown = dmgGap;
                CollidingWithBrick(brick, lastHitNormal);
                playHitBrickSfx(brick);
                playHitBrickVfx(brick, lastHitNormal);
            }
        }

        protected override void playHitBrickSfx(Brick brick)
        {
            sound.play(SoundDefine.CLAW_HIT);
        }

        protected override void playHitBrickVfx(Brick brick, Vector2 normal)
        {
            fx.play(FxDefine.CLAW_FLASH, getWorldPosition());
        }

        protected override void EvaluateHit2D(GameObject hitObject, Vector2 hitNormal, Vector2 hitPoint)
        {
            if (hitObject == null)
                return;

            if (hitObject.layer == LayerManager.Brick)
                return;

            base.EvaluateHit2D(hitObject, hitNormal, hitPoint);
        }

        public override void reset()
        {
            base.reset();
            dmgGap = 0;
        }
    }
}
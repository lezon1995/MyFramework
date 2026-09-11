using System;

namespace MoreMountains
{
    public class Ball_EnergyRay : Ball
    {
        public override BallType BallType => BallType.EnergyRay;

        public float castCooldown = 4F;
        public float castDuration = 2F;
        public float rotateSpeed = 90F;
        float currentCastCooldown => castCooldown * _player.cooldownPct;
        float currentCastDuration => castDuration * _player.durationPct;

        Timer timer;
        Action onFinished;

        public Ball_EnergyRay()
        {
            onFinished = OnFinished;
        }

        void OnFinished()
        {
            timer = currentCastCooldown;
        }

        public override void onAcquire()
        {
            base.onAcquire();
            timer = currentCastCooldown;
        }

        public override void OnFixedUpdate(float dt)
        {
            base.OnFixedUpdate(dt);

            if (timer.update(dt))
            {
                effectManager.addLogic<EnergyRayEffect>().with(this, currentCastDuration, rotateSpeed, onFinished);
            }
        }
    }
}
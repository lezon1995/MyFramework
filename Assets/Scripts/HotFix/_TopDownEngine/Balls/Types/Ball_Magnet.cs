namespace MoreMountains
{
    public class Ball_Magnet : Ball
    {
        public override BallType BallType => BallType.Magnet;
        public float autoPickupInterval = 0.1f;
        public float range = 1;
        float _autoPickupTimer;

        public override void onAcquire()
        {
            base.onAcquire();
        }

        public override void OnFixedUpdate(float dt)
        {
            base.OnFixedUpdate(dt);

            _autoPickupTimer += dt;
            if (_autoPickupTimer >= autoPickupInterval)
            {
                _autoPickupTimer = 0f;

                if (expManager)
                    expManager.TryPickupExpsInRange(_player.transform, curPos, range);

                if (coinManager)
                    coinManager.TryPickupCoinsInRange(_player.transform, curPos, range);
            }
        }

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);
        }
    }
}
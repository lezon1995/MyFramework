namespace MoreMountains
{
    public class Ball_Coin : Ball
    {
        public override BallType BallType => BallType.Coin;

        Chance chance;

        public override void onAcquire()
        {
            base.onAcquire();
            chance = level switch
            {
                1 => 0.25F,
                2 => 0.35F,
                3 => 0.45F,
                4 => 0.55F,
                _ => chance
            };
        }

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            if (chance.check())
            {
                CoinDropHelper.DropCoins(curPos, Direction, 1, 1);
            }
        }
    }
}
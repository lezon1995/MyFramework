namespace MoreMountains
{
    public class Ball_Duplicate : Ball
    {
        public override BallType BallType => BallType.Duplicate;

        Countdown counter = 2;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            //复制出来的球不具备再次复制的能力
            if (isTemp)
                return;

            if (counter.update())
            {
                counter.reset();

                var arg = new BallDuplicateArg();
                arg.Type = BallType;
                arg.Pos = curPos;
                arg.Normal = e.dmg.HitNormal;
                arg.Level = level;
                arg.Duration = getDuration();
                effectManager.addLogic<BallDuplicateEffect>().with(_player, _weapon, arg);
            }
        }
    }
}
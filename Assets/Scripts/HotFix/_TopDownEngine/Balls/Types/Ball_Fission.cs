namespace MoreMountains
{
    public class Ball_Fission : Ball
    {
        public override BallType BallType => BallType.Fission;

        public float SplitChance = 0.5F;
        float curSplitChance => SplitChance + _player.triggerChance;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            if (randomHit(curSplitChance))
            {
                var arg = new BallFissionArg();
                arg.Type = BallType;
                arg.Pos = curPos;
                arg.Normal = e.dmg.HitNormal;
                arg.Level = level;
                arg.Duration = getDuration();

                effectManager.addLogic<BallFissionEffect>().with(_player, _weapon, arg);
            }
        }
    }
}
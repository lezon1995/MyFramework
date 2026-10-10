namespace MoreMountains
{
    /// <summary>
    /// 毒烧球特点：
    /// 单个球对单个敌人施buff层数无上限
    /// 每个毒烧球都会提高敌人身上buff的层数，敌人身上会存在1份毒烧Buff
    /// </summary>
    public class Ball_PoisonBurning : Ball
    {
        public override BallType BallType => BallType.PoisonBurning;

        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            var b = e.brick;
            PoisonBurningEffect.Apply(_player, b);
        }
    }
}
using System;
using System.Collections.Generic;

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

        static Dictionary<Brick, PoisonBurningEffect> affectedBricks = new();
        Action<Brick> onEffectEnd;

        public Ball_PoisonBurning()
        {
            onEffectEnd = OnEffectEnd;
        }

        void OnEffectEnd(Brick brick)
        {
            affectedBricks.Remove(brick);
        }
        
        public override void onEvent(DoHitEffect e)
        {
            base.onEvent(e);

            if (!affectedBricks.TryGetValue(e.brick, out var effect))
            {
                effect = effectManager.addLogic<PoisonBurningEffect>();
                effect.with(this, e.brick, onEffectEnd);
                affectedBricks[e.brick] = effect;
            }
            else
            {
                effect.tryApply();
            }
        }
    }
}
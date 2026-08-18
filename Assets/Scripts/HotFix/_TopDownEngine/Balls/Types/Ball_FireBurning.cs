using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 灼烧球特点：
    /// 单个球对单个敌人最多施加3层buff
    /// 每个灼烧球都可以对单个敌人施加3层buff，所以敌人身上会存在N份灼烧Buff
    /// </summary>
    public class Ball_FireBurning : Ball
    {
        public override BallType BallType => BallType.FireBurning;

        Dictionary<Brick, FireBurningEffect> affectedBricks = new();
        Action<Brick> onEffectEnd;

        public Ball_FireBurning()
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
                effect = effectManager.addLogic<FireBurningEffect>();
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
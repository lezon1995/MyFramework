using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 冰霜特点：
    /// 每个冰霜球都会提高敌人身上buff的层数，敌人身上会存在1份冰霜Buff
    /// </summary>
    public class Ball_IceFrost : Ball
    {
        public override BallType BallType => BallType.IceFrost;

        static Dictionary<Brick, IceFrostEffect> affectedBricks = new();
        Action<Brick> onEffectEnd;

        public Ball_IceFrost()
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
                effect = effectManager.addLogic<IceFrostEffect>();
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
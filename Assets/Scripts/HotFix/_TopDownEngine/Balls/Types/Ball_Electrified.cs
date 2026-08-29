using System;
using System.Collections.Generic;

namespace MoreMountains
{
    public class Ball_Electrified : Ball
    {
        public override BallType BallType => BallType.Electrified;

        static Dictionary<Brick, ElectrifiedEffect> affectedBricks = new();
        Action<Brick> onEffectEnd;

        public Ball_Electrified()
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
                effect = effectManager.addLogic<ElectrifiedEffect>();
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
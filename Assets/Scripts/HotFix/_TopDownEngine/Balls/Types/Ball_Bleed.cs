using System;
using System.Collections.Generic;

namespace MoreMountains
{
    public class Ball_Bleed : Ball
    {
        public override BallType BallType => BallType.Bleed;

        static Dictionary<Brick, BleedEffect> affectedBricks = new();
        Action<Brick> onEffectEnd;

        public Ball_Bleed()
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
                effect = effectManager.addLogic<BleedEffect>();
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
using Drawing;
using UnityEngine;

namespace MarbleHero
{
    public class BattleStartEffect : AGameEffect
    {
        const float maxDuration = 4.0F;

        public override void onCreate()
        {
            base.onCreate();
            duration = maxDuration;
        }

        public override bool update(float dt)
        {
            Draw.xy.Label2D(new Vector2(0, 0), "Battle Start", 20, LabelAlignment.Center, color);

            return base.update(dt);
        }
    }
}
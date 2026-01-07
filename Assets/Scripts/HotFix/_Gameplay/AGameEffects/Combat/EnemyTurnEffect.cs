using Drawing;
using UnityEngine;

namespace MarbleHero
{
    public class EnemyTurnEffect : AGameEffect
    {
        const float maxDuration = 4.0F;

        public override void onCreate()
        {
            duration = maxDuration;
        }

        public override bool update(float dt)
        {
            Draw.xy.Label2D(new Vector2(0, 0), "Enemy Turn Start", 20, LabelAlignment.Center, color);
            return base.update(dt);
        }
    }
}
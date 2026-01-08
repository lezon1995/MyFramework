using Drawing;
using UnityEngine;

namespace MarbleHero
{
    public abstract class AGameEffect : ClassObject
    {
        public float duration;
        public float startingDuration;
        protected Color color = new(1, 1, 1, 1);
        public bool isDone;
        protected float scale = Settings.scale;
        protected float rotation;
        public bool renderBehind;

        public override void resetProperty()
        {
            base.resetProperty();
            duration = 0;
            startingDuration = 0;
            color =  new(1, 1, 1, 1);
            isDone = false;
            scale = 0;
            rotation = 0;
            renderBehind = false;
        }

        public override void onCreate()
        {
            base.onCreate();
        }

        public virtual bool update(float dt)
        {
            Draw.ingame.xy.Label2D(new Vector2(0F, -Screen.height / 4F), $"({duration:F2}) {GetType().Name}", 20, LabelAlignment.Center, color);
            
            duration -= dt;

            if (duration < startingDuration / 2.0F)
                color.a = duration / startingDuration / 2.0F;

            if (duration < 0.0F)
            {
                color.a = 0.0F;
                isDone = true;
            }

            return isDone;
        }

        public virtual void Dispose()
        {
        }
    }
}
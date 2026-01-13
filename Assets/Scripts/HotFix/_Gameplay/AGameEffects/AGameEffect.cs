using Drawing;
using UnityEngine;

namespace MarbleHero
{
    public abstract class AGameEffect : ClassObject
    {
        public Timer duration;
        protected Color color = new(1, 1, 1, 1);
        public bool isDone;
        
        public override void onCreate()
        {
            base.onCreate();
        }

        public override void resetProperty()
        {
            base.resetProperty();
            duration = 0;
            color =  new(1, 1, 1, 1);
            isDone = false;
        }

        public virtual bool update(float dt)
        {
            Draw.ingame.xy.Label2D(new Vector2(Screen.width / 4F, -Screen.height / 4F), $"({duration:F2}) {GetType().Name}", 20, LabelAlignment.Center, color);
            
            duration.update(dt);

            if (duration < duration.duration / 2.0F)
                color.a = duration / duration.duration / 2.0F;

            if (duration.isDone)
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
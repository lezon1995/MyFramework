using Drawing;
using UnityEngine;

namespace MarbleHero
{
    public abstract class AGameEffect : ClassObject
    {
        public abstract bool isLogic { get; }
        public Timer duration;
        public bool isDone;

        public override void onCreate()
        {
            base.onCreate();
        }

        public override void resetProperty()
        {
            base.resetProperty();
            duration = 0;
            isDone = false;
        }

        public abstract bool update(float dt);

        public virtual void Dispose()
        {
        }
    }

    public abstract class ALogicEffect : AGameEffect
    {
        public override bool isLogic => true;

        public virtual bool fixedUpdate(float dt)
        {
            duration.update(dt);
            if (duration.isDone)
            {
                isDone = true;
            }

            return isDone;
        }
        
        public override bool update(float dt)
        {
            return isDone;
        }
    }

    public abstract class ARenderEffect : AGameEffect
    {
        protected Color color = new(1, 1, 1, 1);
        
        public override bool isLogic => false;

        public override bool update(float dt)
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

        public override void resetProperty()
        {
            base.resetProperty();
            color = new(1, 1, 1, 1);
        }
    }
}
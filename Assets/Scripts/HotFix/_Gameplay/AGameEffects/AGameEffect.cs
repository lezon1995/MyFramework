using UnityEngine;

namespace MarbleHero
{
    public abstract class AGameEffect : ClassObject
    {
        public float duration;
        public float startingDuration;
        protected Color color;
        public bool isDone;
        protected float scale = Settings.scale;
        protected float rotation;
        public bool renderBehind;

        public override void resetProperty()
        {
            base.resetProperty();
            duration = 0;
            startingDuration = 0;
            color = default;
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
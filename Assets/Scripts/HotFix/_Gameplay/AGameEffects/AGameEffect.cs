using System;
using UnityEngine;

namespace MarbleHero
{
    public abstract class AGameEffect : IDisposable
    {
        public float Duration;
        public float startingDuration;
        protected Color color;
        public bool isDone;
        protected float scale = Settings.scale;
        protected float rotation = 0.0F;
        public bool renderBehind = false;
        public Action onFinished { get; set; }

        public virtual void update(float dt)
        {
            Duration -= dt;

            if (Duration < startingDuration / 2.0F)
                color.a = Duration / startingDuration / 2.0F;

            if (Duration < 0.0F)
            {
                color.a = 0.0F;
                isDone = true;
            }
        }

        public virtual void Dispose()
        {
        }
        
        public static implicit operator bool(AGameEffect self) => self != null;
    }
}
using System;
using Drawing;
using UnityEngine;

namespace MarbleHero
{
    public abstract class APhase : IDisposable
    {
        protected MonsterRoom _room;
        protected float timeElapsed;

        protected APhase(MonsterRoom room)
        {
            _room = room;
        }

        public virtual void onBegin(APhase last)
        {
            timeElapsed = 0F;
            onBindListeners();
        }

        public virtual void update(float dt)
        {
            timeElapsed += dt;
            Draw.ingame.xy.Label2D(new Vector2(Screen.width / 4F, 0F), $"({timeElapsed:F2}) {GetType().Name}", 20, LabelAlignment.Center, Color.darkOrange);
        }

        public virtual void fixedUpdate(float dt)
        {
        }

        public virtual void onEnd()
        {
            timeElapsed = 0F;
            onUnbindListeners();
        }

        protected abstract void onBindListeners();
        protected abstract void onUnbindListeners();


        public virtual void Dispose()
        {
        }
    }
}
using System;

namespace MarbleHero
{
    public abstract class APhase : IDisposable
    {
        protected MonsterRoom _room;
        protected APhase(MonsterRoom room)
        {
            _room = room;
        }

        public virtual void onBegin(APhase last) => onBindListeners();

        public abstract void update(float dt);
        public abstract void fixedUpdate(float dt);

        public virtual void onEnd() => onUnbindListeners();

        protected abstract void onBindListeners();
        protected abstract void onUnbindListeners();


        public virtual void Dispose()
        {
        }
    }
}
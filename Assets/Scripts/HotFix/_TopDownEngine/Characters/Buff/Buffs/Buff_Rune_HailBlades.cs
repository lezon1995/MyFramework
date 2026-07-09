using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public class Buff_Rune_HailBlades : Buff
        , IEvent<OnWindup>
        , IEvent<Buff.AfterRemoved>
    {
        public Buff Buff;

        bool _triggered;

        protected override void OnAfterAdd()
        {
            _triggered = false;
            Target.Event.addListener<OnWindup>(this);
            Target.Event.addListener<AfterRemoved>(this);
        }

        protected override void OnBeforeRemove()
        {
            Target.Event.removeListener<OnWindup>(this);
            Target.Event.removeListener<AfterRemoved>(this);
        }

        public void onEvent(OnWindup e)
        {
            switch (e.State)
            {
                case OnWindup.States.Start:
                    if (!_triggered)
                        Target.ApplyBuff(Buff);
                    
                    break;
                case OnWindup.States.Finish:
                    if (Target.HasBuff(Buff.BuffType))
                    {
                        _triggered = true;
                        Target.ApplyBuff(Buff, param: new(-1));
                    }

                    break;
                case OnWindup.States.Cancel:
                    if (!_triggered)
                        Target.RemoveBuffWithType(Buff.BuffType);

                    break;
            }
        }

        public void onEvent(AfterRemoved e)
        {
            if (_triggered == false)
                return;

            if (e.Buff.BuffType == Buff.BuffType)
            {
                _triggered = false;
            }
        }
    }
}
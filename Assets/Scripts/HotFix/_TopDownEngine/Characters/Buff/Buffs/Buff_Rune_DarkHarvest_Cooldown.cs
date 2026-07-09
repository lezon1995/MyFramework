using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public class Buff_Rune_DarkHarvest_Cooldown : Buff, IEvent<DoKill>
    {
        public float ResetDuration = 1;
        
        bool _triggered;

        protected override void OnAfterAdd()
        {
            _triggered = false;
            Target.Event.addListener(this);
        }

        protected override void OnBeforeRemove()
        {
            Target.Event.removeListener(this);
        }

        public void onEvent(DoKill e)
        {
            if (_triggered)
                return;

            _triggered = true;

            var duration = ResetDuration;
            if (DurationLeft < duration)
            {
                duration = DurationLeft;
            }

            DurationElapsed = 0F;
            OverrideDuration = duration;
        }
    }
}
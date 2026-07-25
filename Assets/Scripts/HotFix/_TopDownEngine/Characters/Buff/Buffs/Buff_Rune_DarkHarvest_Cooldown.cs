using MoreMountains.Tools;

namespace MoreMountains
{
    public class Buff_Rune_DarkHarvest_Cooldown : Buff, IEvent<DoKill>
    {
        public float ResetDuration = 1;

        bool _triggered;
        float overrideDuration;

        public override float Duration
        {
            get
            {
                if (IsInfinite)
                    return 0F;

                if (overrideDuration > 0)
                    return overrideDuration;

                if (IsStackDecreasing && isOverrideDecreasingDuration)
                    return DecreasingDuration;

                return duration.Value(this);
            }
        }

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

            var d = ResetDuration;
            if (DurationLeft < d)
            {
                d = DurationLeft;
            }

            DurationElapsed = 0F;
            overrideDuration = d;
        }
    }
}
using MoreMountains.Tools;

namespace MoreMountains
{
    public class Buff_Rune_AbsorbLife : Buff, IEvent<DoKill>
    {
        public Mag Healing;

        protected override void OnAfterAdd()
        {
            Target.Event.addListener<DoKill>(this);
        }

        protected override void OnBeforeRemove()
        {
            Target.Event.removeListener<DoKill>(this);
        }

        public void onEvent(DoKill e)
        {
            var value = Healing.Value(this);
            Target.Health.ReceiveHealth(Heal.Fixed((int)value));
        }
    }
}
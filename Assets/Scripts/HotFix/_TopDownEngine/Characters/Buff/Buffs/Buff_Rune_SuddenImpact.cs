using MoreMountains.Tools;

namespace MoreMountains
{
    public class Buff_Rune_SuddenImpact : Buff
        , IEvent<DoDash>
    {
        public Buff Buff;

        protected override void OnAfterAdd()
        {
            Target.Event.addListener(this);
        }

        protected override void OnBeforeRemove()
        {
            Target.Event.removeListener(this);
        }

        public void onEvent(DoDash e)
        {
            Target.ApplyBuff(Buff);
        }
    }
}
using MoreMountains.Tools;

namespace MoreMountains
{
    public class Buff_Rune_SuddenImpact : Buff
        , IEvent<DoDashStart>
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

        public void onEvent(DoDashStart e)
        {
            Target.ApplyBuff(Buff);
        }
    }
}
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public class Buff_Rune_SuddenImpact_Damage : Buff
        , IEvent<DoAttackEffect>
    {
        public DmgMag Damage;

        protected override void OnAfterAdd()
        {
            Target.Event.addListener(this);
        }

        protected override void OnBeforeRemove()
        {
            Target.Event.removeListener(this);
        }

        public void onEvent(DoAttackEffect e)
        {
            var mag = Damage;
            var value = mag.Value(this);
            var dmg = new Dmg(value, mag.DmgType, mag.DmgAlgo);
            dmg.SetEffect(Dmg.Effects.Ability);

            Target.RemoveBuffWithType(BuffType, true);

            e.Character.Health.Damage(dmg, gameObject);
        }
    }
}
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    public class Buff_Rune_Conqueror : Buff
        , IEvent<DoDmg>
    {
        public Mag HealCoeff;

        protected override void OnMaxStacked(int maxStack)
        {
            Target.Health.Event.addListener<DoDmg>(this);
        }

        protected override void OnBeforeRemove()
        {
            Target.Health.Event.removeListener<DoDmg>(this);
        }

        public void onEvent(DoDmg e)
        {
            var damage = e.Dmg.DamageDealt;
            var coeff = HealCoeff.Value(this);
            var heal = damage * coeff;
            Target.Health.ReceiveHealth(Heal.Fixed(heal));
        }
    }
}
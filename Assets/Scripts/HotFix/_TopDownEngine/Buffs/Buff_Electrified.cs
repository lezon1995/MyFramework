namespace MoreMountains
{
    public class Buff_Electrified : Buff
    {
        public override void OnTakeDmg(OnDmg e)
        {
            base.OnTakeDmg(e);

            var curStack = Stack;
            var dmg = DmgGetter();
            dmg.SetValue(dmg.Value * curStack);
            dmg.SetMetaType((int)DotDamageType.Electrified);
            Owner.Character.Health.Damage(ref dmg, gameObject, e.Source);
        }
    }
}
namespace MoreMountains
{
    public class Buff_Bleed : Buff
    {
        protected override bool TryGetPeriodDamage(out Dmg dmg)
        {
            var mag = periodDamage;
            var value = mag.Value(this);
            if (value > 0)
            {
                dmg = new Dmg((int)value, mag.DmgType, mag.DmgAlgo);
                dmg.SetMetaType((int)DotDamageType.Bleed);
                return true;
            }

            dmg = default;
            return false;
        }
        
        Timer cd;

        public override bool OnFixedUpdate(float dt, out Removal removal)
        {
            cd.update(dt);
            return base.OnFixedUpdate(dt, out removal);
        }
        
        public override void OnTakeDmg(OnDmg e)
        {
            base.OnTakeDmg(e);
            
            var hash = GetHashCode();
            if (e.Dmg.Hash == hash)
                return;
            
            if (cd && !cd.isDone)
                return;

            cd = 0.1F;

            var curStack = Stack;
            var dmg = DmgGetter();
            dmg.SetActualType(Dmg.Types.True);
            dmg.SetValue(dmg.Value * curStack);
            dmg.SetMetaType((int)DotDamageType.Bleed);
            Owner.Character.Health.Damage(ref dmg, gameObject, e.Source);
        }
    }
}
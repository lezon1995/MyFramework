namespace MoreMountains
{
    public class Buff_Electrified : Buff
    {
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
            dmg.SetValue(dmg.Value * curStack);
            dmg.SetMetaType((int)DotDamageType.Electrified);
            dmg.SetHash(hash);
            Owner.Character.Health.Damage(ref dmg, gameObject, e.Source);
        }
    }
}
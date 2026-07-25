namespace MoreMountains
{
    public class Buff_PoisonBurning : Buff
    {
        protected override bool TryGetPeriodDamage(out Dmg dmg)
        {
            var mag = periodDamage;
            var value = mag.Value(this) * Stack;
            if (value > 0)
            {
                dmg = new Dmg((int)value, mag.DmgType, mag.DmgAlgo);
                dmg.SetMetaType((int)DotDamageType.Poison);
                return true;
            }

            dmg = default;
            return false;
        }
    }
}
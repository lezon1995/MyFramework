namespace MoreMountains
{
    public enum DotDamageType
    {
        None,
        Fire,
        Poison,
    }
    public class Buff_FireBurning : Buff
    {
        public override float Period
        {
            get
            {
                if (IsPeriodic)
                {
                    return Stack switch
                    {
                        1 => 1,
                        2 => 1 / 2F,
                        3 => 1 / 3F,
                        _ => periodDuration.Value(this)
                    };
                }

                return 0F;
            }
        }
        
        protected override bool TryGetPeriodDamage(out Dmg dmg)
        {
            var mag = periodDamage;
            var value = mag.Value(this);
            if (value > 0)
            {
                dmg = new Dmg((int)value, mag.DmgType, mag.DmgAlgo);
                dmg.SetMetaType((int)DotDamageType.Fire);
                return true;
            }

            dmg = default;
            return false;
        }
    }
}
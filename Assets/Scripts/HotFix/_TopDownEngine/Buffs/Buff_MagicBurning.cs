namespace MoreMountains
{
    public class Buff_MagicBurning : Buff
    {
        protected override void OnAfterAdd()
        {
            base.OnAfterAdd();

            if (Target.Character is Brick b)
            {
            }
        }

        protected override void OnBeforeRemove()
        {
            base.OnBeforeRemove();

            if (Target.Character is Brick b)
            {
            }
        }
        
        protected override bool TryGetPeriodDamage(out Dmg dmg)
        {
            var mag = periodDamage;
            var value = mag.Value(this) * Stack;
            if (value > 0)
            {
                dmg = new Dmg((int)value, mag.DmgType, mag.DmgAlgo);
                dmg.SetMetaType((int)DotDamageType.Magic);
                return true;
            }

            dmg = default;
            return false;
        }
    }
}
namespace MoreMountains
{
    public class EnemyBulletDamageOnTouch : DamageOnTouch
    {
        protected override void BindStats()
        {
            DmgGetter = () =>
            {
                if (Source && Source.GetStat(Character.Stat.AP, out var stat))
                {
                    return Dmg.AP((int)stat.Value);
                }

                return Dmg;
            };
        }
    }
}
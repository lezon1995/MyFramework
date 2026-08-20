namespace MoreMountains
{
    public class EnemyBulletDamageOnTouch : DamageOnTouch
    {
        protected override void BindStats()
        {
            if (Owner.TryGetComponent<Stats>(out var stats))
            {
                DmgGetter = () => Dmg.AP((int)stats.GetStat(Character.Stat.AP.Key()).Value);
            }
            else
            {
                DmgGetter = () => Dmg;
            }
        }
    }
}
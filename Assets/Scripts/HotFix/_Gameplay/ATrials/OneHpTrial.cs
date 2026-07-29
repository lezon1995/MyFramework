namespace MoreMountains
{
    public class OneHpTrial : ATrial
    {
        public override void setupPlayer(ref APlayer p)
        {
            p.currentHealth = 1;
            p.maxHealth = 1;
        }
    }
}
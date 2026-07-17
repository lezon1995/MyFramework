namespace MoreMountains
{
    public class OneHpTrial : ATrial
    {
        public override APlayer setupPlayer(APlayer player)
        {
            player.currentHealth = 1;
            player.maxHealth = 1;
            return player;
        }
    }
}
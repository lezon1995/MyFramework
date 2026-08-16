namespace MoreMountains
{
    public class PlayerExp : Exp
    {
        protected override int CalculateXpRequiredToNextLevel(int level)
        {
            var xpRequired = (level + 3) * (level + 3);
            return xpRequired;
        }

        protected override int CalculateXpTotalToLevel(int level)
        {
            int totalXP = 0;
            for (int i = 1; i <= level; i++)
            {
                int xpRequiredForLevel = CalculateXpRequiredToNextLevel(i);
                totalXP += xpRequiredForLevel;
            }

            return totalXP;
        }
    }
}
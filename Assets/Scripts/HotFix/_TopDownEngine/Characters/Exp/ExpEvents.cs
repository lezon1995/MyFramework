namespace MoreMountains
{
    public struct OnAddXp
    {
        public int Xp;
        public float Ratio;

        public OnAddXp(int xp, float ratio)
        {
            Xp = xp;
            Ratio = ratio;
        }
    }
    
    public struct OnLevelUp
    {
        public int Xp;
        public int Level;
        public float Ratio;

        public OnLevelUp(int xp, int level, float ratio)
        {
            Xp = xp;
            Level = level;
            Ratio = ratio;
        }
    }

    public struct OnXpTotalChange
    {
        public float Xp;
        public OnXpTotalChange(float xp) => Xp = xp;
    }

    public struct OnXpChange
    {
        public float Xp;
        public float Ratio;

        public OnXpChange(float xp, float ratio)
        {
            Xp = xp;
            Ratio = ratio;
        }
    }

    public struct OnLevelChange
    {
        public int Pre;
        public int Cur;

        public OnLevelChange(int pre, int cur)
        {
            Pre = pre;
            Cur = cur;
        }
    }

    public struct OnXpRequiredChange
    {
        public float Xp;
        public OnXpRequiredChange(float xp) => Xp = xp;
    }

    public struct OnMaxLevel
    {
    }
}
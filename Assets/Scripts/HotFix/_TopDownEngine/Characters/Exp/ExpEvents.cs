namespace MoreMountains.TopDownEngine
{
    public struct OnAddXp
    {
        public float Xp;
        public OnAddXp(float xp) => Xp = xp;
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
namespace MoreMountains;

public partial class APlayer
{
    public override T addPower<T>()
    {
        int buffCount = 0;
        foreach (var p in powers)
        {
            if (p.type == PowerType.BUFF)
                buffCount++;
        }

        if (buffCount >= 10)
            UnlockTracker.unlockAchievement("POWERFUL");

        return base.addPower<T>();
    }
}
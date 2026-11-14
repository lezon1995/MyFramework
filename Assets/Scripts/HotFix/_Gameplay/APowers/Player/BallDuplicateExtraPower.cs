namespace MoreMountains;

public class BallDuplicateExtraPower : PlayerPower, IBallDuplicateEffectModifier
{
    public void modifyCount(ref int count)
    {
        count++;
    }
}

public class BallFissionExtraPower : PlayerPower, IBallFissionEffectModifier
{
    public void modifyCount(ref int count)
    {
        count += 2;
    }
}
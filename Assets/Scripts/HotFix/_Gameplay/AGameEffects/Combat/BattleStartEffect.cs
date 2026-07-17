namespace MoreMountains;

public class BattleStartEffect : ARenderEffect
{
    const float maxDuration = 4.0F;

    public override void onCreate()
    {
        base.onCreate();
        duration = maxDuration;
    }

    public override bool update(float dt)
    {
        return base.update(dt);
    }
}
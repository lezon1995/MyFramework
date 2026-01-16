namespace MarbleHero;

public class EnemyTurnEffect : ARenderEffect
{
    const float maxDuration = 4.0F;

    public override void onCreate()
    {
        duration = maxDuration;
    }

    public override bool update(float dt)
    {
        return base.update(dt);
    }
}
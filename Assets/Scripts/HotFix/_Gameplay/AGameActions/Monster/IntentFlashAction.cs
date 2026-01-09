namespace MarbleHero;

public class IntentFlashAction : AGameAction, IGameActionArgs<AMonster, EnemyMoveInfo>
{
    const float DURATION = 0.5F;
    ACreature monster;
    EnemyMoveInfo moveInfo;

    public void onCreate(AMonster m, EnemyMoveInfo info)
    {
        duration = DURATION;
        monster = m;
        moveInfo = info;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        monster = null;
        moveInfo = default;
    }

    public override void update(float dt)
    {
        tickDuration(dt);
        if (isDone)
        {
        }
    }
}
namespace MarbleHero;

public class ShowMoveNameAction : AGameAction, IGameActionArgs<AMonster, EnemyMoveInfo>
{
    const float DURATION = 0.25F;
    AMonster monster;
    EnemyMoveInfo moveInfo;
    MyCurve curve;

    public void onCreate(AMonster m, EnemyMoveInfo info)
    {
        duration = DURATION;
        monster = m;
        moveInfo = info;
        curve = mKeyFrameManager.getKeyFrame(KEY_CURVE.SINE_IN_OUT);
        ADungeon.overlayMenu.intents.hideIntentItemBefore(moveInfo);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        monster = null;
        curve = null;
        moveInfo = default;
    }

    public override void update(float dt)
    {
        tickDuration(dt);
        ADungeon.overlayMenu.intents.updateIntentItemsPos(moveInfo, duration.pct);
    }
}
/*namespace MoreMountains;

public class IntentFlashAction : AGameAction, IArgs<AMonster, EnemyMoveInfo>
{
    const float DURATION = 0.25F;
    ACreature monster;
    EnemyMoveInfo moveInfo;
    MyCurve curve;

    public void onCreate(AMonster m, EnemyMoveInfo info)
    {
        duration = DURATION;
        monster = m;
        moveInfo = info;
        curve = mKeyFrameManager.getKeyFrame(KEY_CURVE.SINE_IN_OUT);
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
        var f = curve.evaluate(duration.pct);
        ADungeon.overlayMenu.intents.updateIntentItemScale(moveInfo, f);
    }
}*/
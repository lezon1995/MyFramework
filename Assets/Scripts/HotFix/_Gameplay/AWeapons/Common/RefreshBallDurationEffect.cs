namespace MoreMountains;

public class RefreshBallDurationEffect : ALogicEffect, IArgs<Ball>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_Ring_Flash.prefab";
    GameEffect effect;
    Ball ball;

    public void onCreate(Ball p1)
    {
        ball = p1;
        duration = 0.3F;
        p1.refreshDuration();

        effect = mEffectManager.createEffect(path);
        refreshEffectPosition();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        mEffectManager.destroyEffect(ref effect);
    }

    public override bool update(float dt)
    {
        refreshEffectPosition();
        return base.update(dt);
    }

    void refreshEffectPosition()
    {
        var pos = ball.getWorldPosition();
        effect.setWorldPosition(pos);
    }
}
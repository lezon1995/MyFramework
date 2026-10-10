namespace MoreMountains;

public class BallDoHitEffect : ALogicEffect, IArgs<Ball, DoHitEffect, float>
{
    Ball ball;
    DoHitEffect hitEffect;

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        hitEffect = default;
    }

    public void onCreate(Ball b, DoHitEffect e, float delay)
    {
        ball = b;
        hitEffect = e;
        duration = delay;
    }

    public override void onFinished()
    {
        ball.Event.trigger(hitEffect);
        ball.getPlayer().Event.trigger(hitEffect);
    }
}
namespace MoreMountains;

/// <summary>
/// 黄色落雷效果
/// </summary>
public class LightningStrikeEffect2 : ALogicEffect, IArgs<Ball>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_LightningStrike_Yellow.prefab";

    protected Ball ball;

    public void onCreate(Ball b1)
    {
        duration = 0.32F;
        ball = b1;

        if (brickManager.getRandomActiveBrick(out var brick))
        {
            var effect = mEffectManager.createEffect(path, 0.5F);
            var pos = brick.getWorldPosition();
            effect.setWorldPosition(pos);

            ball.onSkillHitEnter(brick);
        }
    }

    public override bool fixedUpdate(float dt)
    {
        if (duration.unstarted)
        {
            sound.play(SoundDefine.LIGHTNING_STRIKE);
        }

        return base.fixedUpdate(dt);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
    }
}
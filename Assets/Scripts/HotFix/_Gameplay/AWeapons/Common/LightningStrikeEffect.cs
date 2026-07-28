namespace MoreMountains;

/// <summary>
/// 连锁电流
/// 多个目标顺序受到伤害
/// 伤害递增
/// </summary>
public class LightningStrikeEffect : ALogicEffect, IArgs<Ball, Brick>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_LightningStrike.prefab";

    protected Ball ball;

    public void onCreate(Ball b1, Brick b2)
    {
        duration = 0.1666667F;
        ball = b1;

        if (brickManager.getRandomActiveBrick(out var brick, b2))
        {
            var effect = mEffectManager.createEffect(path, 0.5F);
            var pos = brick.getWorldPosition();
            effect.setWorldPosition(pos);
            
            var dmg = ball.getSkillDmg(brick);
            brick.Health.Damage(ref dmg, ball.gameObject, ball.Player);
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
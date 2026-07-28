using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 连锁电流
/// 多个目标顺序受到伤害
/// 伤害递增
/// </summary>
public class LaserBeamEffect : ALogicEffect, IArgs<Ball, Brick, float>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_LaserBeam.prefab";

    protected Ball ball;

    public void onCreate(Ball b1, Brick b2, float angle)
    {
        duration = 0.5F;
        ball = b1;
        var effect = mEffectManager.createEffect(path, 0.5F);
        var pos = b1.getWorldPosition();
        effect.setWorldPosition(pos);
        effect.setRotationZ(angle);

        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.up;
        using var _ = new ListScope<RaycastHit2D>(out var hits);
        var distance = 40;
        var start = pos - dir * (distance * 0.5F);
        var end = pos + dir * (distance * 0.5F);
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(BRICK_LAYER_MASK);
        var count = Physics2D.CircleCast(start, 0.1F, dir, filter, hits, distance);
        Debug.DrawLine(start, end, Color.red, 0.5F);
        for (var i = 0; i < count; i++)
        {
            if (hits[i].collider.TryGetComponent(out Brick b))
            {
                var dmg = ball.getSkillDmg(b);
                b.Health.Damage(ref dmg, ball.gameObject, ball.Player);
            }
        }
    }

    public override bool fixedUpdate(float dt)
    {
        if (duration.unstarted)
        {
            sound.play(SoundDefine.LASER_BEAM);
        }

        return base.fixedUpdate(dt);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
    }
}
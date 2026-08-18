using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 连锁电流
/// 多个目标顺序受到伤害
/// 伤害递增
/// </summary>
public class RockSplashEffect : ALogicEffect, IArgs<Ball, Brick>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_RockSplash.prefab";

    protected Ball ball;
    Vector2 dirSplash;
    Vector2 posSplash;
    float knockbackForceRatio;

    public void onCreate(Ball b1, Brick b2)
    {
        duration = 0.3F;
        knockbackForceRatio = 0.5F;
        ball = b1;
        var effect = mEffectManager.createEffect(path, 0.3F);
        posSplash = b1.getWorldPosition();
        effect.setWorldPosition(posSplash);
        dirSplash = b1.getDirection();
        effect.transform.right = dirSplash;
    }

    public override bool fixedUpdate(float dt)
    {
        if (duration.unstarted)
        {
            using var _ = new ListScope<Collider2D>(out var colliders);
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(BRICK_LAYER_MASK);
            var radius = 1.5F;
            var count = Physics2D.OverlapCircle(posSplash, radius, filter, colliders);
            for (var i = 0; i < count; i++)
            {
                var collider = colliders[i];
                if (!CollisionHelper.isColliderInSector(posSplash, dirSplash, radius, 80, collider))
                    continue;

                if (collider.TryGetComponent(out Brick b))
                {
                    if (b.IsDead())
                        continue;

                    var dmg = ball.getSkillDmg(b);
                    b.Health.Damage(ref dmg, ball.gameObject, ball.Player);
                    var knockbackForce = ball.getKnockbackForce(b.Health, dmg);
                    b.Health.ApplyKnockback(knockbackForce * knockbackForceRatio, dmg);
                }
            }

            sound.play(SoundDefine.ROCK_QUAKE);
        }

        return base.fixedUpdate(dt);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        knockbackForceRatio = 0F;
        dirSplash = default;
        posSplash = default;
    }
}
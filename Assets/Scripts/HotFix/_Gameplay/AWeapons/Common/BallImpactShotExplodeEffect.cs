using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 撞击暴击爆炸效果
/// </summary>
public class BallImpactShotExplodeEffect : ALogicEffect, IArgs<Ball, Brick, Vector3>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_Shot_Explosion.prefab";

    protected Ball ball;
    protected Vector3 explosionPos;
    protected Vector3 explosionDir;
    float offsetX;
    float explosionRadius;

    public void onCreate(Ball b1, Brick b2, Vector3 dir)
    {
        duration = 0.5F;
        ball = b1;
        explosionDir = dir.normalized;
        var effect = mEffectManager.createEffect(path, 0.5F);
        explosionPos = b1.getWorldPosition();
        effect.setWorldPosition(explosionPos);
        effect.setRightDirection(explosionDir);
        offsetX = 1F;
        explosionRadius = 1F;
    }

    public override bool fixedUpdate(float dt)
    {
        if (duration.unstarted)
        {
            using var _ = new ListScope<Collider2D>(out var colliders);
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(BRICK_LAYER_MASK);
            var count = Physics2D.OverlapCircle(explosionPos + explosionDir * offsetX, explosionRadius, filter, colliders);
            for (var i = 0; i < count; i++)
            {
                if (colliders[i].TryGetComponent(out Brick b))
                {
                    if (b.IsDead())
                        continue;

                    ball.onSkillHitEnter(b, explosionDir);
                }
            }

            sound.play(SoundDefine.BOMB_EXPLODE);
        }

        return base.fixedUpdate(dt);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        explosionPos = default;
        explosionDir = default;
        offsetX = 0F;
        explosionRadius = 0F;
    }
}
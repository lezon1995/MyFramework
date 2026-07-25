using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 连锁电流
/// 多个目标顺序受到伤害
/// 伤害递增
/// </summary>
public class RockQuakeEffect : ALogicEffect, IArgs<Ball, Brick>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_RockQuake.prefab";

    protected Ball ball;

    public void onCreate(Ball b1, Brick b2)
    {
        duration = 0.5F;
        ball = b1;
        var effect = mEffectManager.createEffect(path, 0.5F);
        var pos = b1.getWorldPosition();
        effect.setWorldPosition(pos);
    }

    public override bool fixedUpdate(float dt)
    {
        if (duration.unstarted)
        {
            using var _ = new ListScope<Collider2D>(out var colliders);
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(BRICK_LAYER_MASK);
            var count = Physics2D.OverlapBox(ball.getWorldPosition(), new Vector2(0.675F, 0.675F) * 3F, 0F, filter, colliders);
            for (var i = 0; i < count; i++)
            {
                if (colliders[i].TryGetComponent(out Brick b))
                {
                    if (b.IsDead())
                        continue;
                    
                    var dmg = ball.getSkillDmg(b);
                    b.Health.Damage(ref dmg, ball.gameObject, ball.character);
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
    }
}
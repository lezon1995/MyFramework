using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 冲击波效果
/// </summary>
public class ShockWaveEffect : ALogicEffect, IArgs<APlayer, Vector3, int>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_ShockWave.prefab";

    protected Vector3 explosionPos;
    protected APlayer _player;
    protected int damage;

    public void onCreate(APlayer p, Vector3 pos, int dmg)
    {
        _player = p;
        duration = 0.5F;
        damage = dmg;
        var effect = mEffectManager.createEffect(path, 0.5F);
        explosionPos = pos;
        effect.setWorldPosition(explosionPos);
    }

    public override bool fixedUpdate(float dt)
    {
        if (duration.unstarted)
        {
            using var _ = new ListScope<Collider2D>(out var colliders);
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(BRICK_LAYER_MASK);
            var count = Physics2D.OverlapCircle(explosionPos, 2.5F, filter, colliders);
            for (var i = 0; i < count; i++)
            {
                if (colliders[i].TryGetComponent(out Brick b))
                {
                    if (b.IsDead())
                        continue;

                    var dir = (b.getWorldPosition() - explosionPos).normalized;
                    var dmg = Dmg.AP(damage);
                    b.Health.Damage(ref dmg, _player.gameObject, _player, direction: dir);
                    var force = 2 * dir.normalized;
                    b.Health.ApplyKnockback(force);
                }
            }
            
            sound.play(SoundDefine.BOMB_EXPLODE);
        }

        return base.fixedUpdate(dt);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        _player = null;
        damage = 0;
        explosionPos = default;
    }
}
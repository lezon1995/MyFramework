using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 连锁电流
/// 多个目标顺序受到伤害
/// 伤害递增
/// </summary>
public class LightningRangeStrikeEffect : ALogicEffect, IArgs<APlayer, Vector3, float, int>
{
    protected const string fx_path1 = $"{GAMEPLAY_PATH}/Effects/Fx_Ring_Flash.prefab";
    protected const string fx_path2 = $"{GAMEPLAY_PATH}/Effects/Fx_LightningStrike_Yellow.prefab";

    protected Vector3 pos;
    protected APlayer _player;
    int damage;

    public void onCreate(APlayer p, Vector3 b1, float b2, int value)
    {
        _player = p;
        pos = b1;
        duration = b2;
        damage = value;
        
        var effect = mEffectManager.createEffect(fx_path1, 0.3F);
        effect.setWorldPosition(pos);
    }

    public override void onFinished()
    {
        var effect = mEffectManager.createEffect(fx_path2, 0.2F);
        effect.setWorldPosition(pos);

        using var _ = new ListScope<Collider2D>(out var colliders);
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(BRICK_LAYER_MASK);
        var count = Physics2D.OverlapCircle(pos, BrickUnitSize / 2F, filter, colliders);
        for (var i = 0; i < count; i++)
        {
            if (colliders[i].TryGetComponent(out Brick b))
            {
                if (b.IsDead())
                    continue;

                var dir = (b.getWorldPosition() - pos).normalized;
                var dmg = Dmg.AP(damage);
                b.Health.Damage(ref dmg, _player.gameObject, _player, direction: dir);
            }
        }
        
        sound.play(SoundDefine.LIGHTNING_STRIKE);
    }

    public override bool fixedUpdate(float dt)
    {
        // if (duration.unstarted)
        // {
        //     sound.play(SoundDefine.LIGHTNING_STRIKE);
        // }

        return base.fixedUpdate(dt);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        pos = default;
        _player = null;
        damage = 0;
    }
}
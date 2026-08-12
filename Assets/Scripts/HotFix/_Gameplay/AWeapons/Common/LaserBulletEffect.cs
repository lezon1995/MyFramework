using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 连锁电流
/// 多个目标顺序受到伤害
/// 伤害递增
/// </summary>
public class LaserBulletEffect : ALogicEffect, IArgs<Ball, Brick, Vector2>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_LaserBullet.prefab";

    protected Ball ball;
    protected Projectile bullet;

    public void onCreate(Ball b1, Brick b2, Vector2 dir)
    {
        duration = 0.5F;
        ball = b1;
        var o = prefabPool.createObject(path);
        if (o.TryGetComponent(out bullet))
        {
            var pos = b1.getWorldPosition();
            bullet.setPosition(pos);

            var rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            bullet.SetDirection(dir, rotation);
            bullet.SetOwner(b1.Player);
            bullet.SetDamage(Dmg.AP((int)(5 + ball.GetStat(Ball.Stat.EffectDamage).Value + ball.Player.GetStat(Character.Stat.AP).Value)));
            bullet.TryClearTrails();
        }
    }

    public override bool fixedUpdate(float dt)
    {
        var done = base.fixedUpdate(dt);
        done = bullet.Health.IsDead();
        return done;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        if (bullet)
        {
            bullet.TryClearTrails();
            prefabPool.destroyObject(bullet.gameObject, false);
        }

        bullet = null;
    }
}
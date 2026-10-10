using UnityEngine;

namespace MoreMountains;

public class LaserLineEffect : ALogicEffect, IArgs<APlayer, Brick, int, float>
{
    protected virtual string path => $"{GAMEPLAY_PATH}/Effects/FxLine_Laser.prefab";

    APlayer _player;
    Brick _brick;
    GameEffect effect;
    LineRenderer lineRenderer;
    int damage;

    public void onCreate(APlayer p1, Brick p2, int value, float time)
    {
        _player = p1;
        _brick = p2;
        damage = value;
        duration = time;

        effect = mEffectManager.createEffect(path);
        effect.tryGetUnityComponent(out lineRenderer);
        var color = lineRenderer.startColor;
        color.a =1;
        lineRenderer.startColor = color;
        color = lineRenderer.endColor;
        color.a =1;
        lineRenderer.endColor = color;
    }

    public override bool update(float dt)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, _player.getWorldPosition());
        lineRenderer.SetPosition(1, _brick.getCenterPosition());
        var color = lineRenderer.startColor;
        color.a =1- duration.pct;
        lineRenderer.startColor = color;
        color = lineRenderer.endColor;
        color.a =1- duration.pct;
        lineRenderer.endColor = color;
        return base.update(dt);
    }

    public override bool fixedUpdate(float dt)
    {
        if (duration.unstarted)
        {
            var dmg = Dmg.AP(damage);
            _brick.Health.Damage(ref dmg, _player.gameObject, _player, direction: Vector3.up);
        }

        return base.fixedUpdate(dt);
    }
    
    public override void resetProperty()
    {
        base.resetProperty();
        
        _player = null;
        _brick = null;
        damage = 0;
        mEffectManager.destroyEffect(ref effect);
        lineRenderer = null;
    }
}

public class LaserLine2Effect : LaserLineEffect
{
    protected override string path => $"{GAMEPLAY_PATH}/Effects/FxLine_Laser_White.prefab";
}
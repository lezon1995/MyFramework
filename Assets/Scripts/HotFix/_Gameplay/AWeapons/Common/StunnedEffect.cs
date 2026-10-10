using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 眩晕效果
/// </summary>
public class StunnedEffect : ALogicEffect
    , IArgs<Brick, float, Vector3>
{
    protected const string fx_path = $"{GAMEPLAY_PATH}/Effects/Fx_Stun.prefab";
    protected const string fx_path_loop = $"{GAMEPLAY_PATH}/Effects/Fx_Stun_Loop.prefab";

    protected Brick brick;
    protected GameEffect flashEffect, loopEffect;
    protected Vector3 offsetPos;
    public StunnedEffect() { }

    public void onCreate(Brick b2, float time, Vector3 offset)
    {
        duration = time;
        brick = b2;
        offsetPos = offset;

        flashEffect = mEffectManager.createEffect(fx_path, 0.3F);
        loopEffect = mEffectManager.createEffect(fx_path_loop);
        refreshEffectPosition();
        b2.Stun();
    }

    public override bool update(float dt)
    {
        refreshEffectPosition();
        return base.update(dt);
    }

    void refreshEffectPosition()
    {
        var pos = brick.getCenterPosition();
        flashEffect.setWorldPosition(pos);

        pos += (Vector2)offsetPos;
        loopEffect.setWorldPosition(pos);
    }

    public override void onFinished()
    {
        brick.UnStun();
    }

    public override void resetProperty()
    {
        base.resetProperty();

        brick = null;
        mEffectManager.destroyEffect(ref flashEffect);
        mEffectManager.destroyEffect(ref loopEffect);
        offsetPos = default;
    }
}
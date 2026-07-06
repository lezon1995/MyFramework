using UnityEngine;

namespace MarbleHero;

/// <summary>
/// Brick治疗效果
/// </summary>
public class BrickHealingEffect : ALogicEffect, IArgs<Brick, int>
{
    protected const string path = $"{GAMEPLAY_PATH}/Prefabs/FxParticle/FxBlockBoom.prefab";
    protected const float GAP = 0.5F;
    protected const float AFTER_DURATION = 1F;

    protected Brick brick;
    protected GameObject healingFx;
    protected int healingAmount;

    public void onCreate(Brick b, int amount)
    {
        duration = GAP;
        brick = b;
        healingAmount = amount;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        healingAmount = 0;
        brick = null;
        healingFx = null;
    }

    public override void destroy()
    {
        base.destroy();
        mPrefabPoolManager.destroyObject(healingFx, false);
    }

    public override bool fixedUpdate(float dt)
    {
        if (duration.unstarted)
        {
            healingFx = mPrefabPoolManager.createObject(path);
            healingFx.transform.position = brick.gameObject.transform.position;
            brick.heal(new(healingAmount));
        }

        base.fixedUpdate(dt);
        return isDone;
    }
}
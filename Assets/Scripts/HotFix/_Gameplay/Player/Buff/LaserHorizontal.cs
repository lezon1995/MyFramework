using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MarbleHero;

public class LaserHorizontal : Buff, IDoAttackEffect
{
    Comparison<RaycastHit2D> comparison;

    public LaserHorizontal()
    {
        comparison = Comparison;
    }

    public override void resetProperty()
    {
        base.resetProperty();
        // comparison = null;
        triggerPos = default;
    }

    protected float getChance()
    {
        return level switch
        {
            1 => 0.5F,
            2 => 0.5F,
            3 => 0.5F,
            4 => 0.5F,
            5 => 0.5F,
            _ => 0,
        };
    }

    Vector2 triggerPos;

    public void onDoAttack(APlayer player, Ball ball, Brick brick)
    {
        var chance = getChance();
        if (randomHit(chance))
        {
            var path = $"{GAMEPLAY_PATH}/Prefabs/FxParticle/FxLaser.prefab";
            var effect = mEffectManager.createEffect(path, 0.5F);
            var pos = brick.getWorldPosition();
            triggerPos = pos;
            effect.setWorldPosition(pos);
            effect.setRotationZ(90);

            using var _ = new ListScope2<RaycastHit2D>(out var listLeft, out var listRight);
            var left = new Vector2(levelManager.borderLeft.getWorldPosition().x, pos.y);
            var mid = pos;
            var right = new Vector2(levelManager.borderRight.getWorldPosition().x, pos.y);
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(BRICK_LAYER_MASK);
            var countLeft = Physics2D.Linecast(mid, left, filter, listLeft);
            var countRight = Physics2D.Linecast(mid, right, filter, listRight);
            if (countLeft > 0)
                listLeft.Sort(comparison);

            if (countRight > 0)
                listRight.Sort(comparison);

            UnityEngine.Pool.ListPool<Brick>.Get(out var leftBricks);
            for (var i = 0; i < listLeft.Count; i++)
            {
                var instanceID = listLeft[i].collider.gameObject.GetInstanceID();
                if (brickManager.getActiveBrick(instanceID, out var b) && brick != b)
                    leftBricks.Add(b);
            }

            UnityEngine.Pool.ListPool<Brick>.Get(out var rightBricks);
            for (var i = 0; i < listRight.Count; i++)
            {
                var instanceID = listRight[i].collider.gameObject.GetInstanceID();
                if (brickManager.getActiveBrick(instanceID, out var b) && brick != b)
                    rightBricks.Add(b);
            }

            var dmg = ball.getSkillDmg(brick);
            dmg.setCrit();
            gameplayManager.handleSkillDamage(ball, brick, ref dmg);

            if (leftBricks.Count > 0)
                startTask(leftBricks, ball).Forget();

            if (rightBricks.Count > 0)
                startTask(rightBricks, ball).Forget();
        }
    }

    static async UniTaskVoid startTask(List<Brick> list, Ball ball)
    {
        for (var i = 0; i < list.Count; i++)
        {
            await UniTask.WaitForSeconds(0.02F, delayTiming: PlayerLoopTiming.FixedUpdate);
            var brick = list[i];
            if (brick.isDead())
                continue;

            var dmg = ball.getSkillDmg(brick);
            dmg.setCrit();
            gameplayManager.handleSkillDamage(ball, brick, ref dmg);
        }

        UnityEngine.Pool.ListPool<Brick>.Release(list);
    }

    int Comparison(RaycastHit2D h1, RaycastHit2D h2)
    {
        var d1 = Vector2.Distance(triggerPos, h1.point);
        var d2 = Vector2.Distance(triggerPos, h2.point);
        return d1.CompareTo(d2);
    }
}
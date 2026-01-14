using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MarbleHero;

public class LaserVertical : Buff, IDoAttackEffect
{
    Comparison<RaycastHit2D> comparison;

    public LaserVertical()
    {
        comparison = Comparison;
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
            var effect = mEffectManager.createEffect(path, null, null, 0, true, true, 0.5F);
            var pos = brick.getWorldPosition();
            triggerPos = pos;
            effect.setWorldPosition(pos);
            effect.setRotationZ(0);

            using var _ = new ListScope2<RaycastHit2D>(out var listTop, out var listBot);
            var top = new Vector2(pos.x, levelManager.borderTop.getWorldPosition().y);
            var mid = pos;
            var bot = new Vector2(pos.x, levelManager.borderBot.getWorldPosition().y);
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(BRICK_LAYER_MASK);
            var countTop = Physics2D.Linecast(mid, top, filter, listTop);
            var countBot = Physics2D.Linecast(mid, bot, filter, listBot);
            if (countTop > 0)
                listTop.Sort(comparison);

            if (countBot > 0)
                listBot.Sort(comparison);

            UnityEngine.Pool.ListPool<Brick>.Get(out var topBricks);
            for (var i = 0; i < listTop.Count; i++)
            {
                var instanceID = listTop[i].collider.gameObject.GetInstanceID();
                if (brickManager.getActiveBrick(instanceID, out var b) && brick != b)
                    topBricks.Add(b);
            }

            UnityEngine.Pool.ListPool<Brick>.Get(out var botBricks);
            for (var i = 0; i < listBot.Count; i++)
            {
                var instanceID = listBot[i].collider.gameObject.GetInstanceID();
                if (brickManager.getActiveBrick(instanceID, out var b) && brick != b)
                    botBricks.Add(b);
            }
            
            var dmg = ball.getSkillDmg(brick);
            dmg.setCrit();
            gameplayManager.handleSkillDamage(ball, brick, ref dmg, out var killed);

            if (topBricks.Count > 0)
                startTask(topBricks, ball).Forget();

            if (botBricks.Count > 0)
                startTask(botBricks, ball).Forget();
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
            gameplayManager.handleSkillDamage(ball, brick, ref dmg, out var killed);
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
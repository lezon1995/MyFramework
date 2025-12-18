using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace MarbleHero;

/// <summary>
/// 造成致命撞击伤害时，对随机其他X个砖块造成连锁闪电攻击。
/// </summary>
public class LightingStrike3 : Buff, IDoAttackKillEffect
{
    protected int getCount()
    {
        return level switch
        {
            1 => 3,
            2 => 3,
            3 => 3,
            4 => 3,
            5 => 3,
            _ => 0,
        };
    }

    public void onDoAttackKill(Player player, Ball ball, Brick brick)
    {
        var count = getCount();
        UnityEngine.Pool.ListPool<Brick>.Get(out var list);
        if (brickManager.getRandomBricks(ref list, count, brick))
        {
            startTask(list, ball).Forget();
        }
    }

    static async UniTaskVoid startTask(List<Brick> list, Ball ball)
    {
        for (var i = 0; i < list.Count; i++)
        {
            await UniTask.WaitForSeconds(0.1F, delayTiming: PlayerLoopTiming.FixedUpdate);
            var randomBrick = list[i];
            if (randomBrick.isDead())
                continue;

            var dmg = ball.getAbilityDmg(randomBrick);
            dmg.setCrit();
            gameplayManager.handleAbilityDamage(ball, randomBrick, dmg);
        }

        UnityEngine.Pool.ListPool<Brick>.Release(list);
    }
}
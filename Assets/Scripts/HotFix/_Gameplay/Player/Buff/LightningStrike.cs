// namespace MoreMountains;
//
// /// <summary>
// /// 造成撞击伤害时，有X概率对随机1个其他砖块造成连锁闪电攻击。
// /// </summary>
// public class LightningStrike : BuffObject, IDoAttackEffect
// {
//     protected float getChance()
//     {
//         return level switch
//         {
//             1 => 0.5F,
//             2 => 0.5F,
//             3 => 0.5F,
//             4 => 0.5F,
//             5 => 0.5F,
//             _ => 0,
//         };
//     }
//
//     public void onDoAttack(APlayer player, Ball ball, Brick brick)
//     {
//         var chance = getChance();
//         if (randomHit(chance))
//         {
//             if (brickManager.getRandomActiveBrick(out var randomBrick, brick))
//             {
//                 var dmg = ball.getAbilityDmg(randomBrick);
//                 gameplayManager.handleAbilityDamage(ball, randomBrick, dmg);
//             }
//         }
//     }
// }
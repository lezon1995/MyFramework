using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 基础弹匣
/// 每次反弹有20%概率分裂出1个球，撞击1次砖块后销毁。
/// 【分裂】【撞击概率】
/// </summary>
public class BaseMagazine : ARelic, IEvent<OnBallDeath>
{
    public static string ID = "BaseMagazine";
    static List<Ball> tempBalls = new();

    public BaseMagazine() : base(ID, "BaseMagazine.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onBallReflect(APlayer p, Ball ball, Vector2 normal, bool fromBrick, ref Vector2 reflectDir)
    {
        if (tempBalls.contains(ball))
            return;
        
        var ranDir = randomDirectionInCone(normal, 60);
        var tempBall = ballManager.acquireBall(BallType.Normal, ball.curPos, ranDir);
        tempBall.setInitialHealth(1);
        tempBall.setBorderToBallDamageModifier(BALL_IMMUNE_TO_BORDER_DAMAGE_MODIFIER);
        tempBall.setTemp(true);
        tempBall.Event.addListener(this);
        tempBalls.add(tempBall);
    }

    /// <summary>
    /// 在direction左右maxAngle度范围内随机一个方向
    /// </summary>
    Vector2 randomDirectionInCone(Vector2 direction, float maxAngle)
    {
        if (direction == Vector2.zero)
            return Vector2.zero;

        float angle = randomFloat(-maxAngle, maxAngle);

        return Quaternion.Euler(0, 0, angle) * direction.normalized;
    }

    public override ARelic makeCopy()
    {
        return new BaseMagazine();
    }

    public void onEvent(OnBallDeath e)
    {
        var ball = e.ball;
        ball.Event.removeListener(this);
        tempBalls.Remove(ball);
    }
}
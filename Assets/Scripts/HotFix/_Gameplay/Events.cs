using UnityEngine;

namespace MarbleHero;

public struct DoAttackEffect
{
    public Brick brick;

    public DoAttackEffect(Brick b)
    {
        brick = b;
    }
}
public struct DoAbilityEffect
{
    public Brick brick;

    public DoAbilityEffect(Brick b)
    {
        brick = b;
    }
}
public struct OnHit
{
}
public struct OnDeath
{
    public OnDeath()
    {
    }
}
public struct DoDmgBrick
{
    public Brick brick;
    public Dmg dmg;

    public DoDmgBrick(Brick b, Dmg d)
    {
        brick = b;
        dmg = d;
    }
}
public struct DoDmgBall
{
    public Ball ball;
    public Dmg dmg;

    public DoDmgBall(Ball b, Dmg d)
    {
        ball = b;
        dmg = d;
    }
}
public struct OnDmg
{
    public Ball ball;
    public Dmg dmg;

    public OnDmg(Ball b, Dmg d)
    {
        ball = b;
        dmg = d;
    }
}
public struct DoKillBrick
{
    public Brick brick;
    public GameObject instigator;

    public DoKillBrick(Brick b, GameObject i)
    {
        brick = b;
        instigator = i;
    }
}
public struct DoKillBall
{
    public Ball ball;
    public GameObject instigator;

    public DoKillBall(Ball b, GameObject i)
    {
        ball = b;
        instigator = i;
    }
}
public struct OnBrickColliderChanged
{
}

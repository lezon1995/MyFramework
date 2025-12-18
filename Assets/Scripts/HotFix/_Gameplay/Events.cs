using UnityEngine;

namespace MarbleHero;

public struct DoAttackEffect
{
    public Ball ball;
    public Brick brick;

    public DoAttackEffect(Ball b1, Brick b2)
    {
        ball = b1;
        brick = b2;
    }
}

public struct DoAbilityEffect
{
    public Ball ball;
    public Brick brick;

    public DoAbilityEffect(Ball b1, Brick b2)
    {
        ball = b1;
        brick = b2;
    }
}

public struct OnHit
{
}

public struct OnBallDeath
{
}

public struct OnBallHitBorderBot
{
    public Ball ball;
    public OnBallHitBorderBot(Ball b) => ball = b;
}

public struct OnBrickDeath
{
    public Brick brick;
    public Vector3 deathPosition;
    public int combo;
    public OnBrickDeath(Brick b)
    {
        brick = b;
        deathPosition = b.getWorldPosition();
        combo = 0;
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


public struct DoAttackKillEffect
{
    public Ball ball;
    public Brick brick;
    public GameObject instigator;

    public DoAttackKillEffect(Ball b1, Brick b2, GameObject i)
    {
        ball = b1;
        brick = b2;
        instigator = i;
    }
}

public struct DoKillBrick
{
    public Ball ball;
    public Brick brick;
    public GameObject instigator;

    public DoKillBrick(Ball b1, Brick b2, GameObject i)
    {
        ball = b1;
        brick = b2;
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

public struct OnTurnChanged
{
    public int turn;
    public OnTurnChanged(int v) => turn = v;
}
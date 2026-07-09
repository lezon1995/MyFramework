using MoreMountains.Tools;
using UnityEngine;

namespace MarbleHero;

public struct DoHitEffect
{
    public Ball ball;
    public Brick brick;

    public DoHitEffect(Ball b1, Brick b2)
    {
        ball = b1;
        brick = b2;
    }
}

public struct DoSkillEffect
{
    public Ball ball;
    public Brick brick;

    public DoSkillEffect(Ball b1, Brick b2)
    {
        ball = b1;
        brick = b2;
    }
}

public struct OnHit
{
}

public struct OnHeal
{
}

public struct OnBallDeath
{
    public Ball ball;
    public OnBallDeath(Ball b)
    {
        ball = b;
    }
}

public struct OnBallDeathTotally
{
    public Ball ball;

    public OnBallDeathTotally(Ball b)
    {
        ball = b;
    }
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

public struct OnBrickDeathTotally
{
    public Brick brick;

    public OnBrickDeathTotally(Brick b)
    {
        brick = b;
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

public struct Turn
{
    public int value;

    public int increment()
    {
        var v = ++value;
        new OnTurnChanged(v).trigger();
        return v;
    }

    public void reset()
    {
        value = 0;
        new OnTurnChanged(value).trigger();
    }
    
    public static implicit operator int(Turn turn)
    {
        return turn.value;
    }
}

public struct OnTurnChanged
{
    public int turn;
    public OnTurnChanged(int v) => turn = v;
}
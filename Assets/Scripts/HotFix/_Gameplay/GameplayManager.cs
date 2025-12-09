using System;

namespace MarbleHero;

public class GameplayManager : FrameSystem
{
    IDmgCalculator dmgCalculator;

    public override void init()
    {
        base.init();
        dmgCalculator = DmgCalculator.Default;
    }

    public void handleHitDamage(Ball ball, Brick brick)
    {
        if (brick.canTakeDamageThisFrame(out var resistType))
        {
            var dmg = ball.getDmg(brick);
            brick.damage(dmg, ball.getObject(), ball, 0F, ball.getDirection(), dmgCalculator);
        }
        else
        {
            switch (resistType)
            {
                case ResistDamageType.None:
                    break;
                case ResistDamageType.Invulnerable:
                    break;
                case ResistDamageType.DashInvincible:
                    break;
                case ResistDamageType.ImmuneToDamage:
                    break;
                case ResistDamageType.Dead:
                    break;
                case ResistDamageType.Disabled:
                    break;
                case ResistDamageType.Dodged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (ball.getSelfDamage(brick, out var selfDamage))
        {
            var dmg = Dmg.trueDmg(selfDamage).setSelf();
            ball.damage(dmg, ball.getObject(), brick);
        }
    }
}
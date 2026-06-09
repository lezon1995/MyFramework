using System;

namespace MarbleHero;

public class GameplayManager : FrameSystem
{
    IDmgCalculator dmgCalculator;

    public int curPhase = 1;

    public override void init()
    {
        base.init();
        dmgCalculator = DmgCalculator.Default;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
    }

    public override void destroy()
    {
        base.destroy();
    }

    public void handleHitDamage(Ball ball, Brick brick, ref Dmg dmg)
    {
        if (brick.canTakeDamageThisFrame(out var resistType))
        {
            foreach (var p in ball.powers)
                p.onBeforeHandleHitDamage(ball, brick, ref dmg);

            brick.damage(ref dmg, ball.gameObject, ball, 0F, ball.getDirection(), dmgCalculator);
            if (dmg.isCrit)
                ball.onCritHit(brick);
            
            if (dmg.isLethal)
                ball.onHitKill(brick);
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
            var selfDmg = Dmg.trueDmg(selfDamage).setSelf();
            ball.damage(ref selfDmg, ball.gameObject, brick);
        }
    }

    public void handleSkillDamage(Ball ball, Brick brick, ref Dmg dmg)
    {
        if (brick.canTakeDamageThisFrame(out var resistType))
        {
            foreach (var p in ball.powers)
                p.onBeforeHandleSkillDamage(ball, brick, ref dmg);
            
            brick.damage(ref dmg, ball.gameObject, ball, 0F, ball.getDirection(), dmgCalculator);
            
            if (dmg.isCrit)
                ball.onCritHit(brick);
            
            if (dmg.isLethal)
                ball.onSkillKill(brick);
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
            var selfDmg = Dmg.trueDmg(selfDamage).setSelf();
            ball.damage(ref selfDmg, ball.gameObject, brick);
        }
    }

    public void refreshPhase(int phase)
    {
        curPhase = phase;
        var brickGrid = brickManager.brickLayout;
        brickGrid.getCellSize(out var cellSize);

        var borderLeftX = levelManager.getDefaultBorderLeftX();
        var borderRightX = levelManager.getDefaultBorderRightX();

        var defaultWidth = abs(borderLeftX - borderRightX);
        switch (phase)
        {
            case 1:
                levelManager.moveBorderLeftBy(-cellSize.x * 0);
                levelManager.moveBorderRightBy(cellSize.x * 0);
                brickGrid.setWidth(defaultWidth + cellSize.x * 0 * 2);
                brickGrid.setCols(6);
                break;
            case 2:
                levelManager.moveBorderLeftBy(-cellSize.x * 1);
                levelManager.moveBorderRightBy(cellSize.x * 1);
                brickGrid.setWidth(defaultWidth + cellSize.x * 1 * 2);
                brickGrid.setCols(8);
                break;
            case 3:
                levelManager.moveBorderLeftBy(-cellSize.x * 2);
                levelManager.moveBorderRightBy(cellSize.x * 2);
                brickGrid.setWidth(defaultWidth + cellSize.x * 2 * 2);
                brickGrid.setCols(10);
                break;
            case 4:
                levelManager.moveBorderLeftBy(-cellSize.x * 3);
                levelManager.moveBorderRightBy(cellSize.x * 3);
                brickGrid.setWidth(defaultWidth + cellSize.x * 3 * 2);
                brickGrid.setCols(12);
                break;
        }

        brickGrid.getGrids();
    }
}
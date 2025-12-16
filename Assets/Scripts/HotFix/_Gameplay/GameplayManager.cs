using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

public class GameplayManager : FrameSystem
{
    IDmgCalculator dmgCalculator;

    public bool isStart;
    public bool isGameOver;
    public int turnScore;
    public int turnCount = 1;
    public bool isContinue;
    public int comboCount;
    public bool isAllClear;

    bool _isLock;

    //Screen Drag Lock
    public bool isLock
    {
        get
        {
            if (!isStart)
                return true;

            if (playerManager.getPlayer().anyActiveBall())
                return true;

            if (isGameOver)
                return true;

            return _isLock;
        }
        set => _isLock = value;
    }

    List<BrickGroup> blockGroups = new();

    Action<BrickGroup> onBrickGroupClear;

    public GameplayManager()
    {
        onBrickGroupClear = releaseBrickGroup;
    }

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

    public IEnumerator startGame()
    {
        yield return new WaitForSeconds(0.01F);
        createBricksAtTopRow(turnCount);
        yield return new WaitForSeconds(0.5F);
        isStart = true;
        isLock = false;
    }

    public void createBricksAtTopRow(int turnNum)
    {
        // var brickGroup = CLASS<BrickGroup>(typeof(RandomTopRowBrickGroup));
        var brickGroup = CLASS<BrickGroup>(typeof(RandomAnyEmptyBrickGroup));
        brickGroup.setOnBricksClear(onBrickGroupClear);
        brickGroup.createBricks(turnNum);
        blockGroups.add(brickGroup);
    }

    void releaseBrickGroup(BrickGroup group)
    {
        UN_CLASS(group);
    }

    public void nextTurn()
    {
        isLock = true;
        turnCount += 1;
        // CtrUI.instance.SetTurn(turnCount);

        GameEntry.startCoroutine(nextTurnCo());
    }

    public IEnumerator nextTurnCo(float time = 0.2F)
    {
        // CtrUI.instance.AddScore(turnScore);

        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < blockGroups.Count; i++)
        {
            blockGroups[i].doNextTurnMove(time);
        }

        yield return new WaitForSeconds(time + 0.1F);
        //Create a single block
        createBricksAtTopRow(turnCount);
        //End of turn movement
        nextTurnMoveEnd();
    }

    public void nextTurnMoveEnd()
    {
        if (isGameOver)
            return;

        //All clear check
        // if (CtrUI.instance._ComboEffectText.isAllClear)
        {
            isAllClear = true;
            // CtrUI.instance._ComboEffectText.isAllClear = false;
        }
        // else
        {
            // CtrUI.instance._ComboEffectText.allClearCount = 0;
            isAllClear = false;
        }


        // CtrUI.instance.NextTurnReady();

        turnScore = 0;
        comboCount = 0;
        isLock = false;
        // Player.instance.guideLine.GuidelineOn();
    }
}
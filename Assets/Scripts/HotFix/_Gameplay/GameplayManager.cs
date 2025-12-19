using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MarbleHero;

public class GameplayManager : FrameSystem, IEvent<OnBrickDeath>
{
    IDmgCalculator dmgCalculator;

    public bool isStart;
    public bool isGameOver;
    public int turnScore;
    public int turnCount = 1;
    public bool isContinue;
    public int comboCount;
    public bool isAllClear;
    public int curPhase = 1;

    Camera mainCamera;
    Vector3 cameraInitialPosition;
    float cameraShakeTimer;
    float cameraShakePower;

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

    Queue<OnBrickDeath> brickDeathQueue = new();
    float brickDeathTimer;

    public GameplayManager()
    {
        onBrickGroupClear = releaseBrickGroup;
    }

    public override void init()
    {
        base.init();
        dmgCalculator = DmgCalculator.Default;

        mainCamera = mCameraManager.getMainCamera().getCamera();
        cameraInitialPosition = mainCamera.transform.localPosition;
        this.addListener<OnBrickDeath>();
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);

        handleCameraShake(elapsedTime);
        handleBrickDeathEvent(elapsedTime);
    }

    void handleCameraShake(float elapsedTime)
    {
        if (cameraShakeTimer > 0)
        {
            cameraShakeTimer = clampMin(cameraShakeTimer - elapsedTime, 0);
            Vector3 pos = cameraInitialPosition + Random.insideUnitSphere * cameraShakePower;
            pos.z = cameraInitialPosition.z;
            mainCamera.transform.localPosition = pos;

            if (cameraShakeTimer <= 0)
            {
                mainCamera.transform.localPosition = cameraInitialPosition;
            }
        }
    }

    public override void destroy()
    {
        base.destroy();

        this.removeListener<OnBrickDeath>();
    }

    public void handleAttackDamage(Ball ball, Brick brick)
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
            var selfDmg = Dmg.trueDmg(selfDamage).setSelf();
            ball.damage(selfDmg, ball.getObject(), brick);
        }
    }

    public void handleAbilityDamage(Ball ball, Brick brick, Dmg dmg)
    {
        if (brick.canTakeDamageThisFrame(out var resistType))
        {
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
            var selfDmg = Dmg.trueDmg(selfDamage).setSelf();
            ball.damage(selfDmg, ball.getObject(), brick);
        }
    }

    public IEnumerator startGame()
    {
        new OnTurnChanged(turnCount).trigger();
        yield return new WaitForSeconds(0.01F);
        createBricksAtTopRow(turnCount);
        yield return new WaitForSeconds(0.5F);
        isStart = true;
        isLock = false;
    }

    public void createBricksAtTopRow(int turnNum)
    {
        // var brickGroup = CLASS<TopRowRandomBrickGroup>();
        // var brickGroup = CLASS<RandomRowRandomBrickGroup>();
        var brickGroup = CLASS<RandomColRandomBrickGroup>();
        // var brickGroup = CLASS<RandomAnyEmptyBrickGroup>();
        brickGroup.setBrickManager(brickManager);
        brickGroup.setLevelManager(levelManager);
        brickGroup.setOnBricksClear(onBrickGroupClear);
        brickGroup.createBricks(turnNum);
        blockGroups.add(brickGroup);
    }

    void releaseBrickGroup(BrickGroup group)
    {
        blockGroups.Remove(group);
        UN_CLASS(group);
    }

    public void nextTurn()
    {
        isLock = true;
        ++turnCount;
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

        brickManager.refreshAllBrickGrid();
        
        //Create a single block
        createBricksAtTopRow(turnCount);

        playerManager.getPlayer().addExp(turnScore);
        new OnTurnChanged(turnCount).trigger();

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
        playerManager.getPlayer().getGuideLine().guidelineOn();
    }

    void handleBrickDeathEvent(float elapsedTime)
    {
        brickDeathTimer = clampMin(brickDeathTimer - elapsedTime);
        if (brickDeathTimer <= 0)
        {
            if (brickDeathQueue.TryDequeue(out var e))
            {
                brickDeathTimer = 0.15F;
                comboManager.createComboEffect(e.combo, e.deathPosition);

                //Camera shaking
                shakeCamera(e.combo * 0.02f);
            }
        }
    }

    public void onEvent(OnBrickDeath e)
    {
        var combo = ++comboCount;
        turnScore += combo * 10;
        e.combo = combo;

        brickDeathTimer = 0.15F;
        brickDeathQueue.Enqueue(e);
    }

    void shakeCamera(float power, float time = 0.2F)
    {
        cameraShakePower = power;
        cameraShakeTimer = time;
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
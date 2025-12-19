using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace MarbleHero;

public partial class Player : MovableObject
{
    public bool isReturnBall;
    public int ballMaxCount = 1;
    public int ballCount = 1;

    protected GuideLine guideLine;
    protected Exp exp;
    protected List<Ball> activeBalls = new();
    public Vector3 nextPosition;
    protected bool isFirstBallReturn;

    protected List<Buff> buffs = new();
    protected List<Type> ballBuffs = new();


    public override void init()
    {
        base.init();
        guideLine = CLASS<GuideLine>();
        guideLine.setObject(getGameObject("GuideLine", getObject()));
        guideLine.setName("GuideLine");
        guideLine.init();

        exp = CLASS<Exp>();
        var path = $"{GAMEPLAY_PATH}/ExpData.asset";
        var data = mResourceManager.loadGameResource<ExpData>(path);
        exp.setData(data);
        exp.resetLevel();

        nextPosition = getWorldPosition();
        setNextPositionX(nextPosition.x);

        // buffs.add(CLASS<LightingStrike>()).setBrickManager(brickManager);
        // buffs.add(CLASS<LightingStrike3>()).setBrickManager(brickManager);
        // buffs.add(CLASS<LaserHorizontal>()).setBrickManager(brickManager);
        // buffs.add(CLASS<LaserVertical>()).setBrickManager(brickManager);

        ballBuffs.add(typeof(LaserHorizontal));
        ballBuffs.add(typeof(LaserVertical));

        addListeners();
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);

        guideLine?.update(elapsedTime);
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);

        guideLine?.fixedUpdate(elapsedTime);
    }

    public override void destroy()
    {
        base.destroy();

        UN_CLASS(ref guideLine);

        removeListeners();
    }

    public GuideLine getGuideLine() => guideLine;

    public void shootBall(Vector3 shootPosition, Vector3 shootDirection)
    {
        // CtrUI.instance.SetReturnBallButton(true);


        isReturnBall = false;
        // shotRot.transform.position = guideLine.transform.position;
        // shotRot.transform.rotation = guideLine.transform.rotation;
        //
        gameplayManager.isLock = true;
        //
        GameEntry.startCoroutine(shootBallCo(shootPosition, shootDirection));
        guideLine?.guidelineOff();
    }

    public bool anyActiveBall()
    {
        return activeBalls.any();
    }

    IEnumerator shootBallCo(Vector3 shootPosition, Vector3 shootDirection)
    {
        guideLine.setIndicatorBallActive(false);

        for (int i = 0; i < ballMaxCount; i++)
        {
            //SoundManager.Instance.PlayEffect(Sound.sound_play_sfx_ball_launch);
            // CtrGame.instance.ShotSound();

            var ball = ballManager.acquireBall(shootPosition, 0.14F, shootDirection, 8F);
            foreach (var ballBuff in ballBuffs)
            {
                var buff = CLASS<Buff>(ballBuff);
                buff.setBrickManager(brickManager);
                ball.addBuff(buff);
            }
            // ball.setPenetrable(true);
            // ball.setHorizontalBorderTeleportable(true);

            ballCount -= 1;
            // CtrUI.instance.SetBallCount(ballCount);

            if (i == 0)
            {
                // ball.isFirst = true;
            }

            activeBalls.Add(ball);
            yield return new WaitForSeconds(0.05f);

            if (ballCount < 0)
                ballCount = 0;
        }

        yield return new WaitForSeconds(0.05f);
        // CtrUI.instance.textBallCount.DOFade(0f, 0.1f).SetEase(Ease.OutCubic);
        GameEntry.startCoroutine(checkTurnCo());
    }

    IEnumerator checkTurnCo()
    {
        while (activeBalls.Count > 0)
        {
            yield return null;
        }

        GameEntry.startCoroutine(readyPlayerCo());
    }

    IEnumerator readyPlayerCo()
    {
        // CtrUI.instance.SetReturnBallButton(false);
        // SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
        // CtrUI.instance.textBallCount.DOFade(1f, 0.1f).SetEase(Ease.OutCubic);
        // CtrUI.instance.textBallCount.transform.DOMoveX(nextPosition.x, 0f);

        //Initialize the number of balls
        ballCount = ballMaxCount;
        //Ball count UI applied
        // CtrUI.instance.SetBallCount(ballMaxCount);

        //Additional Ball Animation
        // for (int i = 0; i < addBallBlock.Count; i++)
        // {
        //     addBallBlock[i].transform.DOKill();
        //     addBallBlock[i].transform.DOMove(nextPosition, 0.1f);
        //     SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
        //     fxGet.Play();
        // }

        yield return new WaitForSeconds(0.15f);
        // if (addBallBlock.Count > 0)
        // {
        //     textGetBallCount.text = $"+{addBallBlock.Count}";
        //     textGetBallCount.transform.DOMove(nextPosition, 0f);
        //     textGetBallCount.DOFade(1f, 0f);
        //     textGetBallCount.transform.DOMoveY(0.5f, 0.2f).SetEase(Ease.OutCubic).SetRelative(true);
        //     textGetBallCount.DOFade(0f, 1f).SetEase(Ease.Linear).SetDelay(0.2f);
        // }

        //Delete added ball blocks
        // for (int i = 0; i < addBallBlock.Count; i++)
        // {
        //     addBallBlock[i].Destory();
        // }

        //Existing Ball += Added Ball
        // ballMaxCount += addBallBlock.Count;

        // if (randomHit(0.5F))
        // ballMaxCount++;

        ballCount = ballMaxCount;
        // CtrUI.instance.SetBallCount(ballMaxCount);

        //Initialize the list of added balls
        // addBallBlock.Clear();

        // inBall.Clear();
        isFirstBallReturn = false;
        gameplayManager.nextTurn();
    }

    public void returnBall()
    {
        // StopAllCoroutines();
        // CtrUI.instance.SetReturnBallButton(false);
        GameEntry.startCoroutine(returnBallCo());
    }

    IEnumerator returnBallCo()
    {
        for (int i = 0; i < activeBalls.Count; i++)
        {
            activeBalls[i].returnBall(nextPosition);
        }

        yield return new WaitForSeconds(0.25f);
        activeBalls.Clear();
        setNextPositionX(nextPosition.x);

        GameEntry.startCoroutine(readyPlayerCo());
    }

    public void setNextPositionX(float posX)
    {
        nextPosition.x = posX;
        guideLine.setShootPosition(nextPosition);
        guideLine.setIndicatorBallPosition(nextPosition);
        guideLine.setIndicatorBallActive(true);

        // SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
    }

    public void setBallReturn(Ball ball)
    {
        if (!isFirstBallReturn)
        {
            isFirstBallReturn = true;
            setNextPositionX(ball.getWorldPosition().x);
            activeBalls.Remove(ball);
            ballManager.releaseBall(ball);
            return;
        }

        ball.setEnabled(false);
        activeBalls.Remove(ball);
        Tween
            .Position(ball.getTransform(), endValue: nextPosition, duration: 0.15F, ease: Ease.OutCubic)
            .OnComplete(ball, b =>
            {
                ballManager.releaseBall(b);
            });

        return;
    }

    public void addExp(int turnScore)
    {
        exp.addXp(turnScore);
    }
}
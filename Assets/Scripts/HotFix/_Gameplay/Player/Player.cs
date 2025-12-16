using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

public class Player : MovableObject
{
    public bool isReturnBall;
    public int ballMaxCount = 1;
    public int ballCount = 1;

    protected GuideLine guideLine;
    protected List<Ball> activeBalls = new();
    public Vector3 nextPosition;
    public bool isFirstBallReturn;

    public override void init()
    {
        base.init();
        guideLine = CLASS<GuideLine>();
        guideLine.setObject(getGameObject("GuideLine", getObject()));
        guideLine.init();
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        
        guideLine.update(elapsedTime);
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);
        
        guideLine.fixedUpdate(elapsedTime);
    }

    public override void destroy()
    {
        base.destroy();
        
        UN_CLASS(ref guideLine);
    }

    public void shotBall()
    {
        // CtrUI.instance.SetReturnBallButton(true);


        isReturnBall = false;
        // shotRot.transform.position = guideLine.transform.position;
        // shotRot.transform.rotation = guideLine.transform.rotation;
        //
        gameplayManager.isLock = true;
        //
        GameEntry.startCoroutine(shotBallCo());
        guideLine.GuidelineOff();
    }

    public bool anyActiveBall()
    {
        return activeBalls.any();
    }

    IEnumerator shotBallCo()
    {
        // center.SetActive(false);

        Vector3 shotPos = guideLine.getWorldPosition();
        var direction = guideLine.getTransform().up.normalized;

        for (int i = 0; i < ballMaxCount; i++)
        {
            //SoundManager.Instance.PlayEffect(Sound.sound_play_sfx_ball_launch);
            // CtrGame.instance.ShotSound();

            var ball = ballManager.createBall<NormalBall>("Ball_0", shotPos, 0.14F, direction, 6F);
            ballCount -= 1;
            // CtrUI.instance.SetBallCount(ballCount);

            if (i == 0)
            {
                // ball.isFirst = true;
            }

            activeBalls.Add(ball);
            yield return new WaitForSeconds(0.035f);

            if (ballCount < 0)
                ballCount = 0;
        }

        yield return new WaitForSeconds(0.035f);
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
            // activeBalls[i].returnBall();
        }

        yield return new WaitForSeconds(0.25f);
        activeBalls.Clear();
        setNextPositionX(nextPosition.x);

        GameEntry.startCoroutine(readyPlayerCo());
    }
    
    public void setNextPositionX(float posX)
    {
        nextPosition.x = posX;
        guideLine.setPositionX(posX);
        guideLine.setWorldRotation(Quaternion.identity);
        // center.gameObject.SetActive(true);

        // SoundManager.Instance.PlayEffect(SoundList.sound_play_sfx_ball_comback);
    }
}
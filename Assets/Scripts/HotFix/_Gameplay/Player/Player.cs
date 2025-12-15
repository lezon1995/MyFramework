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
        gameplayManager.IsLock = true;
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

        for (int i = 0; i < ballMaxCount; i++)
        {
            //SoundManager.Instance.PlayEffect(Sound.sound_play_sfx_ball_launch);
            // CtrGame.instance.ShotSound();

            var ball = ballManager.createBall<NormalBall>("Ball_0", shotPos, 0.14F, Random.insideUnitCircle, 6F);
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
        // StartCoroutine(CheckTurnCo());
    }
}
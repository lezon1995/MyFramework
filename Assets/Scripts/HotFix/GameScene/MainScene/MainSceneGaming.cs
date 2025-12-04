using MarbleHero;
using UnityEngine;

public class MainSceneGaming : SceneProcedure
{
    protected Ball mBall;

    protected override void onInit(SceneProcedure lastProcedure)
    {
        mBall = ballManager.createBall<NormalBall>("Ball");
        // LT.LOAD<UIGaming>();
    }

    protected override void onUpdate(float elapsedTime)
    {
        base.onUpdate(elapsedTime);

        if (isKeyCurrentDown(KeyCode.I))
        {
            var v = Random.insideUnitCircle;
            mBall.setDirection(v);
        }
    }


    protected override void onExit(SceneProcedure nextProcedure)
    {
        // LT.HIDE<UIGaming>();
        ballManager?.destroyBall(mBall);
    }
}
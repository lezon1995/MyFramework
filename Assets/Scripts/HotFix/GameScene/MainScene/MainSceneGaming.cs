using MarbleHero;
using UnityEngine;

public class MainSceneGaming : SceneProcedure
{
    protected Ball mBall;


    protected override void onInit(SceneProcedure lastProcedure)
    {
        mBall = ballManager.createBall<NormalBall>("normal");
        // LT.LOAD<UIGaming>();

        var go = getRootGameObject("Sphere");
        mBall.setObject(go);
    }

    protected override void onUpdate(float elapsedTime)
    {
        base.onUpdate(elapsedTime);

        if (isKeyCurrentDown(KeyCode.I))
        {
            var v = Random.insideUnitCircle;
            mBall.setDirection(new(v.x, 0, v.y));
        }
    }


    protected override void onExit(SceneProcedure nextProcedure)
    {
        // LT.HIDE<UIGaming>();
        ballManager?.destroyBall(mBall);
    }
}
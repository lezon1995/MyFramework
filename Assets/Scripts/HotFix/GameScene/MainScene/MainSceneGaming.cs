using MarbleHero;
using UnityEngine;

public class MainSceneGaming : SceneProcedure
{
    protected SafeList<Ball> balls;

    protected override void onInit(SceneProcedure lastProcedure)
    {
        balls = CLASS<SafeList<Ball>>();
        var ball = ballManager.createBall<NormalBall>("Ball", new(3, 3), 0.1F, new(1, 1), 6F);
        balls.add(ball);
        // LT.LOAD<UIGaming>();
    }

    protected override void onUpdate(float elapsedTime)
    {
        base.onUpdate(elapsedTime);

        if (isKeyCurrentDown(KeyCode.I))
        {
            using var a = new SafeListReader<Ball>(balls);
            foreach (var ball in a.mReadList)
            {
                var v = Random.insideUnitCircle;
                ball.setDirection(v);
            }
        }

        if (isKeyCurrentDown(KeyCode.A))
        {
            var mousePosition = getMousePosition();
            var worldPos = screenToWorld(mousePosition, false);
            var ball = ballManager.createBall<NormalBall>("Ball", worldPos, 0.1F, Random.insideUnitCircle, 6F);
            balls.add(ball);
        }
        if (isKeyCurrentDown(KeyCode.B))
        {
            var mousePosition = getMousePosition();
            var worldPos = screenToWorld(mousePosition, false);
            var brick = brickManager.createBrick<NormalBrick>("Brick", worldPos);
            // balls.add(ball);
        }
    }

    protected override void onExit(SceneProcedure nextProcedure)
    {
        // LT.HIDE<UIGaming>();
        balls.clear();
        ballManager?.destroyAllBall();
    }
}
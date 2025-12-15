using MarbleHero;
using UnityEngine;

public class MainSceneGaming : SceneProcedure
{
    protected SafeList<Ball> balls;

    protected override void onInit(SceneProcedure lastProcedure)
    {
        balls = CLASS<SafeList<Ball>>();
        playerManager.createPlayer<Player>("Player");
        // var ball = ballManager.createBall<NormalBall>("Ball_0", new(3, 3), 0.14F, new(1, 1), 6F);
        // balls.add(ball);
        // LT.LOAD<UIGaming>();

        GameEntry.startCoroutine(gameplayManager.startGame());
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
            var ball = ballManager.createBall<NormalBall>("Ball_0", worldPos, 0.14F, Random.insideUnitCircle, 6F);
            balls.add(ball);
        }

        if (isKeyCurrentDown(KeyCode.B))
        {
            var mousePosition = getMousePosition();
            var worldPos = screenToWorld(mousePosition, false);
            var rect = brickManager.brickGrid.getRectAtPos(worldPos);
            // var brick = brickManager.createBrick<NormalBrick>("Brick", worldPos, new(1.14F, 0.82F), 20);
            var brick = brickManager.createBrick<NormalBrick>("Brick", rect.center, rect.size, 20);
            // balls.add(ball);
        }

        if (isKeyCurrentDown(KeyCode.N))
        {
            GameEntry.startCoroutine(gameplayManager.nextTurn());
        }
    }

    protected override void onExit(SceneProcedure nextProcedure)
    {
        // LT.HIDE<UIGaming>();
        balls.clear();
        ballManager?.destroyAllBall();
    }
}
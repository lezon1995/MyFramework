using MarbleHero;
using UnityEngine;

public class MainSceneGaming : SceneProcedure
{
    protected SafeList<Ball> balls;

    protected override void onInit(SceneProcedure lastProcedure)
    {
        LT.LOAD<GameplayPanel>();

        balls = CLASS<SafeList<Ball>>();
        playerManager.createPlayer<Player>("Player");

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
            gameplayManager.nextTurn();
        }

        if (isKeyCurrentDown(KeyCode.R))
        {
            playerManager.getPlayer().returnBall();
        }

        if (isKeyCurrentDown(KeyCode.P))
        {
            var phase = gameplayManager.curPhase;
            gameplayManager.refreshPhase((phase % 4) + 1);
        }
    }

    protected override void onExit(SceneProcedure nextProcedure)
    {
        LT.HIDE<GameplayPanel>();
        balls.clear();
        ballManager?.destroyAllBall();
    }
}
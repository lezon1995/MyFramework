using MarbleHero;
using UnityEngine;

/// <summary>
/// 游戏启动后进入到主菜单页面，一直到游戏退出之间的流程
/// 游戏不分局内局外，统一用一个GameInstance实例来管理
/// </summary>
public class MainSceneGaming : SceneProcedure
{
    MarbleHero.Game gameInstance;

    protected SafeList<Ball> balls;

    protected override void onInit(SceneProcedure lastProcedure)
    {
        gameInstance = new MarbleHero.Game();
        gameInstance.create();

        SeedHelper.setSeed("3Q350M8RNTUM4");

        balls = CLASS<SafeList<Ball>>();
        // GameEntry.startCoroutine(gameplayManager.startGame());
        mGameFrameworkHotFix.registeOnApplicationPause(onApplicationPause);
    }

    void onApplicationPause(bool pause)
    {
        if (pause)
            gameInstance.pause();
        else
            gameInstance.resume();
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
            var ball = ballManager.acquireBall(worldPos, 0.14F, Random.insideUnitCircle, 6F);
            balls.add(ball);
        }

        if (isKeyCurrentDown(KeyCode.B))
        {
            var mousePosition = getMousePosition();
            var worldPos = screenToWorld(mousePosition, false);
            var rect = brickManager.brickLayout.getRectAtPos(worldPos);
            // var brick = brickManager.showBrick(worldPos, new(1.14F, 0.82F), 20);
            var brick = brickManager.acquireBrick(rect.center, rect.size, 20);
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

        gameInstance.update(elapsedTime);
    }

    protected override void onFixedUpdate(float elapsedTime)
    {
        base.onFixedUpdate(elapsedTime);

        gameInstance.fixedUpdate(elapsedTime);
    }

    protected override void onExit(SceneProcedure nextProcedure)
    {
        mGameFrameworkHotFix.unregisteOnApplicationPause(onApplicationPause);
        gameInstance.dispose();
        gameInstance = null;
        balls.clear();
        ballManager?.destroyAllBall();
    }
}
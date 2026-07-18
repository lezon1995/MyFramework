using MoreMountains;
using UnityEngine;
using static FrameUtility;
using static FrameBaseHotFix;
using static LT;

/// <summary>
/// 游戏启动后进入到主菜单页面，一直到游戏退出之间的流程
/// 游戏不分局内局外，统一用一个GameInstance实例来管理
/// </summary>
public class MainSceneGaming : SceneProcedure
{
    MoreMountains.Game gameInstance;

    protected override void onInit(SceneProcedure lastProcedure)
    {
        gameInstance = new();
        gameInstance.create();

        SeedHelper.setSeed("3Q350M8RNTUM4");

        mGameFrameworkHotFix.registeOnApplicationPause(onApplicationPause);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        gameInstance.dispose();
        gameInstance = null;
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

        if (isKeyCurrentDown(KeyCode.B))
        {
            var mousePosition = getMousePosition();
            var worldPos = screenToWorld(mousePosition, false);
            // var rect = brickManager.brickLayout.getRectAtPos(worldPos);
            // var brick = brickManager.showBrick(worldPos, new(1.14F, 0.82F), 20);
            var brick = brickManager.acquireBrick(worldPos, new(1,1));
            // balls.add(ball);
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
        ballManager?.destroyAllBall();
    }
}
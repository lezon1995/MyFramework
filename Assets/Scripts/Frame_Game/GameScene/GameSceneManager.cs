// 逻辑场景管理器
public class GameSceneManager : FrameSystem
{
    protected GameScene curScene; // 当前场景
    public GameScene CurScene => curScene;

    public void enterScene<T>() where T : GameScene, new()
    {
        if (curScene != null)
            return;

        curScene = new T();
        curScene.init();
    }

    public override void update(float dt)
    {
        base.update(dt);
        curScene?.update(dt);
    }

    public override void destroy()
    {
        curScene?.exit();
        curScene?.destroy();
        curScene = null;
        base.destroy();
    }

    public override void willDestroy()
    {
        base.willDestroy();
        curScene?.willDestroy();
    }
}
namespace MoreMountains;

public class SelectDifficultyPhase : ADungeonPhase
{
    public SelectDifficultyPhase(ADungeon dungeon) : base(dungeon)
    {
    }

    public override void onBegin(ADungeonPhase last)
    {
        base.onBegin(last);
        log("进入 难度选择阶段 测试阶段 自动跳过");
    }

    public override void onEnd()
    {
        base.onEnd();
    }

    public override void update(float dt)
    {
        base.update(dt);
    }

    public override void fixedUpdate(float dt)
    {
        base.fixedUpdate(dt);
    }

    protected override void onBindListeners()
    {
    }

    protected override void onUnbindListeners()
    {
    }
}
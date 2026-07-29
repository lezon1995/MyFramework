namespace MoreMountains;

public class BattlePassCleanupPhase : ARoomPhase
{
    Timer timer;

    public BattlePassCleanupPhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(ARoomPhase last)
    {
        base.onBegin(last);
        log("进入 关卡通关阶段 测试阶段 1秒后自动跳过");
        timer = 1F;
    }

    public override void onEnd()
    {
        base.onEnd();
    }

    public override void update(float dt)
    {
        base.update(dt);
        if (timer.update(dt))
        {
            _room.ToPhase = RoomPhaseType.LEVEL_UP_REWARD;
        }
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
namespace MoreMountains;

public class PreparePhase : ARoomPhase
{
    Timer timer;

    public PreparePhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(ARoomPhase last)
    {
        base.onBegin(last);
        log("进入 战前准备阶段 测试阶段 1秒后自动跳过");
        timer = 1F;
        
        wave.StartGame(_room.waveLevelConfig);
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
            _room.ToPhase = RoomPhaseType.BATTLE;
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
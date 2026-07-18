namespace MoreMountains;

public class SelectWeaponPhase : APhase
{
    public SelectWeaponPhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(APhase last)
    {
        base.onBegin(last);
        log("进入 武器选择阶段 测试阶段 自动跳过");
    }

    public override void onEnd()
    {
        base.onEnd();
    }

    public override void update(float dt)
    {
        base.update(dt);
        _room.ToPhase = RoomPhaseType.SELECT_DIFFICULTY;
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
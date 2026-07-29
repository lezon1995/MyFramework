namespace MoreMountains;

public class SelectCharacterPhase : ADungeonPhase
{
    CharSelectInfo _charSelectInfo;

    public SelectCharacterPhase(ADungeon dungeon) : base(dungeon)
    {
    }

    public override void onBegin(ADungeonPhase last)
    {
        base.onBegin(last);
        log("进入 角色选择阶段 测试阶段 自动跳过");
        var panel = LT.LOAD<SelectPlayerPanel>();
        panel.setOnNextStepClick(() =>
        {
            // _dungeon.ToPhase = DungeonPhaseType.SELECT_WEAPON;
        });
        panel.setOnSubmitCharacterSelectInfo(charSelectInfo =>
        {
            _charSelectInfo = charSelectInfo;
        });
    }

    public override void onEnd()
    {
        base.onEnd();
        LT.HIDE<SelectPlayerPanel>();
    }

    public override void update(float dt)
    {
        base.update(dt);

        if (_charSelectInfo != null)
        {
            _dungeon.endPhase();
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
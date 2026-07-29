namespace MoreMountains;

public class FightingPhase : ARoomPhase
{
    public FightingPhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(ARoomPhase last)
    {
        base.onBegin(last);
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

    public override void onEnd()
    {
        base.onEnd();
        player.onFightingPhaseEnd();
    }
}
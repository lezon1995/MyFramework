namespace MoreMountains;

public class PlayerTurnPhase : APhase
{
    public PlayerTurnPhase(MonsterRoom room) : base(room)
    {
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    protected override void onBindListeners()
    {
    }

    protected override void onUnbindListeners()
    {
    }

    public override void onBegin(APhase last)
    {
        base.onBegin(last);
        player.onPlayerTurnBegin();
    }

    public override void update(float dt)
    {
        base.update(dt);
        player.onPlayerTurnUpdate(dt);
    }

    public override void fixedUpdate(float dt)
    {
        base.fixedUpdate(dt);
    }

    public override void onEnd()
    {
        base.onEnd();
        player.onPlayerTurnEnd();
    }
}
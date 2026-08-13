namespace MoreMountains;

public class ShoppingPhase : ARoomPhase
{
    public ShoppingPhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(ARoomPhase last)
    {
        base.onBegin(last);
        OperationPanelService.Instance.Binder.EnterShop();
    }

    public override void onEnd()
    {
        wave.ConfirmRewardSelection();
        OperationPanelService.Instance.Close();
        base.onEnd();
    }

    public override void update(float dt)
    {
        base.update(dt);
        ADungeon.operationPanel.update(dt);
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
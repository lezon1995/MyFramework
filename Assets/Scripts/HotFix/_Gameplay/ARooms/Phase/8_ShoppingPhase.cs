namespace MoreMountains;

public class ShoppingPhase : ARoomPhase
{
    public ShoppingPhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(ARoomPhase last)
    {
        base.onBegin(last);
        var waveNumber = _room.waveGameMode.waveManager.WaveNumber;
        OperationPanelService.Instance.Binder.EnterShop(waveNumber);
    }

    public override void onEnd()
    {
        wave.ConfirmRewardSelection();
        OperationPanelService.Instance.Close();
        
        OverlayMenuService.Instance.Open(_room, _room.Player);
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
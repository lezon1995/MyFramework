namespace MoreMountains;

public class LevelUpRewardPhase : ARoomPhase
{
    public LevelUpRewardPhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(ARoomPhase last)
    {
        base.onBegin(last);
        // 问题1修复：进入奖励选择阶段，由外部调用ExitRewardSelection来开始下一波
        wave.waveManager.EnterRewardSelection();
        
        OverlayMenuService.Instance.Close();
        
        // 通过全局 Service 打开 OperationPanel；面板内部已经绑定或会重绑当前玩家。
        OperationPanelService.Instance.Open(_room, _room.Player);
        var waveNumber = _room.waveGameMode.waveManager.WaveNumber;
        OperationPanelService.Instance.Binder.EnterReward(waveNumber);
        OperationPanelService.Instance.Binder.RewardChoose.OfferBuyClicked += OnRewardItemChosen;
    }

    void OnRewardItemChosen(IPurchasable obj)
    {
        OperationPanelService.Instance.Binder.RewardChoose.OfferBuyClicked -=  OnRewardItemChosen;
        _room.ToPhase = RoomPhaseType.SHOPPING;
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
namespace MoreMountains;

public partial class OperationPanel
{
    /// <summary>直接访问 auto-gen 的子节点。</summary>
    public RewardChooseView RewardChoose           => rewardChooseView;
    public ShopView Shop           => shopView;
    public PlayerInfoView PlayerInfo => playerInfoView;
    public WaveMonsterView WaveMonster => waveMonsterView;
    public RelicInventoryView RelicInventory => relicInventoryView;
    public BallInventoryView BallInventory => ballInventoryView;

    public myUGUITextTMP Title     => textTitle;
    public myUGUIButton  BtnNext  => btnNext;
    public myUGUITextTMP BtnLabel => textBtn;

    public void RefreshTitle(int waveNumber)
    {
        _stringTitle.setInt("waveNumber", waveNumber);
        _stringNextWaveBtn.setInt("waveNumber", waveNumber);
    }

    OperationPanelBinder binder;

    void initBinder()
    {
        binder = new(
            this,
            ballInventoryView.initBinder(),
            relicInventoryView.initBinder(),
            playerInfoView.SlotGroup.initBinder(),
            shopView.initBinder(),
            rewardChooseView.initBinder(),
            playerInfoView.initBinder(),
            waveMonsterView.initBinder()
        );

        OperationPanelService.Instance.Register(binder);
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        BallOperationStateManager.Instance.Update();
        RelicOperationStateManager.Instance.Update();
    }

    public override void onGameState()
    {
        base.onGameState();
		
        // 在面板激活时创建全屏 Blocker GameObject
        // Blocker 透明且默认关闭,在操作状态激活时开启以屏蔽其他 UI
        CreateBlocker();
    }

    void CreateBlocker()
    {
        if (_blockerGO != null) return;

        var go = new UnityEngine.GameObject("BallOpBlocker");

        // 放到当前 canvas 下
        var parent = mRoot?.getGameObject()?.transform?.parent;
        if (parent != null)
            go.transform.SetParent(parent, false);

        // 全屏覆盖
        var rt = go.AddComponent<UnityEngine.RectTransform>();
        rt.anchorMin = UnityEngine.Vector2.zero;
        rt.anchorMax = UnityEngine.Vector2.one;
        rt.sizeDelta = UnityEngine.Vector2.zero;
        rt.anchoredPosition = UnityEngine.Vector2.zero;

        // 透明背景
        var img = go.AddComponent<UnityEngine.UI.Image>();
        img.color = new UnityEngine.Color(0, 0, 0, 0);

        // CanvasGroup 控制 raycast
        var cg = go.AddComponent<UnityEngine.CanvasGroup>();
        cg.blocksRaycasts = false;

        // 添加 BlockerController,自动注册 Blocker 到 BallOperationStateManager
        go.AddComponent<BlockerController>();
        // 添加 RelicBlockerController,自动注册 Blocker 到 RelicOperationStateManager
        go.AddComponent<RelicBlockerController>();

        _blockerGO = go;
        go.SetActive(false);
    }

    UnityEngine.GameObject _blockerGO;
}

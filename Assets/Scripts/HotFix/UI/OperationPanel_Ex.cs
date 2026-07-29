namespace MoreMountains;

public partial class OperationPanel
{
    /// <summary>直接访问 auto-gen 的 ShopView 子节点，供 binder 创建更细粒度的子 View。</summary>
    public ShopView Shop       => shopView;
    public PlayerInfoView PlayerInfo => playerInfoView;
    public RelicInventoryView RelicInventory => relicInventoryView;
    public BallInventoryView BallInventory => ballInventoryView;

    public myUGUITextTMP Title     => textTitle;
    public myUGUIButton  BtnNext   => btnNext;
    public myUGUITextTMP BtnLabel  => textBtn;

    public void SetTitle(string s) => textTitle.setText(s ?? string.Empty);
    public void SetBtnLabel(string s) => textBtn.setText(s ?? string.Empty);

    OperationPanelBinder binder;
    
    void initBinder()
    {
        binder = new(
            this, 
            ballInventoryView.initBinder(),
            relicInventoryView.initBinder(),
            playerInfoView.SlotGroup.initBinder(),
            shopView.initBinder(),
            playerInfoView.initBinder()
            );
        
        OperationPanelService.Instance.Register(this, binder);
    }
}

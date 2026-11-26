namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/EscPanel.prefab
// 
public partial class MenuOptionView : WindowObjectUGUI
// auto generate classname end
{
    EscPanel panel;
    public void SetPanel(EscPanel owner) => panel = owner;
    // auto generate member start
	protected myUGUIButton btnContinue;
	protected myUGUIButton btnRestart;
	protected myUGUIButton btnSettings;
	protected myUGUIButton btnReturnMainMenu;
    // auto generate member end
    public MenuOptionView(IWindowObjectOwner parent) : base(parent)
    {
        // auto generate constructor start
        // auto generate constructor end
    }

    protected override void assignWindowInternal()
    {
        // auto generate assignWindowInternal start
		newObject(out btnContinue, "V/BtnContinue");
		newObject(out btnRestart, "V/BtnRestart");
		newObject(out btnSettings, "V/BtnSettings");
		newObject(out btnReturnMainMenu, "V/BtnReturnMainMenu");
        // auto generate assignWindowInternal end
    }

    public override void init()
    {
        base.init();
        // auto generate init start
        // auto generate init end
        btnContinue.setUGUIButtonClick(onBtnContinueClick);
        btnRestart.setUGUIButtonClick(onBtnRestartClick);
        btnSettings.setUGUIButtonClick(onBtnSettingsClick);
        btnReturnMainMenu.setUGUIButtonClick(onBtnReturnMainMenuClick);
    }

    void onBtnContinueClick()
    {
        EscPanelService.Instance.Close();
    }

    void onBtnRestartClick()
    {
        panel.ShowRestartConfirmation();
    }

    void onBtnSettingsClick()
    {
        panel.ShowSettings();
    }

    void onBtnReturnMainMenuClick()
    {
        panel.ShowReturnConfirmation();
    }

    public override void onShow()
    {
        base.onShow();
    }
}

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class PlayerInfoView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUITextTMP textLevel;
	protected myUGUISlider expSlider;
	protected myUGUITextTMP textCurExp;
	protected myUGUITextTMP textMaxExp;
	protected myUGUIObject itemParent;
	protected BallSlotGroupView ballSlotGroupView;
	protected WindowStructPool<PlayerStatItem> PlayerStatItemPool;
	// auto generate member end
	public PlayerInfoView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		ballSlotGroupView = new(this);
		PlayerStatItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out textLevel, "PlayerStats/Level/TextLevel");
		newObject(out expSlider, "PlayerStats/Level/ExpBar");
		newObject(out textCurExp, "PlayerStats/Level/ExpBar/Exp/TextCur");
		newObject(out textMaxExp, "PlayerStats/Level/ExpBar/Exp/TextMax");
		newObject(out itemParent, "PlayerStats/V");
		ballSlotGroupView.assignWindow(mRoot, "BallSlotGroupView");
		PlayerStatItemPool.assignTemplate(mRoot, "PlayerStats/V/PlayerStatItem");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
	}
	public override void onShow()
	{
		base.onShow();
	}
}

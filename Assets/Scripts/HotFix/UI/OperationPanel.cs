using Obfuz;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class OperationPanel : LayoutScript
// auto generate classname end
{
	// auto generate member start
	protected RewardChooseView rewardChooseView;
	protected ShopView shopView;
	protected PlayerInfoView playerInfoView;
	protected RelicInventoryView relicInventoryView;
	protected BallInventoryView ballInventoryView;
	protected myUGUITextTMP textTitle;
	protected myUGUIButton btnNext;
	protected myUGUITextTMP textBtn;
	// auto generate member end
	public OperationPanel()
	{
		// auto generate constructor start
		rewardChooseView = new(this);
		shopView = new(this);
		playerInfoView = new(this);
		relicInventoryView = new(this);
		ballInventoryView = new(this);
		// auto generate constructor end
		mNeedUpdate = false;
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		rewardChooseView.assignWindow(mRoot, "RewardChooseView");
		shopView.assignWindow(mRoot, "ShopView");
		playerInfoView.assignWindow(mRoot, "PlayerInfoView");
		relicInventoryView.assignWindow(mRoot, "InventoryView/RelicInventoryView");
		ballInventoryView.assignWindow(mRoot, "InventoryView/BallInventoryView");
		newObject(out textTitle, "Title/TextTitle");
		newObject(out btnNext, "BtnNext");
		newObject(out textBtn, "BtnNext/TextBtn");
		// auto generate assignWindow end
	}
	public override void init()
	{
		initBinder();
		base.init();
		// auto generate init start
		// auto generate init end
	}
}

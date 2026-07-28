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
	protected ShopView shopView;
	protected PlayerInfoView playerInfoView;
	protected RelicInventoryView relicInventoryView;
	protected BallInventoryView ballInventoryView;
	// auto generate member end
	public OperationPanel()
	{
		// auto generate constructor start
		shopView = new(this);
		playerInfoView = new(this);
		relicInventoryView = new(this);
		ballInventoryView = new(this);
		// auto generate constructor end
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		shopView.assignWindow(mRoot, "ShopView");
		playerInfoView.assignWindow(mRoot, "PlayerInfoView");
		relicInventoryView.assignWindow(mRoot, "InventoryView/RelicInventoryView");
		ballInventoryView.assignWindow(mRoot, "InventoryView/BallInventoryView");
		// auto generate assignWindow end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
	}
	public override void onGameState()
	{
		base.onGameState();
	}
}

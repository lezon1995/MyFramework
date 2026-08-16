using Obfuz;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/EscPanel.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class EscPanel : LayoutScript
// auto generate classname end
{
	// auto generate member start
	protected PlayerInfoView playerInfoView;
	protected BallInventoryView ballInventoryView;
	protected RelicInventoryView relicInventoryView;
	protected MenuOptionView menuOptionView;
	protected WaveMonsterView waveMonsterView;
	// auto generate member end
	public EscPanel()
	{
		// auto generate constructor start
		playerInfoView = new(this);
		ballInventoryView = new(this);
		relicInventoryView = new(this);
		menuOptionView = new(this);
		waveMonsterView = new(this);
		// auto generate constructor end
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		playerInfoView.assignWindow(mRoot, "PlayerInfoView");
		ballInventoryView.assignWindow(mRoot, "InventoryView/BallInventoryView");
		relicInventoryView.assignWindow(mRoot, "InventoryView/RelicInventoryView");
		menuOptionView.assignWindow(mRoot, "MenuOptionView");
		waveMonsterView.assignWindow(mRoot, "WaveMonsterView");
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

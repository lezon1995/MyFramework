using Obfuz;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class OverlayMenu : LayoutScript
// auto generate classname end
{
	// auto generate member start
	protected CharacterInfoView characterInfoView;
	protected BallTooltipItem ballTooltipItem;
	protected BossHealthBarView bossHealthBarView;
	protected WaveLevelView waveLevelView;
	// auto generate member end
	public OverlayMenu()
	{
		// auto generate constructor start
		characterInfoView = new(this);
		ballTooltipItem = new(this);
		bossHealthBarView = new(this);
		waveLevelView = new(this);
		// auto generate constructor end
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		characterInfoView.assignWindow(mRoot, "Content/Bot/CharacterInfoView");
		ballTooltipItem.assignWindow(mRoot, "BallTooltipItem");
		bossHealthBarView.assignWindow(mRoot, "BossHealthBarView");
		waveLevelView.assignWindow(mRoot, "WaveLevelView");
		// auto generate assignWindow end
	}
	public override void init()
	{
		initBinder();
		base.init();
		// auto generate init start
		// auto generate init end

		bossHealthBarView.setActive(false);
	}
	public override void onGameState()
	{
		base.onGameState();
	}
	
	
}

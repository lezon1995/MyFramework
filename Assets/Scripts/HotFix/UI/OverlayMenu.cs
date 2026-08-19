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
	protected ExpView ExpView;
	protected RelicsView RelicsView;
	protected PlayerHealthView PlayerHealthView;
	protected EnemyHealthView EnemyHealthView;
	protected EnemyIntentsView EnemyIntentsView;
	// auto generate member end
	
	public RelicsView relics => RelicsView;
	public EnemyIntentsView intents => EnemyIntentsView;

	public OverlayMenu()
	{
		// auto generate constructor start
		ExpView = new(this);
		RelicsView = new(this);
		PlayerHealthView = new(this);
		EnemyHealthView = new(this);
		EnemyIntentsView = new(this);
		// auto generate constructor end
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		ExpView.assignWindow(mRoot, "Content/Bot/ExpView");
		RelicsView.assignWindow(mRoot, "Content/Left/PlayerInfo/RelicsView");
		PlayerHealthView.assignWindow(mRoot, "Content/Left/PlayerInfo/PlayerHealthView");
		EnemyHealthView.assignWindow(mRoot, "Content/Right/EnemyInfo/V/EnemyHealthView");
		EnemyIntentsView.assignWindow(mRoot, "Content/Right/EnemyInfo/V/EnemyIntentsView");
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
		// RelicsView.refresh(player.relics);
	}
}

using Obfuz;

namespace MarbleHero;

// auto generate member start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class OverlayMenu : LayoutScript
{
	protected ExpView mExpView;
	protected RelicsView mRelicsView;
	protected PlayerHealthView mPlayerHealthView;
	protected EnemyHealthView mEnemyHealthView;
	protected EnemyIntentsView mEnemyIntentsView;
	// auto generate member end
	
	public EnemyIntentsView intents => mEnemyIntentsView;
	public OverlayMenu()
	{
		// auto generate constructor start
		mExpView = new(this);
		mRelicsView = new(this);
		mPlayerHealthView = new(this);
		mEnemyHealthView = new(this);
		mEnemyIntentsView = new(this);
		// auto generate constructor end
		mNeedUpdate = false;
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		mExpView.assignWindow(mRoot, "Content/Bot/ExpView");
		mRelicsView.assignWindow(mRoot, "Content/Left/PlayerInfo/RelicsView");
		mPlayerHealthView.assignWindow(mRoot, "Content/Left/PlayerInfo/PlayerHealthView");
		mEnemyHealthView.assignWindow(mRoot, "Content/Right/EnemyInfo/V/EnemyHealthView");
		mEnemyIntentsView.assignWindow(mRoot, "Content/Right/EnemyInfo/V/EnemyIntentsView");
		// auto generate assignWindow end

		// intents = new(mIntents);
		// expBar = new(mExpBar);
		// relics = new(mRelics);
		// playerInfo = new(mPlayerInfo);
		// enemyInfo = new(mEnemyInfo);
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

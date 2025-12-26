
// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 因为使用很频繁所以简写为GBH,全称为GameBaseHotFix

using MarbleHero;

public partial class GBH
{
	// FrameSystem
	public static NetManager mNetManager;
	public static DemoSystem mDemoSystem;
	public static BattleSystem mBattleSystem;
	public static GameplayManager gameplayManager;
	public static BallManager ballManager;
	public static BrickManager brickManager;
	public static MarbleHero.LevelManager levelManager;
	public static FTextManager textManager;
	public static PlayerManager playerManager;
	public static ComboManager comboManager;
	// 需要添加auto generate LayoutScript start和auto generate LayoutScript end才会自动生成代码
	// auto generate LayoutScript start
	public static GameplayPanel mGameplayPanel;
	public static UIGaming mUIGaming;
	public static UILogin mUILogin;
	// auto generate LayoutScript end
}
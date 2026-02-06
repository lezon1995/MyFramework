// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 因为使用很频繁所以简写为GBH,全称为GameBaseHotFix

using MarbleHero;

public partial class GBH
{
    public static GameDesign gameDesign { get; set; }

    // FrameSystem
    public static NetManager mNetManager;
    public static DemoSystem mDemoSystem;
    public static BattleSystem mBattleSystem;
    public static GameplayManager gameplayManager;
    public static BallManager ballManager;
    public static BrickManager brickManager;
    public static MarbleHero.LevelManager levelManager;
    public static FTextManager textManager;

    public static ComboManager comboManager;

    // 需要添加auto generate LayoutScript start和auto generate LayoutScript end才会自动生成代码
    // auto generate LayoutScript start
    public static DebugPanel mDebugPanel;
    public static MainMenuScreen mMainMenuScreen;
    public static OverlayMenu mOverlayMenu;
    public static SplashScreen mSplashScreen;
    public static UIGame mUIGame;
    public static UILogin mUILogin;
    // auto generate LayoutScript end


    public static ADungeon _dungeon { get; set; }
    public static GameActionManager actionManager { get; set; }
    public static GameEffectManager effectManager { get; set; }
    public static APlayer player { get; set; }
    public static SoundMaster sound { get; set; }
    public static MusicMaster music { get; set; }

    public static ARoom room
    {
        get => ADungeon.currMapNode?.room;
        set => ADungeon.currMapNode.room = value;
    }

    public static MapRoomNode mapNode => ADungeon.getCurrMapNode();
    public static MonsterGroup monsters => room.monsters;
    public static AMonster enemy => room?.monsters?.main;
    public static MetricData metricData { get; set; }
}
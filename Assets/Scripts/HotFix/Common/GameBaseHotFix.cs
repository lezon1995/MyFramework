using MoreMountains;

// 这个类的添加是方便代码的书写
// 因为使用很频繁所以简写为GBH,全称为GameBaseHotFix
public class GBR
{
	// auto generate SQLite start
	// auto generate SQLite end
	// FrameSystem

    // 需要添加auto generate Excel start和auto generate Excel end才会自动生成代码
    // auto generate Excel start
    // auto generate Excel end
    
    public static GameDesign gameDesign { get; set; }

    // FrameSystem
    public static BallManager ballManager;
    public static BrickManager brickManager;
    public static LevelManager levelManager;
    public static FTextManager textManager;
    public static LocalizedStrings languagePack;

    public static ComboManager comboManager;

    // 需要添加auto generate LayoutScript start和auto generate LayoutScript end才会自动生成代码
    // auto generate LayoutScript start
	public static DebugPanel mDebugPanel;
	public static MainMenuScreen mMainMenuScreen;
	public static OverlayMenu mOverlayMenu;
	public static RewardChoosePanel mRewardChoosePanel;
	public static SplashScreen mSplashScreen;
	public static UIGame mUIGame;
    // auto generate LayoutScript end
    // auto generate LayoutScript end


    public static ADungeon _dungeon { get; set; }
    public static GameActionManager actionManager { get; set; }
    public static GameEffectManager effectManager { get; set; }
    public static APlayer player { get; set; }
    public static SoundMaster sound { get; set; }
    public static MusicMaster music { get; set; }
    public static FxMaster fx { get; set; }

    public static ARoom room
    {
        get => ADungeon.currMapNode?.room;
        set => ADungeon.currMapNode.room = value;
    }

    public static MapRoomNode mapNode => ADungeon.getCurrMapNode();
    public static MonsterGroup monsters => room.monsters;
    public static MetricData metricData { get; set; }
    public static ResourceManager resource => mResourceManager;
    public static PrefabPoolManager prefabPool => mPrefabPoolManager;
}
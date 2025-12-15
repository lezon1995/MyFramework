global using static FrameDefine;
global using static FrameBaseDefine;
global using static UnityUtility;
global using static FrameBaseHotFix;
global using static FrameBaseUtility;
global using static FrameUtility;
global using static MathUtility;
global using static GameDefine;
global using static HotfixDefine;
global using static GBH;
using System;
using System.Reflection;
using MarbleHero;

public class GameHotFix : GameHotFixBase
{
    public static GameHotFixBase createHotFixInstance()
    {
        mInstance = createInstance<GameHotFixBase>(MethodBase.GetCurrentMethod().DeclaringType);
        return mInstance;
    }

    //----------------------------------------------------------------------------------------------------------------------------------
    protected override void registerAll()
    {
        LayoutRegisterHotFix.registeAll();
        PacketRegister.registeAll();
    }

    protected override void initFrameSystem()
    {
        // registeFrameSystem<NetManager>(com => mNetManager = com);
        registeFrameSystem<DemoSystem>(com => mDemoSystem = com);
        registeFrameSystem<BattleSystem>(com => mBattleSystem = com);
        registeFrameSystem<BallManager>(com => ballManager = com);
        registeFrameSystem<BrickManager>(com => brickManager = com);
        registeFrameSystem<LevelManager>(com => levelManager = com);
        registeFrameSystem<PlayerManager>(com => playerManager = com);
        // registeFrameSystem<FTextManager>(com => textManager = com);
        registeFrameSystem<GameplayManager>(com => gameplayManager = com);
    }

    protected override string getAndroidPluginBundleName()
    {
        return ANDROID_PLUGIN_BUNDLE_NAME;
    }

    protected override Type getStartGameSceneType()
    {
        return typeof(MainScene);
    }
}
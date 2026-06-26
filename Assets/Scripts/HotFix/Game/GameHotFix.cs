global using static FrameDefine;
global using static FrameBaseDefine;
global using static UnityUtility;
global using static StringUtility;
global using static FrameBaseHotFix;
global using static FrameBaseUtility;
global using static FrameUtility;
global using static MathUtility;
global using static GameDefine;
global using static HotfixDefine;
global using static GBH;
using System;
using MarbleHero;

public class GameHotFix : GameHotFixBase<GameHotFix>
{
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
        registeFrameSystem<ComboManager>(com => comboManager = com);
        registeFrameSystem<FTextManager>(com => textManager = com);
        registeFrameSystem<GameplayManager>(com => gameplayManager = com);
        registeFrameSystem<LocalizedStrings>(com => languagePack = com);
    }
    
	protected override string getAndroidPluginBundleName() { return ANDROID_PLUGIN_BUNDLE_NAME; }
	protected override Type getStartGameSceneType() { return typeof(MainScene); }
}
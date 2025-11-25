global using static FrameDefine;
global using static FrameBaseDefine;
global using static UnityUtility;
global using static FrameBaseUtility;
global using static FrameUtility;
global using static GameDefine;
global using static GBH;

using System;
using System.Reflection;
using static GameDefine;
using static GBR;

public class GameHotFix : GameHotFixBase<GameHotFix>
{
	//----------------------------------------------------------------------------------------------------------------------------------
	protected override void registerAll()
	{
		LayoutRegisterHotFix.registeAll();
		PacketRegister.registeAll();
		ExcelRegister.registeAll();
    }
	protected override void initFrameSystem()
	{
		registeFrameSystem<NetManager>((com) =>		{ mNetManager = com; });
		registeFrameSystem<DemoSystem>((com) =>		{ mDemoSystem = com; });
		registeFrameSystem<BattleSystem>((com) =>	{ mBattleSystem = com; });
	}
	protected override string getAndroidPluginBundleName() { return ANDROID_PLUGIN_BUNDLE_NAME; }
	protected override Type getStartGameSceneType() { return typeof(MainScene); }
}
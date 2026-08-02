global using static FrameDefine;
global using static FrameBaseDefine;
global using static UnityUtility;
global using static StringUtility;
global using static FrameBaseHotFix;
global using static FrameBaseUtility;
global using static FrameUtility;
global using static MathUtility;
global using static MathExtension;
global using static GameDefine;
global using static HotfixDefine;
global using static GBR;

using System;
using MoreMountains;
using static FrameBaseUtility;
using static GBR;

public class GameHotFix : GameHotFixBase<GameHotFix>
{
	//----------------------------------------------------------------------------------------------------------------------------------
	protected override void registerAllTable()
	{
        // ExcelRegister.registeAll();
    }
	protected override void registerAll()
	{
		LayoutRegisterHotFix.registeAll();
		// PacketRegister.registeAll();
    }
	protected override void initFrameSystem()
	{
		registeFrameSystem<BrickManager>(com => brickManager = com);
		registeFrameSystem<LevelManager>(com => levelManager = com);
		registeFrameSystem<ComboManager>(com => comboManager = com);
		registeFrameSystem<FTextManager>(com => textManager = com);
		registeFrameSystem<LocalizedStrings>(com => languagePack = com);
	}
	protected override void onPostInit()
	{
		if (isDevOrEditor())
		{
			HotFixTest.runAll();
		}
	}
	protected override Type getStartGameSceneType() { return typeof(MainScene); }
}
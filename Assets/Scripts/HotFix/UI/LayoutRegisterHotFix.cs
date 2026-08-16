using static GBR;
using static LayoutManager;
using MoreMountains;

public class LayoutRegisterHotFix
{
	public static void registeAll()
	{
		// 需要添加auto generate start和auto generate end才会自动生成代码
		// auto generate start
		registeLayout<DebugPanel>(script =>						mDebugPanel = script);
		registeLayout<EscPanel>(script =>						mEscPanel = script);
		registeLayout<MainMenuScreen>(script =>					mMainMenuScreen = script);
		registeLayout<OperationPanel>(script =>					mOperationPanel = script);
		registeLayout<OverlayMenu>(script =>					mOverlayMenu = script);
		registeLayout<RewardChoosePanel>(script =>				mRewardChoosePanel = script);
		registeLayout<SelectPlayerPanel>(script =>				mSelectPlayerPanel = script);
		registeLayout<SplashScreen>(script =>					mSplashScreen = script);
		registeLayout<TooltipScreen>(script =>					mTooltipScreen = script);
		registeLayout<UIGame>(script =>							mUIGame = script);
		// auto generate end
	}
}
using static GBH;
using static LayoutManager;
using MarbleHero;

public class LayoutRegisterHotFix
{
	public static void registeAll()
	{
		// 需要添加auto generate start和auto generate end才会自动生成代码
		// auto generate start
		registeLayout<DebugPanel>((script) =>						{ mDebugPanel = script; });
		registeLayout<MainMenuScreen>((script) =>					{ mMainMenuScreen = script; });
		registeLayout<OverlayMenu>((script) =>						{ mOverlayMenu = script; });
		registeLayout<RewardChoosePanel>((script) =>				{ mRewardChoosePanel = script; });
		registeLayout<SplashScreen>((script) =>						{ mSplashScreen = script; });
		registeLayout<UIGame>((script) =>							{ mUIGame = script; });
		registeLayout<UILogin>((script) =>							{ mUILogin = script; });
		// auto generate end
	}
}
using Obfuz;
using UnityEngine;

namespace MoreMountains;

// auto generate member start
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class SplashScreen : LayoutScript
{
	protected myUGUIImageSimple mBg;
	protected myUGUIImageSimple mLogo;
	protected myUGUIText mDebugText;
	// auto generate member end
	public SplashScreen()
	{
		// auto generate constructor start
		// auto generate constructor end
		mNeedUpdate = false;
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out myUGUIObject content, "Content", false);
		newObject(out mBg, content, "Bg");
		newObject(out mLogo, content, "Logo");
		newObject(out mDebugText, "DebugText");
		// auto generate assignWindow end
	}
	public override void init()
	{
		base.init();
	}
	public override void onGameState()
	{
		base.onGameState();
	}
	
	public void setBgColor(Color c)
	{
		mBg.setColor(c);
	}

	public void setLogoColor(Color c)
	{
		mLogo.setColor(c);
	}

	public void setDebugText(string s)
	{
		mDebugText.setText(s);
	}
}

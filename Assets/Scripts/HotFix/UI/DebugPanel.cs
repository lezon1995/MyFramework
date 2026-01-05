using Obfuz;

// auto generate member start
[ObfuzIgnore(ObfuzScope.TypeName)]
public class DebugPanel : LayoutScript
{
	protected myUGUIText mDebugText;
	// auto generate member end
	public DebugPanel()
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
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

	public void setDebugText(string text)
	{
		mDebugText.setText(text);
	}
}

using MarbleHero;
using Obfuz;
using UnityEngine;

// auto generate member start
[ObfuzIgnore(ObfuzScope.TypeName)]
public class MainMenuPanel : LayoutScript
{
	protected myUGUIObject mMenuButtons;
	protected myUGUIText mDebugText;
	// auto generate member end

	protected Transform buttonsParent;
	
	public MainMenuPanel()
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out myUGUIObject content, "Content", false);
		newObject(out mMenuButtons, content, "MenuButtons");
		newObject(out mDebugText, "DebugText");
		// auto generate assignWindow end

		buttonsParent = mMenuButtons.getTransform().Find("V");
	}
	public override void init()
	{
		base.init();
	}
	public override void onGameState()
	{
		base.onGameState();
	}

	public void addButton(MainMenuButton button)
	{
		button.setParent(buttonsParent);
	}
}

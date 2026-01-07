using System.Collections.Generic;
using MarbleHero;
using Obfuz;
using UnityEngine;

// auto generate member start
[ObfuzIgnore(ObfuzScope.TypeName)]
public class MainMenuPanel : LayoutScript
{
	protected myUGUIObject mMenuButtons;
	protected myUGUIImageSimple mOverlay;
	protected myUGUIText mDebugText;
	// auto generate member end

	protected Transform buttonsParent;
	protected Dictionary<MainMenuType, MainMenuButton> buttons = new();
	
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
		newObject(out mOverlay, "Overlay");
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
		buttons[button.type] = button;
	}

	public void setShowPlayButton(bool show)
	{
		buttons[MainMenuType.PLAY].setActive(show);
		buttons[MainMenuType.ABANDON_RUN].setActive(!show);
		buttons[MainMenuType.RESUME_GAME].setActive(!show);
	}

	public void setShowStatAndInfoButton(bool show)
	{
		buttons[MainMenuType.STAT].setActive(show);
		buttons[MainMenuType.INFO].setActive(show);
	}

	public void setShowQuitAndPatchButton(bool show)
	{
		buttons[MainMenuType.QUIT].setActive(show);
		buttons[MainMenuType.PATCH_NOTES].setActive(show);
	}

	public void setOverlapColor(Color color)
	{
		mOverlay.setColor(color);
	}

	public void setOverlapRaycastTarget(bool b)
	{
		mOverlay.setUGUIRaycastTarget(b);
	}
}

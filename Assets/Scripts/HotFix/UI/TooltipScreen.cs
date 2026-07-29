using Obfuz;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/TooltipScreen.prefab
// 
[ObfuzIgnore(ObfuzScope.TypeName)]
public partial class TooltipScreen : LayoutScript
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject tooltipManager;
	// auto generate member end
	public TooltipScreen()
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out tooltipManager, "TooltipManager");
		// auto generate assignWindow end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
	}
	public override void onGameState()
	{
		base.onGameState();
	}
}

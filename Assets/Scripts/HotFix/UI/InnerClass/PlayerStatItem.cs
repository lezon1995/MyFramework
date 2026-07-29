namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class PlayerStatItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject statIcon;
	protected myUGUIObject statName;
	protected myUGUIObject statValue;
	// auto generate member end
	public PlayerStatItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out statIcon, "H/Icon/StatIcon");
		newObject(out statName, "H/Name/StatName");
		newObject(out statValue, "H/Value/StatValue");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
	}
	public override void onShow()
	{
		base.onShow();
	}
}

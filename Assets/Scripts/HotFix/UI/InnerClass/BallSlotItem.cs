using static StringUtility;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public class BallSlotItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIObject selected;
	protected myUGUIObject icon;
	protected myUGUIObject[] stars = new myUGUIObject[3];
	// auto generate member end
	public BallSlotItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out selected, "Selected");
		newObject(out icon, "Icon");
		for (int i = 0; i < stars.Length; ++i)
		{
			newObject(out stars[i], "Icon/Grade_Star_01/Star" + IToS(i));
		}
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

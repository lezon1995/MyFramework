namespace MoreMountains;
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OperationPanel.prefab
// 
public partial class BallInventoryItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIButton btn;
	protected myUGUIObject normal;
	protected myUGUIObject disable;
	protected myUGUIObject focus;
	protected myUGUIObject icon;
	protected myUGUIObject[] stars = new myUGUIObject[3];
	// auto generate member end
	public BallInventoryItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out btn, "Btn");
		newObject(out normal, "Btn/Normal");
		newObject(out disable, "Btn/Disable");
		newObject(out focus, "Btn/Focus");
		newObject(out icon, "Btn/Icon");
		for (int i = 0; i < stars.Length; ++i)
		{
			newObject(out stars[i], "Btn/Icon/Grade_Star_01/Star" + IToS(i));
		}
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
		
		
		if (btn.tryGetUnityComponent(out UIEventListener listener))
		{
			listener.SetOnPotentialDragInitialized(onPotentialDragInitialized);
			listener.SetOnDragStarted(onDragStarted);
			listener.SetOnDragging(onDragging);
			listener.SetOnDragEnded(onDragEnded);
			listener.SetOnDragReleasedOverUI(onDragReleasedOverUI);
		}
	}
	public override void onShow()
	{
		base.onShow();
	}
}

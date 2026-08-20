using MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public partial class ExpView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple expBar;
	protected myUGUITextTMP curExp;
	protected myUGUITextTMP maxExp;
	// auto generate member end
	
	float curProgress, targetProgress;
	float timeElapsed, tweenDuration;
	
	public ExpView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
		targetProgress = -1F;
		tweenDuration = 0.2F;
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out expBar, "Fill");
		newObject(out curExp, "Exp/TextCurExp");
		newObject(out maxExp, "Exp/TextMaxExp");
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

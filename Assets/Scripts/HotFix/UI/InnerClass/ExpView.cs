using MarbleHero;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public class ExpView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple expBar;
	protected myUGUIText curExp;
	protected myUGUIText maxExp;
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
		newObject(out expBar, "ImgExpBar");
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

	public void refresh(float dt, Exp exp)
	{
		if (targetProgress != exp.progress)
		{
			if (exp.progress < targetProgress)
				curProgress = 0;
			else
				curProgress = targetProgress;

			targetProgress = exp.progress;
			timeElapsed = 0;
			curExp.setText(IToS(exp.currentExp));
			maxExp.setText(IToS(exp.currentLevelRequiredExp));
		}

		timeElapsed = clamp(timeElapsed + dt, 0, tweenDuration);
		var t = timeElapsed / tweenDuration;
		var f = lerp(curProgress, targetProgress, t);
		expBar.setFillPercent(f);
	}
}

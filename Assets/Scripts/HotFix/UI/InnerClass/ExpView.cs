
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 

using MarbleHero;

public class ExpView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple mImgExpBar;
	protected myUGUIText mTextCurExp;
	protected myUGUIText mTextMaxExp;
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
		newObject(out mImgExpBar, "ImgExpBar");
		newObject(out myUGUIObject exp, "Exp", false);
		newObject(out mTextCurExp, exp, "TextCurExp");
		newObject(out mTextMaxExp, exp, "TextMaxExp");
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
			mTextCurExp.setText(IToS(exp.currentExp));
			mTextMaxExp.setText(IToS(exp.currentLevelRequiredExp));
		}

		timeElapsed = clamp(timeElapsed + dt, 0, tweenDuration);
		var t = timeElapsed / tweenDuration;
		var f = lerp(curProgress, targetProgress, t);
		mImgExpBar.setFillPercent(f);
	}
}

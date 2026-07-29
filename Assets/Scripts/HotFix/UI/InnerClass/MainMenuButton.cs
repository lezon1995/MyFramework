using System;
using PrimeTween;
namespace MoreMountains;
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/MainMenuScreen.prefab
// 
public class MainMenuButton : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIButton btn;
	protected myUGUIText text;
	// auto generate member end
	Action onClick;
	
	public MainMenuButton(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out btn, "Btn");
		newObject(out text, "Btn/Text");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		btn.registeCollider(onbtnClick);
		// auto generate init end
		
		btn.setUGUIButtonClick(() => onClick?.Invoke());
		// button.setOnTouchEnter((pos, v) =>
		// {
		//     log($" onTouchEnter pos={pos} v={v}");
		// });
		btn.setUGUIMouseEnter((pointer, go) =>
		{
			Tween.UIAnchoredPositionX(text.getRectTransform(), endValue: 30, duration: 0.1F, ease: Ease.OutCubic);
		});
		// button.setOnTouchLeave((pos, v) =>
		// {
		//     log($" onTouchExit pos={pos} v={v}");
		// });
		btn.setUGUIMouseExit((pointer, go) =>
		{
			Tween.UIAnchoredPositionX(text.getRectTransform(), endValue: 0, duration: 0.1F, ease: Ease.OutCubic);
		});
	}
	public override void onShow()
	{
		base.onShow();
	}
	//--------------------------------------------------------------------------------------------------------------------------------------------
	protected void onbtnClick()
	{
		;
	}

	public void setName(string name)
	{
		text.setText(name);
	}

	public void setOnClick(Action callback) => onClick = callback;
}

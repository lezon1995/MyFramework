using MarbleHero;
using PrimeTween;
using UnityEngine;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 


public class EnemyIntentItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIButton button;
	protected myUGUIText desc;
	protected myUGUIObject rect;
	protected myUGUIImageSimple icon;
	protected myUGUIText level;
	// auto generate member end
	
	public Intent type;
	public string name;
	public string content;
	float defaultDescY, targetDescY;
	
	public EnemyIntentItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out button, "Button");
		newObject(out desc, "Button/Desc");
		newObject(out rect, "Button/Rect");
		newObject(out icon, "Button/Rect/Icon");
		newObject(out level, "Button/Rect/Level");
		// auto generate assignWindowInternal end
		
		defaultDescY = desc.getAnchoredPosition().y;
		targetDescY = defaultDescY + 50;
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		button.registeCollider(onbuttonClick);
		// auto generate init end
		
		// button.setUGUIButtonClick(onClick);
		button.setUGUIMouseEnter((pointer, go) => { Tween.Scale(mRoot.getRectTransform(), endValue: 1.2F, duration: 0.1F, ease: Ease.OutCubic); });
		button.setUGUIMouseExit((pointer, go) => { Tween.Scale(mRoot.getRectTransform(), endValue: 1F, duration: 0.1F, ease: Ease.OutCubic); });

	}
	public override void onShow()
	{
		base.onShow();
		setActive(true);
		setScale(1);
		setAlpha(1);
		setDescAlpha(0);
	}

	public override void onHide()
	{
		setActive(false);
		setScale(1);
		setAlpha(1);
		setDescAlpha(0);
		base.onHide();
	}

	//--------------------------------------------------------------------------------------------------------------------------------------------
	protected void onbuttonClick()
	{
		;
	}
	
	public void setScale(float scale)
	{
		rect.setScale(scale);
	}

	public void setAlpha(float alpha)
	{
		rect.setAlpha(alpha);
	}
    
	public void setDescAlpha(float alpha)
	{
		desc.setAlpha(alpha);
	}

	public void setDescAnchoredPosition(float t)
	{
		var y = lerp(defaultDescY, targetDescY, t);
		desc.setAnchoredPosition(new(0, y));
	}

	public void setIntentType(Intent intent)
	{
		type = intent;
		name = intent.ToString();
		content = name;
		desc.setText(content);
	}
}

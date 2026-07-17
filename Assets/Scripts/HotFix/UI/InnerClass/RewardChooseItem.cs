using System;
using MoreMountains;
using PrimeTween;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/RewardChoosePanel.prefab
// 
public class RewardChooseItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIButton btn;
	protected myUGUIText title;
	protected myUGUIText desc;
	// auto generate member end
	
	string relicId;
	Action onChoose;

	public override void recycle()
	{
		base.recycle();
		relicId = null;
		onChoose = null;
	}

	public RewardChooseItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out btn, "Btn");
		newObject(out title, "Btn/Title");
		newObject(out desc, "Btn/Desc");
		// auto generate assignWindowInternal end
	}
	public override void init()
	{
		base.init();
		// auto generate init start
		// auto generate init end
		
		btn.setUGUIButtonClick(onClick);
		btn.setUGUIMouseEnter((pointer, go) => Tween.Scale(go.transform, endValue: 1.2F, duration: 0.1F, ease: Ease.OutCubic));
		btn.setUGUIMouseExit((pointer, go) => Tween.Scale(go.transform, endValue: 1F, duration: 0.1F, ease: Ease.OutCubic));

	}
	public override void onShow()
	{
		base.onShow();
	}
	
	public void refresh(string id, Action chooseAction)
	{
		relicId = id;
		onChoose = chooseAction;
		title.setText(RelicLibrary.getRelic(relicId).relicId);
		desc.setText(RelicLibrary.getRelic(relicId).relicId);
		btn.setScale(1);
	}

	void onClick()
	{
		if (btn.getScript().isHide())
			return;

		RelicLibrary.getRelic(relicId).makeCopy().instantObtain(player, player.relics.Count, true);
		onChoose?.Invoke();
	}
}
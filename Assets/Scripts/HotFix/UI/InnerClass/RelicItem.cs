
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 

using MoreMountains;

public class RelicItem : WindowRecyclableUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple mIcon;
	// auto generate member end
	public RelicItem(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out mIcon, "Icon");
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

	public override void recycle()
	{
		base.recycle();
		mIcon.setSpriteOnly(null);
	}

	public void refresh(ARelic relic)
	{
		mIcon.setSpriteOnly(relic.getSprite());	
	}
}

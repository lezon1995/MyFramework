
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public class RelicsView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected WindowStructPool<RelicItem> mRelicItemPool;
	// auto generate member end
	public RelicsView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		mRelicItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out myUGUIObject g, "G", false);
		mRelicItemPool.assignTemplate(g, "RelicItem");
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

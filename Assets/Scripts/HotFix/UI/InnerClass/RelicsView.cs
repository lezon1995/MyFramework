
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 

using System.Collections.Generic;
using MarbleHero;

public class RelicsView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected WindowStructPool<RelicItem> RelicItemPool;
	// auto generate member end
	public RelicsView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		RelicItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		RelicItemPool.assignTemplate(mRoot, "G/RelicItem");
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

	public void refresh(List<ARelic> relics)
	{
		RelicItemPool.unuseAll();
		foreach (var relic in relics)
		{
			var item = RelicItemPool.newItem();
			item.refresh(relic);
		}
	}
}


using System.Collections.Generic;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/SelectPlayerPanel.prefab
// 
public partial class SelectedCharacterDetailView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected myUGUIImageSimple characterIcon;
	protected myUGUITextTMP characterName;
	protected myUGUITextTMP characterStats;
	protected myUGUITextTMP characterDesc;
	protected myUGUIObject ballParent;
	protected myUGUIObject relicParent;
	protected WindowStructPool<SelectBallItem> SelectBallItemPool;
	protected WindowStructPool<SelectRelicItem> SelectRelicItemPool;
	// auto generate member end
	public SelectedCharacterDetailView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		SelectBallItemPool = new(this);
		SelectRelicItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		newObject(out characterIcon, "V/Info/H/Avatar/Icon/Image");
		newObject(out characterName, "V/Info/H/Title/V/Name/Text");
		newObject(out characterStats, "V/Stats/Text");
		newObject(out characterDesc, "V/Desc/Text");
		newObject(out ballParent, "V/Balls/BallParent");
		newObject(out relicParent, "V/Relics/RelicParent");
		SelectBallItemPool.assignTemplate(mRoot, "V/Balls/BallParent/SelectBallItem");
		SelectRelicItemPool.assignTemplate(mRoot, "V/Relics/RelicParent/SelectRelicItem");
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
	
	public void RefreshCharacterDetail(PlayerDef def)
	{
		characterIcon.setSpriteOnly(def.Icon);
		characterName.setText(def.DisplayName);
		characterStats.setText(def.DisplayStats);
		characterDesc.setText(def.DisplayDesc);
	}

	public void RefreshCharacterSelectBalls(List<BallItem> ballDefs)
	{
		SelectBallItemPool.newItemList(ballDefs, (item, def) =>
		{
			item.refresh(def);
		});
	}

	public void RefreshCharacterSelectRelics(List<RelicDef> relicDefs)
	{
		SelectRelicItemPool.newItemList(relicDefs, (item, def) =>
		{
			item.refresh(def);
		});
	}
}

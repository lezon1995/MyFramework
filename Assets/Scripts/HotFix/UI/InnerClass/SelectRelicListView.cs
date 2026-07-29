
using System.Collections.Generic;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/SelectPlayerPanel.prefab
// 
public partial class SelectRelicListView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected WindowStructPool<SelectRelicItem> SelectRelicItemPool;
	// auto generate member end
	public SelectRelicListView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		SelectRelicItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		SelectRelicItemPool.assignTemplate(mRoot, "G/SelectRelicItem");
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
	
	
	SelectedCharacterDetailView detailView;
	Dictionary<RelicDef, SelectRelicItem> relicItems = new();
	public List<RelicDef> selectedRelics = new();

	public void initRelicItems()
	{
		foreach (var def in relicManager.getDefs())
		{
			var item = SelectRelicItemPool.newItem();
			bool isUnlocked = IsRelicUnlocked(def);
			isUnlocked = true;
			item.refresh(def, isUnlocked, OnRelicItemClicked, OnRelicItemHovered, OnRelicItemHoveredEnd);
			relicItems[def] = item;
		}
	}

	public void setCharacterDetailView(SelectedCharacterDetailView v)
	{
		detailView = v;
	}

	bool IsRelicUnlocked(RelicDef def)
	{
		return true;
	}
	
	void OnRelicItemClicked(SelectRelicItem item)
	{
		if (!item.IsUnlocked)
			return;

		selectedRelics.Clear();
		selectedRelics.Add(item.Def);
		_charSelectInfo.relics.Clear();
		_charSelectInfo.relics.AddRange(selectedRelics);

		RefreshRelicItems();
		detailView.RefreshCharacterSelectRelics(selectedRelics);
		// selectPlayerPanel.updateNextStepButton();
	}

	void OnRelicItemHovered(SelectRelicItem item)
	{
		if (!item.IsUnlocked)
			return;

		// detailView.RefreshCharacterDetail(item.Def);
	}

	void OnRelicItemHoveredEnd(SelectRelicItem item)
	{
		// detailView.RefreshCharacterDetail(selectedPlayer);
	}

	public void RefreshRelicItems()
	{
		foreach (var (def, item) in relicItems)
		{
			bool isSelected = selectedRelics.Contains(def);
			item.SetSelected(isSelected);
		}
	}
}

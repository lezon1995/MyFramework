namespace MoreMountains;
// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/SelectPlayerPanel.prefab
// 

using System;
using System.Collections.Generic;
using MoreMountains;

public class CharacterListView : WindowObjectUGUI
// auto generate classname end
{
	// auto generate member start
	protected WindowStructPool<SelectPlayerItem> SelectPlayerItemPool;
	// auto generate member end
	public CharacterListView(IWindowObjectOwner parent) : base(parent)
	{
		// auto generate constructor start
		SelectPlayerItemPool = new(this);
		// auto generate constructor end
	}
	protected override void assignWindowInternal()
	{
		// auto generate assignWindowInternal start
		SelectPlayerItemPool.assignTemplate(mRoot, "G/SelectPlayerItem");
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
	
	CharacterDetailView detailView;
	SelectPlayerPanel selectPlayerPanel;
    Dictionary<PlayerDef, SelectPlayerItem> playerItems = new();
    public PlayerDef selectedPlayer;
	
	public void initPlayerItems()
	{
		foreach (var def in characterManager.getDefs())
		{
			var item = SelectPlayerItemPool.newItem();
			bool isUnlocked = IsPlayerUnlocked(def);
			isUnlocked = true;
			item.refresh(def, isUnlocked, OnPlayerItemClicked, OnPlayerItemHovered, OnPlayerItemHoveredEnd);
			playerItems[def] = item;
		}
	}

	public void setCharacterDetailView(CharacterDetailView v)
	{
		detailView = v;
	}
	
	bool IsPlayerUnlocked(PlayerDef def)
	{
		return def.Type == APlayer.PlayerClass.IRONCLAD;
	}

	public bool isCharacterSelected()
	{
		return selectedPlayer != null;
	}

	void OnPlayerItemClicked(SelectPlayerItem item)
	{
		if (!item.IsUnlocked)
			return;

		selectedPlayer = item.Def;
		RefreshPlayerItems();
		detailView.RefreshCharacterDetail(item.Def);
		selectPlayerPanel.updateNextStepButton();
	}

	void OnPlayerItemHovered(SelectPlayerItem item)
	{
		if (!item.IsUnlocked)
			return;

		detailView.RefreshCharacterDetail(item.Def);
	}

	void OnPlayerItemHoveredEnd(SelectPlayerItem item)
	{
		detailView.RefreshCharacterDetail(selectedPlayer);
	}

	public void RefreshPlayerItems()
	{
		foreach (var (def, item) in playerItems)
		{
			bool isSelected = selectedPlayer == def;
			item.SetSelected(isSelected);
		}
	}

	public void setSelectPlayerPanel(SelectPlayerPanel p)
	{
		selectPlayerPanel = p;
	}
}

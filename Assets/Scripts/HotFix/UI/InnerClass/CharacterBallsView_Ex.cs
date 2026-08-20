using System;
using System.Collections.Generic;

namespace MoreMountains;

public partial class CharacterBallsView : IBallsContainerView
{
    public BallTooltipItem BallTooltipItem { get; set; }

    public void SetTitle(string title)
    {
    }

    public void BuildBallsWithIndex(List<BallInventorySlot> slotList, Action<int, BallInventoryItem, BallInventorySlot> onBuild)
    {
        BallInventoryItemPool.unuseAll();
        BallInventoryItemPool.newItem(slotList.Count);
        var used = BallInventoryItemPool.getUsedList();
        for (int i = 0; i < slotList.Count; i++)
            onBuild?.Invoke(i, used[i], slotList[i]);
    }

    public bool GetUsedItem(int index, out BallInventoryItem item)
    {
        var list = BallInventoryItemPool.getUsedList();
        if (index >= 0 && index < list.Count)
        {
            item = list[index];
            return item != null;
        }

        item = null;
        return false;
    }

    public void SetActive(bool active) => setActive(active);
    
    BallInventoryBinder binder;

    public BallInventoryBinder initBinder(OverlayMenu panel)
    {
        BallTooltipItem = panel.BallTooltipItem;
        return binder ??= new(this);
    }
}
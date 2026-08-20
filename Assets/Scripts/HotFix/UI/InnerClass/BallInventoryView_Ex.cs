using System;
using System.Collections.Generic;

namespace MoreMountains;

public partial class BallInventoryView : IBallsContainerView
{
    public BallTooltipItem BallTooltipItem { get; set; }
    public void SetTitle(string s) => textTitle.setText(s ?? string.Empty);

    public void BuildBallsWithIndex(List<BallInventorySlot> slotList, Action<int, BallInventoryItem, BallInventorySlot> onBuild)
    {
        BallInventoryItemPool.unuseAll();
        BallInventoryItemPool.newItem(slotList.Count);
        var used = BallInventoryItemPool.getUsedList();
        for (int i = 0; i < slotList.Count; i++)
            onBuild?.Invoke(i, used[i], slotList[i]);
    }

    public myUGUIObject ItemParent => itemParent;

    public void BuildBalls<TSlot>(List<TSlot> slotList, Action<BallInventoryItem, TSlot> onBuild)
    {
        BallInventoryItemPool.newItemList(slotList, onBuild);
    }

    /// <summary>同 BuildBalls,但回调里多了 index(0..slotList.Count-1)。
    /// 内部走 Pool.newItem(count) + 手动 for,不再创建额外 lambda,避免每次 Rebuild 分配。
    /// 直接传 slotList,binder 不需要在中间建一个 List&lt;BallItem&gt;,空格子用 slot.Item==null 表示。</summary>
    public void BuildBallsWithIndex<TSlot>(List<TSlot> slotList, Action<int, BallInventoryItem, TSlot> onBuild)
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

    public BallInventoryBinder initBinder(OperationPanel panel)
    {
        BallTooltipItem = panel.BallTooltipItem;
        return binder ??= new(this);
    }

    public BallInventoryBinder initBinder(EscPanel panel)
    {
        BallTooltipItem = panel.BallTooltipItem;
        return binder ??= new(this);
    }
}
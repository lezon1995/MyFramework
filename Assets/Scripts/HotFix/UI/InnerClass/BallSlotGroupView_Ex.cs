using System;
using System.Collections.Generic;

namespace MoreMountains;

public partial class BallSlotGroupView
{
    public void SetTitle(string s) => textTitle.setText(s ?? string.Empty);
    public myUGUIObject SlotRoot => itemParent;

    public void BuildSlots<TSlot>(List<TSlot> slotList, Action<BallSlotItem, TSlot> onBuild)
    {
        BallSlotItemPool.newItemList(slotList, onBuild);
    }

    /// <summary>同 BuildSlots,回调里多了 index。
    /// 直接传 slotList,binder 不需要在中间建一个 List&lt;BallSlot&gt;。</summary>
    public void BuildSlotsWithIndex<TSlot>(List<TSlot> slotList, Action<int, BallSlotItem, TSlot> onBuild)
    {
        BallSlotItemPool.unuseAll();
        BallSlotItemPool.newItem(slotList.Count);
        var used = BallSlotItemPool.getUsedList();
        for (int i = 0; i < slotList.Count; i++)
            onBuild?.Invoke(i, used[i], slotList[i]);
    }

    /// <summary>取第 index 个已经分配(used)的 item。Binder 用于选中态同步。</summary>
    public bool GetUsedItem(int index, out BallSlotItem item)
    {
        var list = BallSlotItemPool.getUsedList();
        if (index >= 0 && index < list.Count)
        {
            item = list[index];
            return item != null;
        }

        item = null;
        return false;
    }

    BallSlotGroupBinder binder;

    public BallSlotGroupBinder initBinder()
    {
        return binder ??= new(this);
    }
}
using System;
using System.Collections.Generic;

namespace MoreMountains;

public partial class BallSlotGroupView
{
    public void SetTitle(string s) => textTitle.setText(s ?? string.Empty);
    public myUGUIObject SlotRoot => itemParent;

    public void BuildSlots<TData>(List<TData> dataList, Action<BallSlotItem, TData> onBuild)
    {
        BallSlotItemPool.newItemList(dataList, (item, data) => onBuild?.Invoke(item, data));
    }

    /// <summary>取第 index 个已经分配（used）的 item。Binder 用于选中态同步。</summary>
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
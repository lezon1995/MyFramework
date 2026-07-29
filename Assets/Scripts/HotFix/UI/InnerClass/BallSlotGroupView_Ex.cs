using System;
using System.Collections.Generic;

namespace MoreMountains;

public partial class BallSlotGroupView
{
    public void SetTitle(string s) => textTitle.setText(s ?? string.Empty);
    public myUGUIObject SlotRoot => itemParent;

    public void BuildSlots<TData>(IList<TData> dataList, Action<BallSlotItem, TData> onBuild)
    {
        var list = dataList is List<TData> l ? l : new List<TData>(dataList);
        BallSlotItemPool.newItemList(list, (item, data) => onBuild?.Invoke(item, data));
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
}
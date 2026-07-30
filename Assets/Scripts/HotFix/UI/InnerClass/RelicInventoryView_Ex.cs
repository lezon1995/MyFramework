using System.Collections.Generic;

namespace MoreMountains;

public partial class RelicInventoryView
{
    public void SetTitle(string s) => textTitle.setText(s ?? string.Empty);
    public myUGUIObject ItemParent => itemParent;

    public void BuildRelics<TSlot>(List<TSlot> slotList, System.Action<RelicInventoryItem, TSlot> onBuild)
    {
        RelicInventoryItemPool.newItemList(slotList, onBuild);
    }

    /// <summary>同 BuildRelics,回调里多了 index(0..slotList.Count-1)。
    /// 直接传 slotList,binder 不需要在中间建一个 List&lt;RelicItem&gt;,空格子用 slot.Item==null 表示。</summary>
    public void BuildRelicsWithIndex<TSlot>(List<TSlot> slotList, System.Action<int, RelicInventoryItem, TSlot> onBuild)
    {
        RelicInventoryItemPool.unuseAll();
        RelicInventoryItemPool.newItem(slotList.Count);
        var used = RelicInventoryItemPool.getUsedList();
        for (int i = 0; i < slotList.Count; i++)
            onBuild?.Invoke(i, used[i], slotList[i]);
    }

    public bool GetUsedItem(int index, out RelicInventoryItem item)
    {
        var list = RelicInventoryItemPool.getUsedList();
        if (index >= 0 && index < list.Count)
        {
            item = list[index];
            return item != null;
        }

        item = null;
        return false;
    }

    RelicInventoryBinder binder;

    public RelicInventoryBinder initBinder()
    {
        return binder ??= new(this);
    }
}
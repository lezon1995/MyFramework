using System.Collections.Generic;

namespace MoreMountains;

public partial class RelicInventoryView
{
    public void SetTitle(string s) => textTitle.setText(s ?? string.Empty);
    public myUGUIObject ItemParent => itemParent;

    public void BuildRelics<TData>(List<TData> dataList, System.Action<RelicInventoryItem, TData> onBuild)
    {
        RelicInventoryItemPool.newItemList(dataList, (item, data) => onBuild?.Invoke(item, data));
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
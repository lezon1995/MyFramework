using System.Collections.Generic;

namespace MoreMountains;

public partial class BallInventoryView
{
    public void SetTitle(string s) => textTitle.setText(s ?? string.Empty);
    public myUGUIObject ItemParent => itemParent;

    public void BuildBalls<TData>(IList<TData> dataList, System.Action<BallInventoryItem, TData> onBuild)
    {
        var list = dataList is List<TData> l ? l : new List<TData>(dataList);
        BallInventoryItemPool.newItemList(list, (item, data) => onBuild?.Invoke(item, data));
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
}
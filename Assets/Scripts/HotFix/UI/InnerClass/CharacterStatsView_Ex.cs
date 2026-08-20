using System;
using System.Collections.Generic;

namespace MoreMountains;

public partial class CharacterStatsView
{
    public void BuildPlayerStats<TData>(List<TData> dataList, Action<PlayerStatItem, TData> onBuild)
    {
        PlayerStatItemPool.newItemList(dataList, onBuild);
    }
}
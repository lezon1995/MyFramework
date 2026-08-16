using System;
using System.Collections.Generic;

namespace MoreMountains;

public partial class WaveMonsterView
{
    WaveMonsterBinder binder;

    public WaveMonsterBinder initBinder()
    {
        return binder ??= new(this);
    }

    public void BuildWaveMonsterItems<TData>(List<TData> dataList, Action<WaveMonsterItem, TData> onBuild)
    {
        WaveMonsterItemPool.newItemList(dataList, onBuild);
    }

    public void RefreshTitle(int waveNumber)
    {
        _stringEvent.setInt("waveNumber", waveNumber);
    }
}
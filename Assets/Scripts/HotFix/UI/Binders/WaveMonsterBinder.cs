using System;
using System.Collections.Generic;
using UniStats;
using UnityEngine;

namespace MoreMountains
{
    public sealed class WaveMonsterBinder
    {
        WaveMonsterView _view;
        APlayer _player;
        WaveConfig _waveConfig;

        WaveMonsterBinder()
        {
        }

        public WaveMonsterBinder(WaveMonsterView view) : this()
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Attach(WaveConfig waveConfig)
        {
            _waveConfig = waveConfig ?? throw new ArgumentNullException(nameof(waveConfig));
            Rebuild();
        }

        public void Detach()
        {
        }
        
        public void Rebuild()
        {
            _view.RefreshTitle(_waveConfig.waveNumber);
            _view.BuildWaveMonsterItems(_waveConfig.availableMonsters, (item, spawnConfig) =>
            {
                var icon = spawnConfig.monsterDef.UnitIcon;
                item.SetMonsterIcon(icon);
                item.SetAtLeastSpawnCount(spawnConfig.atLeastSpawnCount);
            });
        }
    }
}
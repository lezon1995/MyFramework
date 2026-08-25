using UnityEngine.Localization.Components;

namespace MoreMountains;

// auto generate classname start
// generate from:Assets/GameResources/UI/UIPrefab/OverlayMenu.prefab
// 
public partial class WaveLevelView : WindowObjectUGUI
// auto generate classname end
{
    // auto generate member start
    protected myUGUITextTMP textRemainSeconds;
    protected myUGUITextTMP textWaveNumber;
    protected myUGUITextTMP textActiveMonsterCount;
    protected myUGUITextTMP textKillMonsterCount;
    // auto generate member end

    LocalizeStringEvent _stringWaveNumber;

    public WaveLevelView(IWindowObjectOwner parent) : base(parent)
    {
        // auto generate constructor start
        // auto generate constructor end
    }

    protected override void assignWindowInternal()
    {
        // auto generate assignWindowInternal start
        newObject(out textRemainSeconds, "CurWave/Seconds/Remain");
        newObject(out textWaveNumber, "CurWave/Wave");
        newObject(out textActiveMonsterCount, "WaveMonster/ActiveMonsters/TextActiveCount");
        newObject(out textKillMonsterCount, "WaveMonster/KillCount/TextKillCount");
        // auto generate assignWindowInternal end

        textWaveNumber.tryGetUnityComponent(out _stringWaveNumber);
    }

    public override void init()
    {
        base.init();
        // auto generate init start
        // auto generate init end
    }

    public override void onShow()
    {
        base.onShow();
    }

    public override void update(float dt)
    {
        base.update(dt);

        var w = waveManager;
        if (w)
        {
            setRemainSeconds(w.WaveTimeRemaining.ceil());
            setWaveNumber(w.WaveNumber, w.CurLevel?.MaxWave ?? 0);
            setActiveMonsterCount(w.ActiveMonsterCount);
            setKillMonsterCount(w.WaveKillCount);
        }
    }
}
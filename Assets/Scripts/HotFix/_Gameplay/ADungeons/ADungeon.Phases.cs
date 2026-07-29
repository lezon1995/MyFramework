using System.Collections.Generic;

namespace MoreMountains;

public partial class ADungeon
{
    public DungeonPhaseType LastPhase { get; set; }
    public DungeonPhaseType CurPhase { get; set; }
    public DungeonPhaseType ToPhase { get; set; }
    public bool isEndPhase { get; set; }
    public ADungeonPhase curPhase;
    protected Dictionary<DungeonPhaseType, ADungeonPhase> _phases = new();

    protected abstract void initializePhases();

    protected virtual void changePhase(DungeonPhaseType type)
    {
        LastPhase = CurPhase;
        CurPhase = type;
        nextPhase(type);
    }

    protected void nextPhase(DungeonPhaseType type)
    {
        curPhase?.onEnd();
        var last = curPhase;
        curPhase = _phases[type];
        curPhase.onBegin(last);
    }

    public void endPhase()
    {
        isEndPhase = true;
    }

    protected void onPhaseUpdate(float dt)
    {
        if (isEndPhase)
        {
            curPhase?.onEnd();
            curPhase = null;
            _phases.Clear();
            LastPhase = DungeonPhaseType.NONE;
            CurPhase = DungeonPhaseType.NONE;
            ToPhase = DungeonPhaseType.NONE;
            screen = CurrentScreen.INITIALIZING_PLAYER;
            return;
        }

        if (CurPhase != ToPhase)
        {
            changePhase(ToPhase);
        }
        else
        {
            curPhase?.update(dt);
        }
    }

    protected void onPhaseFixedUpdate(float dt)
    {
        curPhase?.fixedUpdate(dt);
    }
}
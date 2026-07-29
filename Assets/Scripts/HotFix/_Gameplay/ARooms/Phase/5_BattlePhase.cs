using System;

namespace MoreMountains;

public class BattlePhase : ARoomPhase
{
    public BattlePhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(ARoomPhase last)
    {
        base.onBegin(last);
        
        wave.StartNextWave();
    }

    public override void onEnd()
    {
        base.onEnd();
    }

    public override void update(float dt)
    {
        base.update(dt);

        switch (wave.CurWaveState)
        {
            case WaveState.Completed:
                _room.ToPhase = RoomPhaseType.BATTLE_PASS_CLEANUP;
                break;
            case WaveState.Failed:
                _room.ToPhase = RoomPhaseType.GAME_SETTLEMENT;
                break;
        }
    }

    public override void fixedUpdate(float dt)
    {
        base.fixedUpdate(dt);
    }

    protected override void onBindListeners()
    {
    }

    protected override void onUnbindListeners()
    {
    }
}
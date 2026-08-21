using UniStats;

namespace MoreMountains;

public class BattlePassCleanupPhase : ARoomPhase
{
    Timer timer;

    public BattlePassCleanupPhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(ARoomPhase last)
    {
        base.onBegin(last);
        log("进入 关卡通关阶段 测试阶段 1秒后自动跳过");
        timer = 1F;

        var p = _room.Player;
        if (p.GetStat(Character.Stat.Greed, out var stat))
        {
            var value = stat.Value.round();
            if (value > 0)
            {
                p.gainExp(value);
                p.gainGold(value);
            }

            var newValue = (stat.Value * gameDesign.PlayerGreedIncreasementPerWave).round();
            if (newValue > value)
            {
                var delta = newValue - value;
                stat.BonusFlat.AddFlat(delta);
            }
        }
    }

    public override void onEnd()
    {
        base.onEnd();
    }

    public override void update(float dt)
    {
        base.update(dt);
        if (timer.update(dt))
        {
            _room.ToPhase = RoomPhaseType.LEVEL_UP_REWARD;
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
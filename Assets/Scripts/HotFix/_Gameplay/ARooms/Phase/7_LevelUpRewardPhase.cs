using UnityEngine;

namespace MoreMountains;

public class LevelUpRewardPhase : APhase
{
    public LevelUpRewardPhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(APhase last)
    {
        base.onBegin(last);
        log("进入 升级奖励阶段 按数字键盘1 进入商店阶段");
        // 问题1修复：进入奖励选择阶段，由外部调用ExitRewardSelection来开始下一波
        wave.waveManager.EnterRewardSelection();
    }

    public override void onEnd()
    {
        base.onEnd();
    }

    public override void update(float dt)
    {
        base.update(dt);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            wave.ConfirmRewardSelection();
            _room.ToPhase = RoomPhaseType.SHOPPING;
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
using UnityEngine;

namespace MoreMountains;

public class ShoppingPhase : APhase
{
    public ShoppingPhase(MonsterRoom room) : base(room)
    {
    }

    public override void onBegin(APhase last)
    {
        base.onBegin(last);
        log("进入 商店购买阶段 按数字键盘1 开启下一波");
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
            _room.ToPhase = RoomPhaseType.BATTLE;
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
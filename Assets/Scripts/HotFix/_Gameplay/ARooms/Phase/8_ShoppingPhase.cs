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

        // 通过全局 Service 打开 OperationPanel；面板内部已经绑定或会重绑当前玩家。
        OperationPanelService.Instance.Open();
    }

    public override void onEnd()
    {
        OperationPanelService.Instance.Close();
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
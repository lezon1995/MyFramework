namespace MarbleHero;

public class NeowEvent : AEvent
{
    public NeowEvent(bool isDone)
    {
    }

    public override void onEnterRoom()
    {
        base.onEnterRoom();
        buttonEffect(0);

        if (Settings.isDebug)
        {
            var node = ADungeon.getRoomNodeAt(1, 0);
            ADungeon.enterTargetRoom(node);
        }
    }

    public override void update(float dt)
    {
        base.update(dt);

        if (Settings.isDebug)
        {
            if (InputActionSet.confirm.isJustPressed())
            {
                var node = ADungeon.getRoomNodeAt(1, 0);
                ADungeon.enterTargetRoom(node);
            }
        }
    }

    protected override void buttonEffect(int paramInt)
    {
        switch (paramInt)
        {
            case 0:
                openMap();
                break;
        }
    }
}
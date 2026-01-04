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
    }

    protected override void buttonEffect(int paramInt)
    {
        openMap();
    }
}
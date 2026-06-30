namespace MarbleHero;

public class CounterTrigger : ATrigger
{
    int elapsed;
    int gap;

    public override void resetProperty()
    {
        base.resetProperty();
        elapsed = 0;
        gap = 0;
    }

    public void setGap(int value)
    {
        gap = value;
    }

    public void count(int delta)
    {
        elapsed += delta;
        if (elapsed>=gap)
        {
            
        }
    }
}
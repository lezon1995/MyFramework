namespace MarbleHero;

public class CounterTrigger : ATrigger
{
    int elapsed;
    int gap;

    public void setGap(int value)
    {
        gap = value;
    }

    public void count(int delta)
    {
        elapsed+=delta;
        if (elapsed>=gap)
        {
            
        }
    }
}
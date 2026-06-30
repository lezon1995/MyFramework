public class Counter : ClassObject
{
    public int elapsed;

    public override void resetProperty()
    {
        base.resetProperty();
        elapsed = 0;
    }

    public virtual bool count(int delta = 1)
    {
        return internalCount(delta);
    }

    protected virtual bool internalCount(int delta = 1)
    {
        elapsed += delta;
        return true;
    }

    public void reset()
    {
        elapsed = 0;
    }
}

public class RepeatCounter : Counter
{
    public int triggerInterval;
    public int triggeredTimes => triggerInterval == 0 ? 0 : elapsed / triggerInterval;
    public int remain => triggerInterval == 0 ? 0 : triggerInterval - (elapsed % triggerInterval);
    public float pct => triggerInterval == 0 ? 0F : (elapsed % triggerInterval) * 1F / triggerInterval;
    public bool unstarted => triggerInterval != 0 && elapsed == 0;

    public override void resetProperty()
    {
        base.resetProperty();
        triggerInterval = 0;
    }

    protected override bool internalCount(int delta = 1)
    {
        base.internalCount(delta);
        var triggered = triggerInterval != 0 && elapsed % triggerInterval == 0;
        if (triggered)
            return true;

        return false;
    }

    public void setInterval(int interval)
    {
        triggerInterval = interval;
    }
}
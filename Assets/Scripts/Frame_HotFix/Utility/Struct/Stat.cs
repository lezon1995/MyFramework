public struct Stat
{
    public float initial;
    public float extra;
    public float multiplier;

    public Stat()
    {
        initial = 0;
        extra = 0;
        multiplier = 1F;
    }

    public void increase(float delta)
    {
        extra += delta;
    }

    public void increasePct(float delta)
    {
        multiplier += delta;
    }

    public void reset()
    {
        multiplier = 1F;
    }

    public static implicit operator Stat(float v) => new()
    {
        initial = v,
        extra = 0,
        multiplier = 1F,
    };

    public static implicit operator float(Stat v)
    {
        return (v.initial + v.extra) * v.multiplier;
    }
}
using System;

[Serializable]
public struct Stat
{
    public float initial;
    public float extra;
    public float multiplier;
    public float extraMultiplier;

    public Stat()
    {
        initial = 0;
        extra = 0;
        multiplier = 1F;
        extraMultiplier = 0;
    }

    public void increase(float delta)
    {
        extra += delta;
    }

    public void increasePct(float delta)
    {
        extraMultiplier += delta;
    }

    public void reset()
    {
        extra = 0F;
        multiplier = 1F;
        extraMultiplier = 0F;
    }

    public static implicit operator Stat(float v) => new()
    {
        initial = v,
        extra = 0F,
        multiplier = 1F,
        extraMultiplier = 0F
    };

    public static implicit operator float(Stat v) => v.value;

    public float value =>  (initial +extra) * (multiplier + extraMultiplier);
}
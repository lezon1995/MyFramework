public struct Chance
{
    float chance;

    public bool check()
    {
        if (MathUtility.randomHit(chance))
            return true;

        return false;
    }

    public void kill()
    {
        chance = 0;
    }

    public void reset()
    {
    }

    public static implicit operator bool(Chance timer)
    {
        return timer.chance > 0;
    }

    public static implicit operator float(Chance timer) => timer.chance;

    public static implicit operator Chance(float c) => new()
    {
        chance = c,
    };
}
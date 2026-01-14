namespace MarbleHero;

public static class PowerExtensions
{
    public static void with<P1>(this APower power, P1 p1)
    {
        if (power is IArgs<P1> action)
        {
            action.onCreate(p1);
        }
    }

    public static void with<P1, P2>(this APower power, P1 p1, P2 p2)
    {
        if (power is IArgs<P1, P2> action)
        {
            action.onCreate(p1, p2);
        }
    }

    public static void with<P1, P2, P3>(this APower power, P1 p1, P2 p2, P3 p3)
    {
        if (power is IArgs<P1, P2, P3> action)
        {
            action.onCreate(p1, p2, p3);
        }
    }
}
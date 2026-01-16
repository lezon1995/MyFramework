namespace MarbleHero;

public static class GameActionExtensions
{
    public static void with<P1>(this AGameAction gameAction, P1 p1)
    {
        if (gameAction is IArgs<P1> args)
        {
            args.onCreate(p1);
        }
    }

    public static void with<P1, P2>(this AGameAction gameAction, P1 p1, P2 p2)
    {
        if (gameAction is IArgs<P1, P2> args)
        {
            args.onCreate(p1, p2);
        }
    }

    public static void with<P1, P2, P3>(this AGameAction gameAction, P1 p1, P2 p2, P3 p3)
    {
        if (gameAction is IArgs<P1, P2, P3> args)
        {
            args.onCreate(p1, p2, p3);
        }
    }
}
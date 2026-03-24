public interface IArgs
{
}

public interface IArgs<in P1> : IArgs
{
    void onCreate(P1 p1);
}

public interface IArgs<in P1, in P2> : IArgs
{
    void onCreate(P1 p1, P2 p2);
}

public interface IArgs<in P1, in P2, in P3> : IArgs
{
    void onCreate(P1 p1, P2 p2, P3 p3);
}

public interface IArgs<in P1, in P2, in P3, in P4> : IArgs
{
    void onCreate(P1 p1, P2 p2, P3 p3, P4 p4);
}

public interface IArgs<in P1, in P2, in P3, in P4, in P5> : IArgs
{
    void onCreate(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
}
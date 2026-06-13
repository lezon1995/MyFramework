public interface IRefresh
{
}

public interface IRefresh<in P1> : IRefresh
{
    void refresh(P1 p1);
}

public interface IRefresh<in P1, in P2> : IRefresh
{
    void refresh(P1 p1, P2 p2);
}

public interface IRefresh<in P1, in P2, in P3> : IRefresh
{
    void refresh(P1 p1, P2 p2, P3 p3);
}

public interface IRefresh<in P1, in P2, in P3, in P4> : IRefresh
{
    void refresh(P1 p1, P2 p2, P3 p3, P4 p4);
}

public interface IRefresh<in P1, in P2, in P3, in P4, in P5> : IRefresh
{
    void refresh(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
}
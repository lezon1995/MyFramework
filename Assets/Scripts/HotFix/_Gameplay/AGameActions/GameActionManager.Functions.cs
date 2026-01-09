using System;

namespace MarbleHero;

public interface IGameActionArgs
{
}

public interface IGameActionArgs<in P1> : IGameActionArgs
{
    void onCreate(P1 p1);
}

public interface IGameActionArgs<in P1, in P2> : IGameActionArgs
{
    void onCreate(P1 p1, P2 p2);
}

public interface IGameActionArgs<in P1, in P2, in P3> : IGameActionArgs
{
    void onCreate(P1 p1, P2 p2, P3 p3);
}

public interface IGameActionArgs<in P1, in P2, in P3, in P4> : IGameActionArgs
{
    void onCreate(P1 p1, P2 p2, P3 p3, P4 p4);
}

public interface IGameActionArgs<in P1, in P2, in P3, in P4, in P5> : IGameActionArgs
{
    void onCreate(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5);
}

public partial class GameActionManager
{
    #region AddTop

    public AGameAction addToTop<T>() where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (room.inCombat())
        {
            actions.Insert(0, gameAction);
            return gameAction;
        }

        UN_CLASS(gameAction);
        return null;
    }

    /*public void addToTop<T, P1>(P1 p1) where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (gameAction is IGameActionArgs<P1> action)
        {
            action.onCreate(p1);
            if (room.inCombat())
            {
                actions.Insert(0, gameAction);
                return;
            }
        }

        UN_CLASS(gameAction);
    }

    public void addToTop<T, P1, P2>(P1 p1, P2 p2) where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (gameAction is IGameActionArgs<P1, P2> action)
        {
            action.onCreate(p1, p2);
            if (room.inCombat())
            {
                actions.Insert(0, gameAction);
                return;
            }
        }

        UN_CLASS(gameAction);
    }

    public void addToTop<T, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (gameAction is IGameActionArgs<P1, P2, P3> action)
        {
            action.onCreate(p1, p2, p3);
            if (room.inCombat())
            {
                actions.Insert(0, gameAction);
                return;
            }
        }

        UN_CLASS(gameAction);
    }

    public void addToTop<T, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4) where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (gameAction is IGameActionArgs<P1, P2, P3, P4> action)
        {
            action.onCreate(p1, p2, p3, p4);
            if (room.inCombat())
            {
                actions.Insert(0, gameAction);
                return;
            }
        }

        UN_CLASS(gameAction);
    }

    public void addToTop<T, P1, P2, P3, P4, P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5) where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (gameAction is IGameActionArgs<P1, P2, P3, P4, P5> action)
        {
            action.onCreate(p1, p2, p3, p4, p5);
            if (room.inCombat())
            {
                actions.Insert(0, gameAction);
                return;
            }
        }

        UN_CLASS(gameAction);
    }*/

    #endregion

    #region AddBot

    public AGameAction addToBot<T>() where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (room.inCombat())
        {
            actions.Add(gameAction);
            return gameAction;
        }

        UN_CLASS(gameAction);
        return null;
    }

    /*
    public void addToBot<T, P1>(P1 p1) where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (gameAction is IGameActionArgs<P1> action)
        {
            action.onCreate(p1);
            if (room.inCombat())
            {
                actions.Add(gameAction);
                return;
            }
        }

        UN_CLASS(gameAction);
    }

    public void addToBot<T, P1, P2>(P1 p1, P2 p2) where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (gameAction is IGameActionArgs<P1, P2> action)
        {
            action.onCreate(p1, p2);
            if (room.inCombat())
            {
                actions.Add(gameAction);
                return;
            }
        }

        UN_CLASS(gameAction);
    }

    public void addToBot<T, P1, P2, P3>(P1 p1, P2 p2, P3 p3) where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (gameAction is IGameActionArgs<P1, P2, P3> action)
        {
            action.onCreate(p1, p2, p3);
            if (room.inCombat())
            {
                actions.Add(gameAction);
                return;
            }
        }

        UN_CLASS(gameAction);
    }

    public void addToBot<T, P1, P2, P3, P4>(P1 p1, P2 p2, P3 p3, P4 p4) where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (gameAction is IGameActionArgs<P1, P2, P3, P4> action)
        {
            action.onCreate(p1, p2, p3, p4);
            if (room.inCombat())
            {
                actions.Add(gameAction);
                return;
            }
        }

        UN_CLASS(gameAction);
    }

    public void addToBot<T, P1, P2, P3, P4, P5>(P1 p1, P2 p2, P3 p3, P4 p4, P5 p5) where T : AGameAction
    {
        var gameAction = CLASS<AGameAction>(typeof(T));
        if (gameAction is IGameActionArgs<P1, P2, P3, P4, P5> action)
        {
            action.onCreate(p1, p2, p3, p4, p5);
            if (room.inCombat())
            {
                actions.Add(gameAction);
                return;
            }
        }

        UN_CLASS(gameAction);
    }
    */

    #endregion
}
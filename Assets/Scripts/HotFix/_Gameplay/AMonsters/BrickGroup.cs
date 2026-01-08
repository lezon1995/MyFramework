using System;
using System.Collections.Generic;

namespace MarbleHero;

[Serializable]
public class EnemyMoveInfo : ClassObject
{
    public int nextMove;
    public Intent intent;
    public int baseDamage;
    public int multiplier;
    public bool isMultiDamage;

    public int intentDmg;

    public override void resetProperty()
    {
        base.resetProperty();
        nextMove = 0;
        intent = default;
        baseDamage = 0;
        multiplier = 0;
        isMultiDamage = false;
        intentDmg = 0;
    }

    public static EnemyMoveInfo get(int _nextMove, Intent _intent, int _intentBaseDmg, int _multiplier, bool _isMultiDamage)
    {
        var info = CLASS<EnemyMoveInfo>();
        info.setup(_nextMove, _intent, _intentBaseDmg, _multiplier, _isMultiDamage);
        return info;
    }

    public static void release(EnemyMoveInfo info)
    {
        UN_CLASS(info);
    }

    public void setup(int _nextMove, Intent _intent, int _intentBaseDmg, int _multiplier, bool _isMultiDamage)
    {
        nextMove = _nextMove;
        intent = _intent;
        baseDamage = _intentBaseDmg;
        multiplier = _multiplier;
        isMultiDamage = _isMultiDamage;
    }
}

public class EnemyMoveInfoGroup : ClassObject
{
    protected List<EnemyMoveInfo> moveInfos = new();

    public override void resetProperty()
    {
        base.resetProperty();
        moveInfos.Clear();
    }

    public override void onCreate()
    {
        base.onCreate();
    }

    public override void destroy()
    {
        base.destroy();

        for (var i = moveInfos.Count - 1; i >= 0; i--)
        {
            var info = moveInfos[i];
            removeMove(info);
        }
    }

    public List<EnemyMoveInfo> getMoveInfos()
    {
        return moveInfos;
    }

    public void addMove(int nextMove, Intent intent, int baseDamage, int multiplier, bool isMultiDamage)
    {
        var info = EnemyMoveInfo.get(nextMove, intent, baseDamage, multiplier, isMultiDamage);
        moveInfos.Add(info);
    }

    public void removeMove(EnemyMoveInfo info)
    {
        EnemyMoveInfo.release(info);
        moveInfos.Remove(info);
    }
}
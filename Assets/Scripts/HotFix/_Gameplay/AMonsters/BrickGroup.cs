using System.Collections.Generic;

namespace MarbleHero;

public struct EnemyMoveInfo
{
    public int nextMove;
    public Intent intent;
    public int baseDamage;
    public int multiplier;
    public bool isMultiDamage;

    public EnemyMoveInfo(int _nextMove, Intent _intent, int _intentBaseDmg, int _multiplier, bool _isMultiDamage)
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
    public List<EnemyMoveInfo> moveInfos = new();

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


        moveInfos.Clear();
    }

    public void addMove(int nextMove, Intent intent, int baseDamage, int multiplier, bool isMultiDamage)
    {
        var info = new EnemyMoveInfo(nextMove, intent, baseDamage, multiplier, isMultiDamage);
        moveInfos.Add(info);
    }
}
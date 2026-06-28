namespace MarbleHero;

/// <summary>
/// 弹药供给
/// 当前回合数为3的倍数时获得1个基础球
/// </summary>
public class AmmoSupply : ARelic
{
    public static string ID = "AmmoSupply";

    public AmmoSupply() : base(ID, "AmmoSupply.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);
    }

    public override void onPlayerTurnBegin(APlayer p)
    {
        var b = GameActionManager.turn % 3 == 0;
        if (b)
        {
            p.ballMaxCount++;
        }
    }

    public override ARelic makeCopy() => new AmmoSupply();
}
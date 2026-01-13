namespace MarbleHero;

/// <summary>
/// 自由球
/// 发球点可以左右移动
/// </summary>
public class FreeBall : ARelic
{
    public static string ID = "FreeBall";

    public FreeBall() : base(ID, "FreeBall.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onEquip(APlayer p)
    {
        base.onEquip(p);
    }

    public override void onPlayerTurnUpdate(APlayer p, float dt)
    {
        if (InputActionSet.left.isPressed() && !InputActionSet.right.isPressed())
        {
            p.moveShootPositionX(-dt);
        }
        else if (InputActionSet.right.isPressed() && !InputActionSet.left.isPressed())
        {
            p.moveShootPositionX(dt);
        }
    }

    public override void onUnequip(APlayer p)
    {
        base.onUnequip(p);
    }

    public override ARelic makeCopy() => new FreeBall();
}
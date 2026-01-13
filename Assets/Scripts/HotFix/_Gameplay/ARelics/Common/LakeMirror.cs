namespace MarbleHero;

/// <summary>
/// 湖中镜
/// 发球时若朝着下方发射，发球点则会出现在顶边界。
/// </summary>
public class LakeMirror : ARelic
{
    public static string ID = "LakeMirror";
    float directionY = 1F;

    public LakeMirror() : base(ID, "LakeMirror.png", RelicTier.COMMON, LandingSound.SOLID)
    {
    }

    public override void onPlayerTurnUpdate(APlayer p, float dt)
    {
        var guideLine = p.getGuideLine();
        var direction = guideLine.getRawShootDirection();
        if (directionY > 0 && direction.y < 0)
        {
            var position = levelManager.borderTop.getWorldPosition();
            position.x = p.shootPosition.x;
            p.setCurrentShootPosition(position);
            guideLine.setShootDirectionLimitAngle(-357, -97);
            guideLine.removeMask(BORDER_TOP_LAYER_MASK);
        }
        else if (directionY < 0 && direction.y > 0)
        {
            var position = levelManager.borderBot.getWorldPosition();
            position.x = p.shootPosition.x;
            p.setCurrentShootPosition(position);
            guideLine.resetShootDirectionLimitAngle();
            guideLine.addMask(BORDER_TOP_LAYER_MASK);
        }

        directionY = direction.y;
    }


    public override ARelic makeCopy() => new LakeMirror();
}
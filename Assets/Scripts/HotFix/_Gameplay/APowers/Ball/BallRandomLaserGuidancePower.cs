using UnityEngine;

namespace MoreMountains;

public interface ILaserBeamEffectAngleModifier
{
    void modifyAngle(Vector3 pos, ref float angle);
}

/// <summary>
/// 随机激光球的引导能力
/// </summary>
public class BallRandomLaserGuidancePower : BallPower, ILaserBeamEffectAngleModifier
{
    public void modifyAngle(Vector3 pos, ref float angle)
    {
        if (brickManager.getTheToppestHealthActiveBrick(out var result))
        {
            var dir = result.getCenterPosition() - (Vector2)pos;
            angle = dir.angle() + 180F;
            angle = 360 - angle;
        }
    }
}
using UnityEngine;

namespace MoreMountains;

public interface ILaserBeamEffectAngleModifier
{
    void modifyAngle(Vector3 pos, ref float angle);
}
public interface IBallDuplicateEffectModifier
{
    void modifyCount(ref int count);
}
public interface IBallFissionEffectModifier
{
    void modifyCount(ref int count);
}
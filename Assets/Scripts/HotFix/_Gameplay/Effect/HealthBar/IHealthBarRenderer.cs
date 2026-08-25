using UnityEngine;

namespace MoreMountains;

public interface IHealthBarRenderer
{
    void ApplyDamage(float curHpPct, Color? chunkColor = null);
    void SetProgress(float curPct);
    void ClearAllChunks();
    void ApplyToMaterial();
}
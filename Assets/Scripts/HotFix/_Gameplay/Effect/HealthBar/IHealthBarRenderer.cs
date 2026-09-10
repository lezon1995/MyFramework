namespace MoreMountains;

public interface IHealthBarRenderer
{
    void SetHealth(Health h);
    void ApplyDamageToHealthBar(float curHpPct);
    void ApplyDamageToShieldBar(float curProgress);
    void RefreshHealthBarAndShieldBar();
    void SetProgress(float curPct);
    void ClearAllChunks();
    void ApplyToMaterial();
}
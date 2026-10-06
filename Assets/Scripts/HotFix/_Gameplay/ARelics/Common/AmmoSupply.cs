namespace MoreMountains;

/// <summary>
/// 掉落金币时立即拾取
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

    public override void onDropCoin(APlayer p, Coin coin)
    {
        if (coin.State is CoinState.BeingCollected or CoinState.Collected)
            return;

        coinManager.PickupCoin(coin, p.getTransform());
    }

    public override ARelic makeCopy() => new AmmoSupply();
}
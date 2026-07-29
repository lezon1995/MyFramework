namespace MoreMountains
{
    /// <summary>
    /// 扣金币原因枚举，ACreature.loseGold / APlayer.loseGold 用它做业务路由。
    /// 注意：这个 enum 是项目里过去漏掉定义的既有依赖项，由这个项目新增补齐，
    /// 不会破坏既有逻辑（只是给现有"PayType.DEFAULT"等使用提供合法类型）。
    /// </summary>
    public enum PayType
    {
        DEFAULT,
        BALL_BUY,
        RELIC_BUY,
        BALL_REROLL,
        RELIC_REROLL,
        BALL_UPGRADE,
        BALL_MERGE,
        BUY_EXP,
        BUY_RELIC_BAG_SIZE,
        BUY_BALL_BAG_SIZE,
        BUY_SLOT,
        OTHER,
    }

    /// <summary>
    /// 加金币原因枚举，与 PayType 对称。
    /// </summary>
    public enum EarnType
    {
        DEFAULT,
        SELL_BALL,
        SELL_RELIC,
        REWARD,
        QUEST,
        OTHER,
    }
}

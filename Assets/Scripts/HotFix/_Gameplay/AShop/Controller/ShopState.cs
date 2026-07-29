namespace MoreMountains
{
    /// <summary>
    /// 商店系统的内部状态。
    /// </summary>
    public enum ShopState : byte
    {
        Idle,
        ShowingMixedBoard,
        ShowingBallBoard,
        ShowingRelicBoard,
        Done,
    }
}

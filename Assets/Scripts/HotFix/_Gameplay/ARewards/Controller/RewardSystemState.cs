namespace MoreMountains
{
    /// <summary>
    /// 升级奖励系统的内部状态。
    /// </summary>
    public enum RewardSystemState : byte
    {
        Idle,
        ShowingBallStatBoard,
        ShowingPlayerStatBoard,
        ShowingMixedBoard,
        Done,
    }
}

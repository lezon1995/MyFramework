using System;

namespace MoreMountains
{
    public enum EarnType
    {
        DEFAULT = 0,
        SELL_BALL,
        SELL_RELIC,
    }
    
    public enum PayType
    {
        DEFAULT = 0,
        
        BALL_UPGRADE,
        BALL_MERGE,
        BALL_BUY,
        BALL_REROLL,
        RELIC_BUY,
        RELIC_REROLL,
    }
    
    /// <summary>
    /// 玩家钱包 —— 唯一外部金币出入口。
    /// 硬币实体掉落 / 拾取仍然走 CoinManager（MoneyPickupAction 之类），
    /// 但所有"因购买/出售/升级/融合/重新随机"导致的金币变动，都必须通过 PlayerWallet。
    /// 后续接到金币系统时，建议在 CoinManager.OnGoldCollected 增加回调自动给钱包入账；
    /// 售出 / 升级扣金币时则调 Pay() 并配合新的"金币减少"动画。
    /// </summary>
    public class PlayerWallet : PlayerAbility
    {
        public int Balance { get; private set; }
        public event Action<int> OnBalanceChanged; // (newBalance)
        public event Action<int, PayType> OnPaid; // (amount, reason)
        public event Action<int, EarnType> OnEarned; // (amount, reason)

        protected override void Initialization()
        {
            base.Initialization();
            OnBalanceChanged?.Invoke(Balance);
        }

        public bool CanPay(int amount)
        {
            return Balance >= amount && amount >= 0;
        }

        /// <summary>
        /// 扣金币。可选 reason 用于审计。
        /// </summary>
        public bool Pay(int amount, PayType reason = PayType.DEFAULT)
        {
            if (amount <= 0)
                return true;

            if (Balance < amount)
            {
                logWarning($"PlayerWallet.Pay rejected: amount={amount}, balance={Balance}, reason={reason}");
                return false;
            }

            Balance -= amount;
            OnPaid?.Invoke(amount, reason);
            OnBalanceChanged?.Invoke(Balance);
            return true;
        }

        /// <summary>
        /// 加金币。
        /// </summary>
        public void Earn(int amount, EarnType reason = EarnType.DEFAULT)
        {
            if (amount <= 0)
                return;

            Balance += amount;
            OnEarned?.Invoke(amount, reason);
            OnBalanceChanged?.Invoke(Balance);
        }

        public void SetBalance(int newBalance, int type = 0)
        {
            var oldBalance = Balance;
            if (newBalance > oldBalance)
            {
                Earn(newBalance - oldBalance, (EarnType)type);
            }
            else if (newBalance < oldBalance)
            {
                Pay(oldBalance - newBalance, (PayType)type);
            }
        }

        public void ResetWallet(int initial = 0)
        {
            Balance = Math.Max(0, initial);
            OnBalanceChanged?.Invoke(Balance);
        }
    }
}
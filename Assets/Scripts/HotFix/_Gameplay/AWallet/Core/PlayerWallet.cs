using System;
using UnityEngine;
using static FrameBaseUtility;

namespace MoreMountains
{
    /// <summary>
    /// 玩家钱包 —— 唯一外部金币出入口。
    /// 硬币实体掉落 / 拾取仍然走 CoinManager（MoneyPickupAction 之类），
    /// 但所有"因购买/出售/升级/融合/重新随机"导致的金币变动，都必须通过 PlayerWallet。
    /// 后续接到金币系统时，建议在 CoinManager.OnGoldCollected 增加回调自动给钱包入账；
    /// 售出 / 升级扣金币时则调 Pay() 并配合新的"金币减少"动画。
    /// </summary>
    public class PlayerWallet : FrameSystem
    {
        public static PlayerWallet Instance { get; private set; }

        public int Balance { get; private set; }
        public event Action<int> OnBalanceChanged;     // (newBalance)
        public event Action<int, string> OnPaid;       // (amount, reason)
        public event Action<int, string> OnEarned;     // (amount, reason)

        public override void init()
        {
            base.init();
            Instance = this;
            OnBalanceChanged?.Invoke(Balance);
        }

        public override void willDestroy()
        {
            base.willDestroy();
            if (Instance == this) Instance = null;
        }

        public bool CanPay(int amount) => Balance >= amount && amount >= 0;

        /// <summary>
        /// 扣金币。可选 reason 用于审计。
        /// </summary>
        public bool Pay(int amount, string reason = "")
        {
            if (amount <= 0) return true;
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
        public void Earn(int amount, string reason = "")
        {
            if (amount <= 0) return;
            Balance += amount;
            OnEarned?.Invoke(amount, reason);
            OnBalanceChanged?.Invoke(Balance);
        }

        public void Reset(int initial = 0)
        {
            Balance = Math.Max(0, initial);
            OnBalanceChanged?.Invoke(Balance);
        }
    }
}

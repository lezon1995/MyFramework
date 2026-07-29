using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 玩家钱包 —— 唯一外部金币出入口。
    /// 球售出 / 商店购买 / 升级扣金币 / 重新随机扣金币 / 球融合扣金币 / BuyExp 都走这里。
    ///
    /// 接入方式：在 APlayer 的 GameObject 上 AddComponent 该脚本；外部通过
    ///     _player.Wallet.Pay(amount, payType)
    ///     _player.Wallet.Earn(amount, earnType)
    ///     _player.Wallet.CanPay(amount)
    ///     _player.Wallet.Balance         (APlayer.gold 读这里)
    ///     _player.Wallet.SetBalance(int) (APlayer.gold= 也写这里)
    /// 调用。
    ///
    /// 继承 PlayerAbility：基类会自动赋值 _player（不需要再写 Initialize(APlayer)）。
    /// 不维护静态 Instance，避免多玩家时串数据。
    /// Pay / Earn 接收项目既有的 PayType / EarnType 枚举，与 APlayer.loseGold / gainGold 保持类型一致。
    /// </summary>
    public sealed class PlayerWallet : PlayerAbility
    {
        [SerializeField] int initialBalance = 100;

        public int Balance { get; private set; }

        public event Action<int /*newBalance*/>                          OnBalanceChanged;
        public event Action<int /*amount*/, PayType /*reason*/>          OnPaid;
        public event Action<int /*amount*/, EarnType /*reason*/>         OnEarned;

        protected override void Initialization()
        {
            base.Initialization();
            Balance = initialBalance;
            OnBalanceChanged?.Invoke(Balance);
        }

        public bool CanPay(int amount) => Balance >= amount && amount >= 0;

        /// <summary>扣金币。余额不足返回 false 且不动数据。</summary>
        public bool Pay(int amount, PayType type = PayType.DEFAULT, string reason = null)
        {
            if (amount <= 0) return true;
            if (Balance < amount)
            {
                logWarning($"PlayerWallet.Pay rejected: amount={amount}, balance={Balance}, type={type}, reason={reason}");
                return false;
            }
            Balance -= amount;
            OnPaid?.Invoke(amount, type);
            OnBalanceChanged?.Invoke(Balance);
            return true;
        }

        /// <summary>加金币。</summary>
        public void Earn(int amount, EarnType type = EarnType.DEFAULT, string reason = null)
        {
            if (amount <= 0) return;
            Balance += amount;
            OnEarned?.Invoke(amount, type);
            OnBalanceChanged?.Invoke(Balance);
        }

        /// <summary>强写余额（用于重置或读 / 写 APlayer.gold）。</summary>
        public void SetBalance(int value, int type = 0)
        {
            Balance = Math.Max(0, value);
            OnBalanceChanged?.Invoke(Balance);
        }

        public void ResetWallet(int initial)
        {
            Balance = Math.Max(0, initial);
            OnBalanceChanged?.Invoke(Balance);
        }
    }
}

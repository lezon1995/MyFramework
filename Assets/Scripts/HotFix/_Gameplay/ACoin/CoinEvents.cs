using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 金币生成事件
    /// </summary>
    public struct OnCoinSpawned_S
    {
        public Coin Coin;
        public Vector3 Position;
        public Vector3 Direction;
        public int Value;

        public OnCoinSpawned_S(Coin coin, Vector3 position, Vector3 direction, int value)
        {
            Coin = coin;
            Position = position;
            Direction = direction;
            Value = value;
        }
    }

    /// <summary>
    /// 金币落地事件（掉落动画完成）
    /// </summary>
    public struct OnCoinLanded_S
    {
        public Coin Coin;
        public Vector3 Position;
        public int Value;

        public OnCoinLanded_S(Coin coin, Vector3 position, int value)
        {
            Coin = coin;
            Position = position;
            Value = value;
        }
    }

    /// <summary>
    /// 金币拾取动画完成事件（实际金币到账）
    /// </summary>
    public struct OnCoinCollected_S
    {
        public Coin Coin;
        public Vector3 PickupPosition;
        public int Value;
        public APicker Picker;

        public OnCoinCollected_S(Coin coin, Vector3 pickupPosition, int value, APicker picker)
        {
            Coin = coin;
            PickupPosition = pickupPosition;
            Value = value;
            Picker = picker;
        }
    }

    /// <summary>
    /// 玩家拾取金币总量事件
    /// </summary>
    public struct OnGoldPickedUp_S
    {
        public APicker Picker;
        public int Amount;
        public Vector3 Position;

        public OnGoldPickedUp_S(APicker picker, int amount, Vector3 position)
        {
            Picker = picker;
            Amount = amount;
            Position = position;
        }
    }

    /// <summary>
    /// 拾取者接口 - 实现此接口的对象可以拾取金币
    /// 用于将金币拾取事件通知给具体的拾取者
    /// </summary>
    public interface APicker
    {
        /// <summary>
        /// 拾取者位置
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// 当金币拾取完成时被调用（拾取动画结束后实际到账）
        /// </summary>
        void OnGoldCollected(int amount);
    }
}

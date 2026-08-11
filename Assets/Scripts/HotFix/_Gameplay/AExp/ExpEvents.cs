using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 经验值物品生成事件
    /// </summary>
    public struct OnExpSpawned_S
    {
        public ExpOrb ExpOrb;
        public Vector3 Position;
        public Vector3 Direction;
        public int Value;

        public OnExpSpawned_S(ExpOrb expOrb, Vector3 position, Vector3 direction, int value)
        {
            ExpOrb = expOrb;
            Position = position;
            Direction = direction;
            Value = value;
        }
    }

    /// <summary>
    /// 经验值物品落地事件（掉落动画完成）
    /// </summary>
    public struct OnExpLanded_S
    {
        public ExpOrb ExpOrb;
        public Vector3 Position;
        public int Value;

        public OnExpLanded_S(ExpOrb expOrb, Vector3 position, int value)
        {
            ExpOrb = expOrb;
            Position = position;
            Value = value;
        }
    }

    /// <summary>
    /// 经验值物品拾取动画完成事件（实际经验值到账）
    /// </summary>
    public struct OnExpCollected_S
    {
        public ExpOrb ExpOrb;
        public Vector3 PickupPosition;
        public int Value;
        public IExpPicker Picker;

        public OnExpCollected_S(ExpOrb expOrb, Vector3 pickupPosition, int value, IExpPicker picker)
        {
            ExpOrb = expOrb;
            PickupPosition = pickupPosition;
            Value = value;
            Picker = picker;
        }
    }

    /// <summary>
    /// 玩家拾取经验值总量事件
    /// </summary>
    public struct OnExpPickedUp_S
    {
        public IExpPicker Picker;
        public int Amount;
        public Vector3 Position;

        public OnExpPickedUp_S(IExpPicker picker, int amount, Vector3 position)
        {
            Picker = picker;
            Amount = amount;
            Position = position;
        }
    }

    /// <summary>
    /// 拾取者接口 - 实现此接口的对象可以拾取经验值
    /// 用于将经验值拾取事件通知给具体的拾取者
    /// </summary>
    public interface IExpPicker
    {
        /// <summary>
        /// 拾取者位置
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// 当经验值物品拾取动画完成时被调用（飞向玩家动画真正到达玩家位置后实际到账）
        /// </summary>
        void OnExpCollected(int amount);
    }
}
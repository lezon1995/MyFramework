using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// MOBA 风格的护盾系统扩展。
    /// 护盾独立于生命值存在，受伤时优先扣除护盾，护盾为 0 时才开始扣除生命值。
    /// </summary>
    [Serializable]
    public class HealthShield
    {
        Health _health;

        public void SetHealth(Health health)
        {
            _health = health;
        }
        
        #region Fields

        [Header("护盾基础属性")]
        [Tooltip("当前护盾值")]
        public int CurrentShield;

        [Header("护盾再生")]
        [Tooltip("基础护盾回复速度（每秒回复 X 点护盾）")]
        public float BaseShieldRegen;

        [Header("护盾减伤")]
        [Tooltip("护盾受到伤害时减免的比例（0-1），0 表示不减免，1 表示完全免疫")]
        public float DamageReductionRatio;

        [Tooltip("护盾穿透伤害比例（穿透护盾直接伤害血量，0-1），0 表示不穿透，1 表示完全穿透")]
        [Range(0f, 1f)]
        public float ShieldPenetrationRatio;

        #endregion

        #region Properties

        /// <summary> 护盾是否有效（大于 0） </summary>
        public bool HasShield => CurrentShield > 0;

        #endregion

        #region Events

        //护盾变化时触发，参数：previousShield, currentShield
        public event Action<int, int> OnShieldChanged;

        //护盾被完全打破时触发
        public event Action OnShieldDepleted;

        //护盾吸收伤害时触发，参数：damageAbsorbedByShield
        public event Action<int> OnShieldAbsorbDamage;

        #endregion

        #region Initialization

        /// <summary>
        /// 完全清空护盾值。
        /// </summary>
        public void ClearShield()
        {
            SetShield(0);
        }

        #endregion

        #region Shield Manipulation

        /// <summary>
        /// 设置护盾值。
        /// </summary>
        /// <param name="value">新的护盾值</param>
        protected void SetShield(int value)
        {
            int prevShield = CurrentShield;
            CurrentShield = Mathf.Max(0, value);

            _health.onShieldChanged?.Invoke(prevShield, CurrentShield);
            OnShieldChanged?.Invoke(prevShield, CurrentShield);

            // 检测护盾是否刚被清空
            if (prevShield > 0 && CurrentShield <= 0)
            {
                OnShieldDepleted?.Invoke();
            }
        }

        /// <summary>
        /// 增加护盾值（护盾不会超过最大护盾上限）。
        /// </summary>
        /// <param name="value">增加的护盾值</param>
        public void AddShield(int value)
        {
            if (value <= 0)
                return;

            int newShield = CurrentShield + value;
            SetShield(newShield);
        }

        /// <summary>
        /// 减少护盾值（不会低于 0）。
        /// </summary>
        public void RemoveShield(int value)
        {
            if (value <= 0)
                return;

            SetShield(CurrentShield - value);
        }

        #endregion

        #region Damage Processing (Core Shield Logic)

        /// <summary>
        /// 处理护盾的伤害吸收。
        /// 先将伤害作用于护盾，护盾减免后计算实际护盾消耗。
        /// 若伤害超过护盾值，返回实际穿透到生命值的伤害量。
        /// </summary>
        /// <param name="rawDamage">原始伤害值（传入前应已完成伤害计算）</param>
        /// <returns>穿透护盾后需要作用于生命值的伤害值</returns>
        public int AbsorbDamage(int rawDamage, out int shieldedDamage)
        {
            if (rawDamage <= 0 || CurrentShield <= 0)
            {
                // 无护盾且无穿透时，全部伤害穿透到生命值
                shieldedDamage = 0;
                return rawDamage;
            }

            // 计算护盾减免后的实际伤害
            int absorbedDamage = Mathf.CeilToInt(rawDamage * (1f - DamageReductionRatio));

            // 触发护盾吸收事件
            OnShieldAbsorbDamage?.Invoke(absorbedDamage);

            // 计算护盾能吸收多少伤害
            if (absorbedDamage <= CurrentShield)
            {
                // 护盾足够，直接扣除护盾，无穿透
                RemoveShield(absorbedDamage);
                shieldedDamage = absorbedDamage;
                return 0;
            }

            // 护盾不足，先清空护盾，剩余伤害按穿透比例穿透到生命值
            // 计算穿透伤害
            shieldedDamage = CurrentShield;
            int remainingDamage = absorbedDamage - CurrentShield;
            ClearShield();
            int penetratingDamage = Mathf.CeilToInt(remainingDamage * (1f - ShieldPenetrationRatio));
            return penetratingDamage;
        }

        #endregion

        #region Shield Regen

        /// <summary>
        /// 每帧更新护盾回复。
        /// </summary>
        /// <param name="dt">帧间隔时间（秒）</param>
        /// <param name="regenBonus">额外的回复加成（可为负数）</param>
        public void UpdateShieldRegen(float dt, float regenBonus = 0f)
        {
            if (CurrentShield <= 0)
                return;

            float totalRegen = BaseShieldRegen + regenBonus;
            if (totalRegen <= 0)
                return;

            float accumulated = totalRegen * dt;

            // 每秒回复一次
            if (accumulated >= 1f)
            {
                int regenValue = Mathf.FloorToInt(accumulated);
                AddShield(regenValue);
            }
        }

        #endregion
    }
}
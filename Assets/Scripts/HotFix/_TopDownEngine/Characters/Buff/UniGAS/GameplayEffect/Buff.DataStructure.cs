using System;
using Sirenix.OdinInspector;
using UniStats;
using UnityEngine;

namespace MoreMountains
{
    public partial class Buff
    {
        public struct Param
        {
            public Vector3 Dir;
            public int Stack;

            public Param()
            {
                Dir = Vector3.zero;
                Stack = 1;
            }

            public Param(int stack) : this()
            {
                Stack = stack;
            }

            public Param(Vector3 dir, int stack)
            {
                Dir = dir;
                Stack = stack;
            }
        }

        public enum InstanceModes
        {
            //只保存一份实例，多次Apply相同的Buff，多余的Buff实例会被丢弃。
            //Eg：假如同时施加3个点燃效果，那么Target身上会保存第一个添加的Buff实例，后面的两个都会被丢弃
            //每一跳同时受到1次点燃的周期伤害
            Single,

            //无法叠加，分别应用多份实例，多次Apply相同的Buff，会在Target身上保存多份Buff实例。
            //Eg：假如同时施加3个点燃效果，那么Target身上会保存3份Buff实例，并且每一跳同时受到3次点燃的周期伤害
            Multiple,
        }

        //Buff被移除的原因种类
        public enum Removal
        {
            None,
            Routinely,
            MaxStacked,
            Overflowed,
            Exclusively,
            ApplyStack,
            
            StackExpirePolicy_ClearAllStack,
            StackExpirePolicy_DecreaseLastStack,
            
            WithTag,
            WithType,
            Death,
        }

        public enum Types
        {
            Instant,
            Duration,
            Infinite,
        }

        public enum Actors
        {
            //作用于 目标Buffable（默认）
            Target,

            //作用于 来源Buffable
            Source,
        }

        /// <summary>
        /// Buff的添加结果
        /// </summary>
        public enum Result
        {
            //丢弃
            Discard,

            //添加 从0到1
            FirstAdd,

            //添加叠加类GE 从0到1
            FirstStack,

            //叠加 从1到1+
            Stacking,

            //溢出 从max到max+1 具有超过最大叠加层数的趋势
            StackOverflow,
        }

        [Serializable]
        public class Data
        {
            [HorizontalGroup("Data", Width = 0.2F), HideLabel]
            public Actors ApplyTo;

            [HorizontalGroup("Data", Width = 0.7F), HideLabel]
            public Buff Buff;

            [HorizontalGroup("Data", Width = 0.1F), HideLabel]
            public int Stack = 1;
        }

        [Serializable]
        public class Mod
        {
            public enum Algos
            {
                Flat,
                Pct,
            }

            [HorizontalGroup, HideLabel]
            [SuffixLabel("Stat", Overlay = true)]
            [ValueDropdown("@StatKey.Names")]
            public string Stat;

            [HorizontalGroup, HideLabel]
            public Algos Algo;

            [InlineProperty, HideLabel]
            public Mag Mag;

            public float Value(Buff buff) => Mag.Value(buff);

            public UniStats.Stat StatFrom(Buffable buffable) => buffable.GetStat(Stat);

            public override string ToString()
            {
                return GetHashCode().ToString();
            }
        }

        [Serializable]
        public class Mag
        {
            [Serializable]
            public class Item
            {
                public enum Types
                {
                    Const,
                    Stat,
                    Mixed,
                    BasedOnLevel,
                }

                [HorizontalGroup, HideLabel]
                public Types Type;

                [ShowIf("@this.Type == Types.Const || this.Type == Types.Mixed")]
                [HorizontalGroup, HideLabel, SuffixLabel("Const", Overlay = true)]
                public float Const;

                [ShowIf("@this.Type == Types.Stat || this.Type == Types.Mixed || this.Type == Types.BasedOnLevel")]
                [HorizontalGroup, HideLabel]
                public Actors Actor;

                [ShowIf("@this.Type == Types.Stat || this.Type == Types.Mixed")]
                [HorizontalGroup, HideLabel, ValueDropdown("@StatKey.Names")]
                public string Stat;

                [ShowIf("@this.Type == Types.Stat || this.Type == Types.Mixed")]
                [HorizontalGroup, ToggleLeft]
                public bool Bonus;

                [ShowIf(nameof(Type), Types.BasedOnLevel)]
                [HorizontalGroup, HideLabel]
                public RangeByLevel Range;

                [Serializable]
                [InlineProperty]
                public struct RangeByLevel
                {
                    [HorizontalGroup, HideLabel, SuffixLabel("Min", Overlay = true)]
                    public float Min;

                    [HorizontalGroup, HideLabel, SuffixLabel("Max", Overlay = true)]
                    public float Max;
                }

                public float Value(Buff buff)
                {
                    return Type switch
                    {
                        Types.Const => Const,
                        Types.Stat => StatValue(buff),
                        Types.Mixed => StatValue(buff) * Const,
                        Types.BasedOnLevel => RangeValue(buff),
                        _ => Const
                    };
                }

                float StatValue(Buff buff)
                {
                    return Actor switch
                    {
                        Actors.Target => Bonus ? buff.Target.GetStat(Stat).PeekBonus() : buff.Target.GetStat(Stat).Value,
                        Actors.Source => Bonus ? buff.Source.GetStat(Stat).PeekBonus() : buff.Source.GetStat(Stat).Value,
                        _ => 0
                    };
                }

                float RangeValue(Buff buff)
                {
                    var level = Actor switch
                    {
                        Actors.Target => buff.Target.Level,
                        Actors.Source => buff.Source.Level,
                        _ => 0
                    };
                    var levelMax = Actor switch
                    {
                        Actors.Target => buff.Target.LevelMax,
                        Actors.Source => buff.Source.LevelMax,
                        _ => 0
                    };

                    if (levelMax == 0)
                        return 0;

                    return Mathf.Lerp(Range.Min, Range.Max, Mathf.InverseLerp(1, levelMax, level));
                }
            }

            public Item[] Items;

            public virtual float Value(Buff buff)
            {
                if (Items == null || Items.Length == 0)
                    return 0F;

                float value = 0F;
                foreach (var item in Items)
                    value += item.Value(buff);
                return value;
            }
        }

        [Serializable]
        public class DmgMag : Mag
        {
            [HorizontalGroup, HideLabel]
            public Dmg.Types DmgType;

            [HorizontalGroup, HideLabel]
            public Dmg.Algos DmgAlgo;
        }

        [Serializable]
        public class HealMag : Mag
        {
            public Heal.Algos HealAlgo;
        }

        [Serializable]
        public class AbilityData
        {
            public bool RemoveWithBuff = true;

            [HorizontalGroup, HideLabel]
            public Actors ApplyTo;

            [HorizontalGroup, HideLabel]
            public CharacterAbility Ability;
        }

        [Serializable]
        public class StackSource
        {
            [HorizontalGroup("StackSource")]
            public Sources Source;

            [HorizontalGroup("StackSource")]
            public int Increment;

            [HorizontalGroup("StackSource")]
            public float Unit = 1;

            public enum Sources
            {
                DoAttackHit,
                DoMove,
            }
        }
    }
}
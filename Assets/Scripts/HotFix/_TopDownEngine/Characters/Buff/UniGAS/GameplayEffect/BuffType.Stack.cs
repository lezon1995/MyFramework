using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    public partial class BuffType : SerializedScriptableObject
    {
        public StackConfig stack;

        internal Dictionary<Buffable, Buff> Exclusive = new();

        [Serializable, HideLabel]
        [BoxGroup("Stack", order: 4), ShowIfGroup("Stack/Toggle", Condition = STACKABLE)]
        public class StackConfig
        {
            public StackType Type;

            public DeltaStack Delta;

            [BoxGroup("MaxStack", false), InlineProperty]
            public Buff.Mag MaxStack;

            [ToggleLeft]
            [Tooltip("是否只能同时叠加在一个单位上")]
            public bool Exclusive;

            [ToggleLeft]
            public bool RefreshDurationOnStacked = true;

            [ToggleLeft]
            public bool ResetPeriodOnStacked;

            [ToggleLeft]
            public bool ClearOnMaxStacked;

            [ToggleLeft]
            public bool HasExtraStackSources;

            public StackExpirePolicy StackExpirePolicy;

            [ShowIf(nameof(StackExpirePolicy), StackExpirePolicy.DecreaseStack)]
            public Decreasing DecreasingDuration;

            public Buff.Data[] MaxStackBuffs;

            public Buff.Mod[] Mods;

            [ShowIf(nameof(HasExtraStackSources))]
            public Buff.StackSource[] ExtraStackSources;

            [Serializable]
            [InlineProperty]
            public class DeltaStack
            {
                [HorizontalGroup]
                public int Incre = 1;

                [HorizontalGroup]
                public int Decre = 1;
            }

            [Serializable]
            [InlineProperty]
            public class Decreasing
            {
                [HorizontalGroup]
                public bool Override;

                [HorizontalGroup, EnableIf(nameof(Override)), HideLabel]
                public float Duration;
            }
        }
    }

    public enum StackType
    {
        //在Target上叠加
        //Example：假如有A和B两个火男，A对Target释放一次技能叠加第1层灼烧，B对Target释放一次技能叠加第2层灼烧，A和B共享在Target身上的叠加。
        ByTarget,

        //在Source上叠加
        //Example：假如有A和B两个火男，A对Target释放一次技能叠加一层灼烧（来自A），B对Target释放一次技能叠加一层灼烧（来自B），Target身上存在两种相同类型的叠加各一层。
        BySource,
    }

    public enum StackExpirePolicy
    {
        //清空所有叠加
        ClearAllStack,

        //移除一层叠加 并刷新Duration
        DecreaseStack,

        //刷新Duration (无限循环 具体逻辑可以自行实现)
        RefreshDuration,
    }
}
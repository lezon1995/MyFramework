using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains
{
    public partial class BuffType : SerializedScriptableObject
    {
        public PeriodConfig period;

        [Serializable, HideLabel]
        [BoxGroup("Period", order: 3), ShowIfGroup("Period/Toggle", Condition = PERIODIC)]
        public class PeriodConfig
        {
            [BoxGroup("Period/Time", false), InlineProperty]
            public Buff.Mag Time;

            [ToggleLeft]
            public bool ExecuteOnApply;

            [ToggleLeft, LabelText("Damage")]
            public bool IsPeriodDamage;

            [ShowIf(nameof(IsPeriodDamage)), HideLabel]
            public Buff.DmgMag PeriodDamage;

            [ToggleLeft, LabelText("Heal")]
            public bool IsPeriodHeal;

            [ShowIf(nameof(IsPeriodHeal)), HideLabel]
            public Buff.HealMag PeriodHeal;

            [HideInInspector]
            public PeriodicInhibitionPolicy PeriodicInhibitionPolicy;

            public Buff.Mod[] Mods;
        }
    }

    public enum PeriodicInhibitionPolicy
    {
        NeverReset,
        ResetPeriod,
        ExecuteAndResetPeriod,
    }
}
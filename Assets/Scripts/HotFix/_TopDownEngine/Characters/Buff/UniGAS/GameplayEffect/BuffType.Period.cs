using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    public partial class Buff
    {
        public PeriodConfig period;

        [Serializable, HideLabel]
        [BoxGroup("Period", order: 3), ShowIfGroup("Period/Toggle", Condition = PERIODIC)]
        public class PeriodConfig
        {
            [BoxGroup("Time", false), InlineProperty]
            public Mag Time;

            [ToggleLeft]
            public bool ExecuteOnApply;

            [ToggleLeft, LabelText("Damage")]
            public bool IsPeriodDamage;

            [ShowIf(nameof(IsPeriodDamage)), HideLabel]
            public DmgMag PeriodDamage;

            [ToggleLeft, LabelText("Heal")]
            public bool IsPeriodHeal;

            [ShowIf(nameof(IsPeriodHeal)), HideLabel]
            public HealMag PeriodHeal;

            [HideInInspector]
            public PeriodicInhibitionPolicy PeriodicInhibitionPolicy;

            public Mod[] Mods;
        }
    }

    public enum PeriodicInhibitionPolicy
    {
        NeverReset,
        ResetPeriod,
        ExecuteAndResetPeriod,
    }
}
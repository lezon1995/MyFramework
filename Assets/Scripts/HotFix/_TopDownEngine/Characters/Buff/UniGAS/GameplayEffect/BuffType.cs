using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// A scriptable object you can create assets from, to identify damage types
    /// </summary>
    public partial class Buff
    {
        public MainConfig main;

        public bool IsInstant => main.Type == Types.Instant;
        public bool IsDuration => main.Type == Types.Duration;
        public bool IsInfinite => main.Type == Types.Infinite;
        public bool IsPeriodic => main.Periodic;
        public bool IsStackable => main.Stackable;
        public bool HasCooldown => main.HasCooldown;
        public bool WithEvent => main.WithEvent;
        public InstanceModes InstanceMode => main.InstanceMode;

        const string INSTANT = nameof(IsInstant);
        const string PERIODIC = nameof(IsPeriodic);
        const string STACKABLE = nameof(IsStackable);
        const string COOLDOWN = nameof(HasCooldown);
        const string EVENT = nameof(WithEvent);

        [Serializable, HideLabel]
        [BoxGroup("Main", order: 1)]
        public class MainConfig
        {
            public string Name;
            public string Desc;
            public Types Type;

            [HideIf(nameof(Type), Types.Instant)]
            [DisableIf(nameof(Stackable))]
            public InstanceModes InstanceMode;

            [HideIf(nameof(Type), Types.Instant)]
            [ToggleLeft]
            public bool Periodic;

            [HideIf(nameof(Type), Types.Instant)]
            [ToggleLeft, OnValueChanged(nameof(OnStackableChanged))]
            public bool Stackable;

            [ToggleLeft]
            public bool HasCooldown;

            [ToggleLeft]
            public bool WithEvent;

            void OnStackableChanged()
            {
                if (Stackable) InstanceMode = InstanceModes.Single;
            }

            [ShowIf(nameof(Type), Types.Duration)]
            [BoxGroup("Duration", false), InlineProperty]
            public Mag Duration;

            [ShowIf(nameof(Type), Types.Duration)]
            [ToggleLeft]
            public bool RefreshDurationWhileInCombat;

            [HideIf(nameof(Type), Types.Instant)]
            public Mod[] Mods;

            [ShowIf(nameof(Type), Types.Instant)]
            [ToggleLeft, LabelText("Damage")]
            [BoxGroup("Instant")]
            public bool IsInstantDamage;

            [ShowIf(nameof(ShowInstantDamage)), HideLabel]
            [BoxGroup("Instant")]
            public DmgMag InstantDamage;

            [ShowIf(nameof(IsInstantDamage))]
            [ToggleLeft, LabelText("Alternative")]
            [BoxGroup("Instant")]
            public bool HasAlternativeInstantDamage;

            [ShowIf(nameof(HasAlternativeInstantDamage)), HideLabel]
            [BoxGroup("Instant")]
            public DmgMag AlternativeInstantDamage;

            [PropertySpace(SpaceBefore = 30)]
            [ShowIf(nameof(Type), Types.Instant)]
            [ToggleLeft, LabelText("Heal")]
            [BoxGroup("Instant")]
            public bool IsInstantHeal;

            [ShowIf(nameof(ShowInstantHeal)), HideLabel]
            [BoxGroup("Instant")]
            public HealMag InstantHeal;

            [HideInInspector]
            public ConditionalBuff[] ConditionalBuffs;

            bool ShowInstantDamage => Type == Types.Instant && IsInstantDamage;
            bool ShowInstantHeal => Type == Types.Instant && IsInstantHeal;
        }
    }
}
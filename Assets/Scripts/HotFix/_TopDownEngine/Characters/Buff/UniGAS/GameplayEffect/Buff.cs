// using MoreMountains.Feedbacks;

using System;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UniStats;
using UnityEngine;

namespace MoreMountains
{
    [HideMonoScript]
    public partial class Buff : SerializedMonoBehaviour, IResetable
    {
        [ShowInInspector, ReadOnly, HorizontalGroup("Buffable", order: -90)]
        public Buffable Source { get; private set; }

        [ShowInInspector, ReadOnly, HorizontalGroup("Buffable", order: -90)]
        public Buffable Target { get; private set; }

        public Buffable Owner { get; set; }

        public bool IsPrototype { get; internal set; }
        public bool HasReset { get; internal set; }
        public Transform DefaultParent { get; internal set; }
        public Transform CurrentParent { get; private set; }

        public Action OnRemoved { get; set; }
        public Action<Buffable> OnRemovedTarget { get; set; }
        public Func<Dmg> DmgGetter { get; set; }

        Mod[] mods => main.Mods;
        bool isInstantDamage => main.IsInstantDamage;
        DmgMag instantDamage => main.InstantDamage;
        bool hasAlternativeInstantDamage => main.HasAlternativeInstantDamage;
        DmgMag alternativeInstantDamage => main.AlternativeInstantDamage;
        bool isInstantHeal => main.IsInstantHeal;
        HealMag instantHeal => main.InstantHeal;
        ConditionalBuff[] conditionalBuffs => main.ConditionalBuffs;

        // public MMFeedbacks FB_Instant;

        protected virtual void OnInstant()
        {
        }

        public void Initialize(Buffable source, Buffable target)
        {
            Source = source;
            Target = target;
            InitializePeriod();
            InitializeStack();
            OnInitialized();
            HasReset = false;
        }

        public void SetParent(Transform parent)
        {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        protected Buffable GetActor(Actors type)
        {
            return type switch
            {
                Actors.Target => Target,
                Actors.Source => Source,
                _ => Target
            };
        }

        public bool Execute(Param param)
        {
            ApplyConditionalBuff();

            if (IsInstant)
            {
                ExecuteInstantBuff();

                //InstantBuff会立即检查是否AddCooldown
                CheckAddCooldown();

                //如果是瞬间的GE则会在应用后立即回收
                return false;
            }

            return Target.DispatchBuff(this, param);
        }

        void CheckAddCooldown()
        {
            if (hasCooldown)
            {
                var cooldown = MMCooldown.Get(cooldownDuration);
                var buffable = CooldownActor();
                buffable.BuffCooldown.Add((GetType(), cooldown));
            }
        }

        void ExecuteInstantBuff()
        {
            if (isInstantDamage)
                ExecuteInstantDamage();

            if (isInstantHeal)
                ExecuteInstantHeal();

            OnInstant();
            return;

            void ExecuteInstantDamage()
            {
                var mag = instantDamage;
                var value = mag.Value(this);

                if (hasAlternativeInstantDamage)
                {
                    var alternativeValue = alternativeInstantDamage.Value(this);
                    if (alternativeValue > value)
                    {
                        mag = alternativeInstantDamage;
                        value = alternativeValue;
                    }
                }

                if (value > 0)
                {
                    var dmg = new Dmg((int)value, mag.DmgType, mag.DmgAlgo);
                    Target.Health.Damage(ref dmg, gameObject, source: Source.Character);
                }
            }

            void ExecuteInstantHeal()
            {
                var mag = instantHeal;
                var value = mag.Value(this);
                if (value > 0)
                {
                    var heal = new Heal((int)value, mag.HealAlgo);
                    Target.Health.ReceiveHealth(heal, source: Target.Character);
                }
            }
        }

        void AddMod(Mod mod, string key)
        {
            var value = mod.Value(this);
            var stat = mod.StatFrom(Target);
            if (stat == null)
                return;

            switch (mod.Algo)
            {
                case Mod.Algos.Flat:
                    stat.BonusFlat.AddFlat(value, key);
                    break;
                case Mod.Algos.Pct:
                    stat.BonusPct.AddFlat(value, key);
                    break;
            }
        }

        void AddOrEditMod(Mod mod, string key, int oldStack, int newStack)
        {
            var value = mod.Value(this);
            var stat = mod.StatFrom(Target);
            if (stat == null)
                return;
            
            if (!stat.GetMod(key, out var statMod))
            {
                switch (mod.Algo)
                {
                    case Mod.Algos.Flat:
                        stat.BonusFlat.AddFlat(value * newStack, key);
                        break;
                    case Mod.Algos.Pct:
                        stat.BonusPct.AddFlat(value * newStack, key);
                        break;
                }
            }
            else
            {
                switch (mod.Algo)
                {
                    case Mod.Algos.Flat:
                        statMod.Value = value * newStack;
                        break;
                    case Mod.Algos.Pct:
                        statMod.Value = value * newStack;
                        break;
                }
            }
        }

        void RemoveOrEditMod(Mod mod, string key, int oldStack, int newStack)
        {
            var value = mod.Value(this);
            var stat = mod.StatFrom(Target);
            if (stat == null)
                return;
            
            if (stat.GetMod(key, out var statMod))
            {
                if (newStack > 0)
                {
                    switch (mod.Algo)
                    {
                        case Mod.Algos.Flat:
                            statMod.Value = value * newStack;
                            break;
                        case Mod.Algos.Pct:
                            statMod.Value =  value * newStack;
                            break;
                    }
                }
                else
                {
                    stat.RemoveMod(key);
                }
            }
        }

        internal void AddMainMods()
        {
            var mods = this.mods;
            if (mods == null || mods.Length == 0)
                return;

            foreach (var mod in mods)
            {
                AddMod(mod, mod.ToString());
            }
        }

        void RemoveMainMods()
        {
            var mods = this.mods;
            if (mods == null || mods.Length == 0)
                return;

            foreach (var mod in mods)
            {
                var key = mod.ToString();
                var stat = mod.StatFrom(Target);
                if (stat == null)
                    continue;

                stat.RemoveMod(key);
            }
        }

        void ApplyConditionalBuff()
        {
            var buffs = conditionalBuffs;
            if (buffs == null || buffs.Length == 0)
                return;

            foreach (var data in buffs)
            {
                if (CheckConditionalBuffs(data))
                {
                    // GetActor(data.ApplyTo).ApplyBuff(data.Buff, Source);
                }
            }
        }

        public bool CheckConditionalBuffs(ConditionalBuff conditionalBuff)
        {
            return true;
        }

        public virtual void OnInitialized()
        {
        }

        public void reset()
        {
            HasReset = true;
            Source = null;
            Target = null;
            Owner = null;
            IsPrototype = false;
            IsKillByPeriodDamage = false;
            OnRemoved = null;
            OnPeriodDamage = null;
            OnStackChanged = null;

            ClearPeriod();
            ClearStack();
        }
    }
}
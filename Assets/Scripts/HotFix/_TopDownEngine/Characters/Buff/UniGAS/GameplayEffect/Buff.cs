// using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UniStats;
using UnityEngine;

namespace MoreMountains
{
    [HideMonoScript]
    public partial class Buff : SerializedMonoBehaviour
    {
        [CustomContextMenu("Create BuffType", nameof(CreateBuffType))]
        [InlineEditor, PropertyOrder(-100)]
        public BuffType BuffType;

        [ShowInInspector, ReadOnly, HorizontalGroup("Buffable", order: -90)]
        public Buffable Source { get; private set; }

        [ShowInInspector, ReadOnly, HorizontalGroup("Buffable", order: -90)]
        public Buffable Target { get; private set; }

        public Buffable Owner { get; set; }

        public Transform DefaultParent { get; internal set; }
        public Transform CurrentParent { get; private set; }

        bool _isStackable => BuffType.IsStackable;
        InstanceModes _instanceMode => BuffType.InstanceMode;
        bool _isInstant => BuffType.IsInstant;
        bool _isDuration => BuffType.IsDuration;
        bool _isInfinite => BuffType.IsInfinite;
        Mod[] _mods => BuffType.main.Mods;
        bool _isInstantDamage => BuffType.main.IsInstantDamage;
        DmgMag _instantDamage => BuffType.main.InstantDamage;
        bool _hasAlternativeInstantDamage => BuffType.main.HasAlternativeInstantDamage;
        DmgMag _alternativeInstantDamage => BuffType.main.AlternativeInstantDamage;
        bool _isInstantHeal => BuffType.main.IsInstantHeal;
        HealMag _instantHeal => BuffType.main.InstantHeal;
        ConditionalBuff[] _conditionalBuffs => BuffType.main.ConditionalBuffs;

        // public MMFeedbacks FB_Instant;

        protected virtual void OnInstant()
        {
        }

        public void Initialize(BuffType buffType, Buffable source, Buffable target)
        {
            BuffType = buffType;
            Initialize(source, target);
        }

        public void Initialize(Buffable source, Buffable target)
        {
            Source = source;
            Target = target;
            InitializePeriod();
            InitializeStack();
            OnInitialized();
        }

        void CreateBuffType()
        {
            if (BuffType)
            {
                Debug.Log("BuffType不为null，无法创建新的BuffType");
                return;
            }

            var buffType = ScriptableObject.CreateInstance<BuffType>();
            BuffType = buffType;
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

            if (_isInstant)
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
            if (_hasCooldown)
            {
                var cooldown = MMCooldown.Get(_cooldownDuration);
                var buffable = CooldownActor();
                buffable.BuffCooldown.Add((BuffType, cooldown));
            }
        }

        void ExecuteInstantBuff()
        {
            if (_isInstantDamage)
                ExecuteInstantDamage();

            if (_isInstantHeal)
                ExecuteInstantHeal();

            OnInstant();
            return;

            void ExecuteInstantDamage()
            {
                var mag = _instantDamage;
                var value = mag.Value(this);

                if (_hasAlternativeInstantDamage)
                {
                    var alternativeValue = _alternativeInstantDamage.Value(this);
                    if (alternativeValue > value)
                    {
                        mag = _alternativeInstantDamage;
                        value = alternativeValue;
                    }
                }

                if (value > 0)
                {
                    var dmg = new Dmg(value, mag.DmgType, mag.DmgAlgo);
                    Target.Health.Damage(ref dmg, gameObject, source: Source.Character);
                }
            }

            void ExecuteInstantHeal()
            {
                var mag = _instantHeal;
                var value = mag.Value(this);
                if (value > 0)
                {
                    var heal = new Heal(value, mag.HealAlgo);
                    Target.Health.ReceiveHealth(heal, source: Target.Character);
                }
            }
        }

        void AddMod(Mod mod, string key)
        {
            var value = mod.Value(this);
            var stat = mod.StatFrom(Target);
            switch (mod.Algo)
            {
                case Mod.Algos.Flat:
                    stat.AddFlat(value, key);
                    break;
                case Mod.Algos.Pct:
                    stat.AddPct(value, key);
                    break;
            }
        }

        void AddOrEditMod(Mod mod, string key, int oldStack, int newStack)
        {
            var value = mod.Value(this);
            var stat = mod.StatFrom(Target);
            if (!stat.GetMod(key, out var statMod))
            {
                switch (mod.Algo)
                {
                    case Mod.Algos.Flat:
                        stat.AddFlat(value * newStack, key);
                        break;
                    case Mod.Algos.Pct:
                        stat.AddPct(value * newStack, key);
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
                        statMod.Value = 1 + value * newStack;
                        break;
                }
            }
        }

        void RemoveOrEditMod(Mod mod, string key, int oldStack, int newStack)
        {
            var value = mod.Value(this);
            var stat = mod.StatFrom(Target);
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
                            statMod.Value = 1 + value * newStack;
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
            var mods = _mods;
            if (mods == null || mods.Length == 0)
                return;

            foreach (var mod in mods)
            {
                AddMod(mod, mod.ToString());
            }
        }

        void RemoveMainMods()
        {
            var mods = _mods;
            if (mods == null || mods.Length == 0)
                return;

            foreach (var mod in mods)
            {
                var key = mod.ToString();
                var stat = mod.StatFrom(Target);
                stat.RemoveMod(key);
            }
        }

        void ApplyConditionalBuff()
        {
            var buffs = _conditionalBuffs;
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

        public virtual void OnNew()
        {
        }

        public virtual void OnGet()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnInitialized()
        {
        }

        public virtual void OnRelease()
        {
            Clear();
            gameObject.SetActive(false);
            SetParent(DefaultParent);
            Owner = null;
        }

        protected void Clear()
        {
            BuffType = null;
            Source = null;
            Target = null;

            ClearPeriod();
            ClearStack();
        }
    }
}
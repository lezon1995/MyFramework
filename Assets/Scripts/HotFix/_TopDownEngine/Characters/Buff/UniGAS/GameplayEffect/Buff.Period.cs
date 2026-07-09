using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    public partial class Buff
    {
        Mag _duration => BuffType.main.Duration;
        bool _refreshDurationWhileInCombat => BuffType.main.RefreshDurationWhileInCombat;

        bool _isPeriodic
        {
            get
            {
                if (BuffType == null)
                    return false;

                return BuffType.IsPeriodic;
            }
        }

        Mag _period => BuffType.period.Time;
        bool _isPeriodDamage => BuffType.period.IsPeriodDamage;
        DmgMag _periodDamage => BuffType.period.PeriodDamage;
        bool _isPeriodHeal => BuffType.period.IsPeriodHeal;
        HealMag _periodHeal => BuffType.period.PeriodHeal;
        Mod[] _periodMods => BuffType.period.Mods;
        bool _executeOnApply => BuffType.period.ExecuteOnApply;

        public MMFeedbacks FB_PeriodicTick;


        void InitializePeriod()
        {
            DurationElapsed = 0F;
            PeriodElapsed = 0F;
            PeriodTick = 0;

            if (_isInstant)
                return;
            ResetPeriod();
        }

        public float Duration
        {
            get
            {
                if (BuffType == null)
                    return 0F;

                if (_isInfinite)
                    return 0F;

                if (OverrideDuration.HasValue)
                    return OverrideDuration.Value;

                if (IsStackDecreasing && _isOverrideDecreasingDuration)
                    return _DecreasingDuration;

                return _duration.Value(this);
            }
        }

        public float? OverrideDuration { get; set; }

        public float DurationElapsed { get; set; }

        public float DurationLeft => Duration - DurationElapsed;

        public float DurationPct
        {
            get
            {
                var durationLeft = DurationLeft;
                var duration = Duration;
                if (durationLeft == 0F) return 0F;
                if (duration == 0F) return 0F;
                return durationLeft / duration;
            }
        }

        public float Period => _isPeriodic ? _period.Value(this) : 0F;

        public float PeriodElapsed { get; set; }

        public float PeriodLeft => Period - PeriodElapsed;

        public float PeriodPct
        {
            get
            {
                if (PeriodLeft == 0F) return 0F;
                if (Period == 0F) return 0F;
                return PeriodLeft / Period;
            }
        }

        public int PeriodTick { get; set; }

        public bool OnUpdate(float dt)
        {
            if (_isDuration)
                DurationElapsed += dt;

            if (_isPeriodic)
            {
                //更新周期时间
                PeriodElapsed += dt;
                if (PeriodLeft <= 0)
                {
                    PeriodElapsed = 0;
                    OnPeriod();
                }
            }

            if (_isInfinite)
                return false;

            if (DurationLeft > 0)
                return false;

            if (_isStackable)
                return DoStackExpirePolicy();

            return true;
        }

        public void BeforeRemove(Removal removal, bool cooldown)
        {
            OnBeforeRemove();

            switch (removal, cooldown)
            {
                case (Removal.MaxStacked, true):
                case (Removal.Overflowed, true):
                case (Removal.ApplyStack, true):
                case (Removal.WithType, true):
                    //Buff结束时 检查是否施加冷却
                    CheckAddCooldown();
                    break;
            }

            //Buff结束时 移除所施加的属性修改器
            RemoveMainMods();
            RemovePeriodMods(PeriodTick, 0);
            RemoveStackMods(Stack, 0);

            if (removal == Removal.Routinely)
                ApplyExpiredBuff();
            else
                ApplyPreExpiredBuff();
        }

        void ExecutePeriodBuff()
        {
            if (_isPeriodDamage)
                ExecutePeriodDamage();

            if (_isPeriodHeal)
                ExecutePeriodHeal();

            return;

            void ExecutePeriodDamage()
            {
                var mag = _periodDamage;
                var value = mag.Value(this);
                if (value > 0)
                {
                    var dmg = new Dmg(value, mag.DmgType, mag.DmgAlgo);
                    Target.Health.Damage(dmg, gameObject, source: Source.Character);
                }
            }

            void ExecutePeriodHeal()
            {
                var mag = _periodHeal;
                var value = mag.Value(this);
                if (value > 0)
                {
                    var heal = new Heal(value, mag.HealAlgo);
                    Target.Health.ReceiveHealth(heal, source: Target.Character);
                }
            }
        }

        void OnPeriod()
        {
            var oldTick = PeriodTick;
            PeriodTick++;
            ExecutePeriodBuff();
            AddPeriodMods(oldTick, PeriodTick);
        }

        void AddPeriodMods(int oldTick, int newTick)
        {
            var mods = _periodMods;
            if (mods == null || mods.Length == 0)
                return;

            foreach (var mod in mods)
            {
                AddOrEditMod(mod, mod.ToString(), oldTick, newTick);
            }
        }

        void RemovePeriodMods(int oldTick, int newTick)
        {
            var mods = _periodMods;
            if (mods == null || mods.Length == 0)
                return;

            foreach (var mod in mods)
            {
                RemoveOrEditMod(mod, mod.ToString(), oldTick, newTick);
            }
        }

        bool RefreshDuration()
        {
            DurationElapsed = 0F;
            return false;
        }

        void ResetPeriod()
        {
            PeriodElapsed = 0F;
            PeriodTick = 0;

            //是否在应用GE时执行周期Tick
            if (_executeOnApply)
            {
                PeriodElapsed = Period;
            }
        }

        void ClearPeriod()
        {
            OverrideDuration = null;
            DurationElapsed = 0F;
            PeriodElapsed = 0F;
            PeriodTick = 0;
        }
    }
}
using System;

namespace MoreMountains
{
    public partial class Buff
    {
        protected Mag duration => main.Duration;
        protected bool isRefreshDurationWhileInCombat => main.RefreshDurationWhileInCombat;

        protected Mag periodDuration => period.Time;
        protected bool isPeriodDamage => period.IsPeriodDamage;
        protected DmgMag periodDamage => period.PeriodDamage;
        protected bool isPeriodHeal => period.IsPeriodHeal;
        protected HealMag periodHeal => period.PeriodHeal;
        protected Mod[] periodMods => period.Mods;
        protected bool isExecuteOnApply => period.ExecuteOnApply;

        public Action OnPeriodDamage { get; set; }
        public bool IsKillByPeriodDamage { get; set; }

        void InitializePeriod()
        {
            DurationElapsed = 0F;
            PeriodElapsed = 0F;
            PeriodTick = 0;

            if (IsInstant)
                return;
            ResetPeriod();
        }

        public virtual float Duration
        {
            get
            {
                if (IsInfinite)
                    return 0F;

                if (IsStackDecreasing && isOverrideDecreasingDuration)
                    return DecreasingDuration;

                return duration.Value(this);
            }
        }

        public float DurationElapsed { get; set; }

        public float DurationLeft => Duration - DurationElapsed;

        public float DurationPct
        {
            get
            {
                var durationLeft = DurationLeft;
                var d = Duration;
                if (durationLeft == 0F)
                    return 0F;
                if (d == 0F)
                    return 0F;
                return durationLeft / d;
            }
        }

        public virtual float Period => IsPeriodic ? periodDuration.Value(this) : 0F;

        public float PeriodElapsed { get; set; }

        public float PeriodLeft => Period - PeriodElapsed;

        public float PeriodPct
        {
            get
            {
                if (PeriodLeft == 0F)
                    return 0F;

                if (Period == 0F)
                    return 0F;

                return PeriodLeft / Period;
            }
        }

        public int PeriodTick { get; set; }

        public bool OnFixedUpdate(float dt, out Removal removal)
        {
            if (IsDuration)
                DurationElapsed += dt;

            if (IsPeriodic)
            {
                //更新周期时间
                PeriodElapsed += dt;
                if (PeriodLeft <= 0)
                {
                    PeriodElapsed = 0;
                    OnPeriod();
                }
            }
            
            //如果是被这一次的PeriodDamage所击杀
            if (IsKillByPeriodDamage)
            {
                removal = Removal.Death;
                return true;
            }

            if (IsInfinite)
            {
                removal = Removal.None;
                return false;
            }

            if (DurationLeft > 0)
            {
                removal = Removal.None;
                return false;
            }

            if (IsStackable)
                return DoStackExpirePolicy(out removal);

            removal = Removal.Routinely;
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
            else if(removal != Removal.Death)
                ApplyPreExpiredBuff();
        }

        void ExecutePeriodBuff()
        {
            if (isPeriodDamage)
                ExecutePeriodDamage();

            if (isPeriodHeal)
                ExecutePeriodHeal();
        }

        protected virtual bool TryGetPeriodDamage(out Dmg dmg)
        {
            var mag = periodDamage;
            var value = mag.Value(this);
            if (value > 0)
            {
                dmg = new Dmg((int)value, mag.DmgType, mag.DmgAlgo);
                return true;
            }

            dmg = default;
            return false;
        }
        
        protected virtual void ExecutePeriodDamage()
        {
            if (TryGetPeriodDamage(out var dmg))
            {
                Target.Health.Damage(ref dmg, gameObject, source: Source.Character);
                IsKillByPeriodDamage = dmg.IsLethal;
            }

            OnPeriodDamage?.Invoke();
        }

        void ExecutePeriodHeal()
        {
            var mag = periodHeal;
            var value = mag.Value(this);
            if (value > 0)
            {
                var heal = new Heal((int)value, mag.HealAlgo);
                Target.Health.ReceiveHealth(heal, source: Target.Character);
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
            var m = periodMods;
            if (m == null || m.Length == 0)
                return;

            foreach (var mod in m)
            {
                AddOrEditMod(mod, mod.ToString(), oldTick, newTick);
            }
        }

        void RemovePeriodMods(int oldTick, int newTick)
        {
            var mods = periodMods;
            if (mods == null || mods.Length == 0)
                return;

            foreach (var mod in mods)
            {
                RemoveOrEditMod(mod, mod.ToString(), oldTick, newTick);
            }
        }

        void RefreshDuration()
        {
            DurationElapsed = 0F;
        }

        void ResetPeriod()
        {
            PeriodElapsed = 0F;
            PeriodTick = 0;

            //是否在应用GE时执行周期Tick
            if (isExecuteOnApply)
            {
                PeriodElapsed = Period;
            }
        }

        void ClearPeriod()
        {
            DurationElapsed = 0F;
            PeriodElapsed = 0F;
            PeriodTick = 0;
        }
    }
}
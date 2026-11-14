using System.Collections.Generic;

namespace MoreMountains
{
    public abstract class ACreature : Character
    {
        public string id { get; set; }
        public abstract bool isPlayer { get; }
        public bool isBloodied { get; set; }

        public virtual int gold { get; set; }
        public int displayGold { get; set; }
        public bool isDying { get; set; }
        public bool isDead { get; set; }
        public bool halfDead { get; set; }

        float healthHideTimer { get; set; }
        public int lastDamageTaken { get; set; }
        public bool inStealth { get; set; }

        public virtual int currentHealth
        {
            get => Health.CurrentHealth;
            set => Health.SetHealth(value, RefreshHealthBarType.Immediately);
        }

        public float currentHealthPct => Health.HealthPct;

        public virtual int maxHealth
        {
            get => Health.MaxHealth;
            set => Health.SetHealth(currentHealth, value, RefreshHealthBarType.Immediately);
        }

        public ABlock block;

        public List<CreaturePower> powers = new();

        protected TopDownController2D _controller2D;

        public TopDownController2D Controller2D
        {
            get
            {
                if (_controller2D == null)
                    TryGetComponent(out _controller2D);

                return _controller2D;
            }
        }

        protected override void Initialization()
        {
            base.Initialization();
            _controller2D = _controller as TopDownController2D;
        }

        #region Damage & Heal

        public virtual void damage(DamageInfo info) { }

        #endregion

        public override void onAcquire()
        {
            base.onAcquire();
        }

        public override void onRelease()
        {
            base.onRelease();

            removeAllPowers();
        }

        #region Events

        public void applyStartOfTurnPowers()
        {
            foreach (var p in powers)
                p.atStartOfTurn();
        }

        public void applyTurnPowers()
        {
            foreach (var p in powers)
                p.duringTurn();
        }

        public void applyStartOfTurnPostDrawPowers()
        {
            foreach (var p in powers)
                p.atStartOfTurnPostDraw();
        }

        public virtual void applyEndOfTurnTriggers() { }

        #endregion

        #region Powers

        protected void fixedUpdatePowers(float dt)
        {
            foreach (var p in powers)
                p.fixedUpdate(dt);
        }
        
        public void updatePowers(float dt)
        {
            foreach (var p in powers)
                p.update(dt);
        }

        public virtual T addPower<T>() where T : CreaturePower
        {
            var power = CLASS<CreaturePower>(typeof(T));
            power.with(this);
            power.onGainPower(this);

            bool hasBuffAlready = false;
            /*foreach (var p in powers)
            {
                if (p.ID == power.ID)
                {
                    p.stackPower(power.amount);
                    p.updateDescription();
                    hasBuffAlready = true;
                }
            }*/

            if (!hasBuffAlready)
            {
                powers.Add(power);
            }

            return power as T;
        }

        public void removeAllPowers()
        {
            for (var i = powers.Count - 1; i >= 0; i--)
            {
                var power = powers[i];
                power.onLosePower(this);
                powers.removeAt(i);
                UN_CLASS(power);
            }
        }

        public bool removePower<T>() where T : CreaturePower
        {
            for (var i = powers.Count - 1; i >= 0; i--)
            {
                if (powers[i] is T power)
                {
                    power.onLosePower(this);
                    powers.removeAt(i);
                    UN_CLASS(power);
                    return true;
                }
            }

            return false;
        }

        public bool removePower<T>(T p) where T : CreaturePower
        {
            for (var i = powers.Count - 1; i >= 0; i--)
            {
                if (powers[i] is T power && power == p)
                {
                    power.onLosePower(this);
                    powers.removeAt(i);
                    UN_CLASS(power);
                    return true;
                }
            }

            return false;
        }
        
        public bool tryGetPower<T>(out T power) where T : CreaturePower
        {
            for (var i = powers.Count - 1; i >= 0; i--)
            {
                if (powers[i] is T t)
                {
                    power = t;
                    return true;
                }
            }

            power = null;
            return false;
        }

        public CreaturePower getPower(string targetID)
        {
            foreach (var p in powers)
            {
                if (p.ID == targetID)
                    return p;
            }

            return null;
        }

        public bool hasPower(string targetID)
        {
            foreach (var p in powers)
            {
                if (p.ID == targetID)
                    return true;
            }

            return false;
        }

        public bool TryGetPower(string targetID, out APower result)
        {
            foreach (var p in powers)
            {
                if (p.ID == targetID)
                {
                    result = p;
                    return true;
                }
            }

            result = null;
            return false;
        }

        #endregion

        #region Gold

        public virtual void loseGold(int amount, PayType type = PayType.DEFAULT)
        {
            if (amount > 0)
            {
                gold = (gold - amount).clampMin(0);
            }
            else
            {
                log("NEGATIVE MONEY???");
            }
        }

        public virtual void gainGold(int amount, EarnType type = EarnType.DEFAULT)
        {
            if (amount > 0)
            {
                gold += amount;
            }
            else
            {
                log("NEGATIVE MONEY???");
            }
        }

        #endregion

        public bool IsDead() => Health.IsDead();
        public bool IsAlive() => Health.IsAlive();
        public virtual bool isDeadOrEscaped() => isDying || halfDead;
    }
}
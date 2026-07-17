using System.Collections.Generic;

namespace MoreMountains
{
    public abstract class ACreature : Character
    {
        public string id;
        public abstract bool isPlayer { get; }
        public bool isBloodied;

        public int gold;
        public int displayGold;
        public bool isDying;
        public bool isDead;
        public bool halfDead;

        float healthHideTimer;
        public int lastDamageTaken;

        public virtual int currentHealth
        {
            get => (int)Health.CurrentHealth;
            set => Health.SetHealth(value, RefreshHealthBarType.Immediately);
        }

        public float currentHealthPct => Health.HealthPct;

        public virtual int maxHealth
        {
            get => (int)Health.MaximumHealth;
            set => Health.SetHealth(currentHealth, value, RefreshHealthBarType.Immediately);
        }

        public ABlock block;

        public List<CreaturePower> powers = new();
        public List<ARelic> relics = new();
        
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

        public virtual void damage(DamageInfo info)
        {
        }



        #endregion

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

        public virtual void applyEndOfTurnTriggers()
        {
        }

        #endregion

        #region Powers

        public void updatePowers(float dt)
        {
            for (int i = 0; i < powers.Count; i++)
                powers[i].update(dt);
        }

        public void addPower(CreaturePower power)
        {
            bool hasBuffAlready = false;
            foreach (var p in powers)
            {
                if (p.ID == power.ID)
                {
                    p.stackPower(power.amount);
                    p.updateDescription();
                    hasBuffAlready = true;
                }
            }

            if (!hasBuffAlready)
            {
                powers.Add(power);
                if (isPlayer)
                {
                    int buffCount = 0;
                    foreach (var p in powers)
                    {
                        if (p.type == PowerType.BUFF)
                            buffCount++;
                    }

                    if (buffCount >= 10)
                        UnlockTracker.unlockAchievement("POWERFUL");
                }
            }
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

        public virtual void loseGold(int amount)
        {
            if (amount > 0)
            {
                gold -= amount;
                if (gold < 0)
                    gold = 0;
            }
            else
            {
                log("NEGATIVE MONEY???");
            }
        }

        public virtual void gainGold(int amount)
        {
            if (amount < 0)
            {
                log("NEGATIVE MONEY???");
            }
            else
            {
                gold += amount;
            }
        }

        #endregion

        public void addRelic(ARelic relic)
        {
            relic.owner = this;
            relics.Add(relic);
        }

        public void setRelic(int slot, ARelic relic)
        {
            relic.owner = this;
            relics[slot] = relic;
        }

        public virtual bool isDeadOrEscaped() => isDying || halfDead;
    }
}
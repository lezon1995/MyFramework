using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(Ball))]
    public class BallHealth : Health
    {
        Ball ball;

        public override void Initialization()
        {
            if (Model)
                Model.SetActive(true);

            TryGetComponent(out ball);

            _initialLayer = gameObject.layer;

            DamageMMFeedbacks.Initialize(gameObject);
            DeathMMFeedbacks.Initialize(gameObject);

            _initialized = true;
            _timeElapsed = 0F;

            DamageEnabled();
        }

        protected override void Start()
        {
        }

        protected override void OnEnable()
        {
            if (IsDead())
                DoResurrect();

            DamageEnabled();
        }

        protected override void GrabAnimator()
        {
        }

        protected override void BindStats()
        {
        }

        protected override void FixedUpdate()
        {
            if (IsDeadTotally)
                return;

            var dt = Time.fixedDeltaTime;
            UpdateCoroutineState(dt);
        }

        protected override void DoResurrect()
        {
            if (DisableChildCollisionsOnDeath)
            {
                _collider2D.enabled = true;
            }
        }

        public override void SetHealth(int curHealth, RefreshHealthBarType type = RefreshHealthBarType.Immediately)
        {
            CurrentHealth = curHealth;
        }

        public override void SetHealth(int curHealth, int maxHealth, RefreshHealthBarType type = RefreshHealthBarType.Immediately)
        {
            CurrentHealth = curHealth;
            MaximumHealth = maxHealth;
        }

        public override bool CanTakeDamageThisFrame(out ResistDamageType type)
        {
            type = ResistDamageType.None;
            return true;
        }

        public override bool ComputeDamageOutput(ref Dmg dmg, IDmgCalculator calculator = null)
        {
            if (Invincible)
                return false;

            if (ImmuneToDamage)
                return false;

            calculator ??= DmgCalculator.Default;

            float damage = dmg.Value;
            float totalDamage = damage;
            float actualDamage = calculator.computeDamageDefence(dmg.ActualType, totalDamage, AR, MR);

            dmg.SetDamageRaw((int)totalDamage);
            dmg.SetDamageDealt((int)actualDamage);
            return actualDamage > 0;
        }

        public override void Damage(ref Dmg dmg, GameObject instigator, Character source = null, float invincibleTime = 0F, Vector3 direction = default, IDmgCalculator calculator = null)
        {
            if (!CanTakeDamageThisFrame(out _))
                return;

            ComputeDamageOutput(ref dmg, calculator);

            if (dmg.DamageDealt > 0)
            {
                // we decrease the character's health by the damage
                float preHealth = CurrentHealth;
                SetHealth(CurrentHealth - dmg.DamageDealt, RefreshHealthBarType.ReceiveDamage);
                LastDamage = dmg.DamageDealt;
                LastDamageType = dmg.ActualType;
                LastDamageDirection = direction;

                if (CurrentHealth <= 0)
                {
                    CurrentHealth = 0;

                    var isLethal = Kill();
                    dmg.IsLethal = isLethal;
                }
            }
        }

        public override bool Kill()
        {
            SetHealth(0, RefreshHealthBarType.Killed);

            ball.setEnabled(false);

            var e = new OnBallDeath(ball);
            e.trigger(ball);
            e.trigger();

            ball.ballRenderer.playFxDead();
            ball.ballRenderer.setRendererActive(false);
            ball.ballRenderer.clearTrail();

            DeathMMFeedbacks.Play(transform.position);

            Event.trigger(new OnDeath());

            // if (DisableModelOnDeath && Model)
            //     Model.SetActive(false);

            // if (DelayBeforeDestruction > 0f)
            // {
            //     _coroutineTimeElapsed = 0F;
            //     _coroutineState = CoroutineState.DestroyObject;
            // }
            // else
                DestroyObject();

            return true;
        }

        protected override void DestroyObject()
        {
            var e = new OnBallDeathTotally(ball);
            e.trigger(ball);

            base.DestroyObject();
        }
    }
}
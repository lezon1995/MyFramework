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

            StoreInitialPosition();
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
        
        public override void SetHealth(float curHealth, RefreshHealthBarType type = RefreshHealthBarType.Immediately)
        {
            CurrentHealth = curHealth;
        }
        
        public override void SetHealth(float curHealth, float maxHealth, RefreshHealthBarType type = RefreshHealthBarType.Immediately)
        {
            CurrentHealth = curHealth;
            MaximumHealth = maxHealth;
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

            if (DisableModelOnDeath && Model)
                Model.SetActive(false);

            if (DelayBeforeDestruction > 0f)
            {
                _coroutineTimeElapsed = 0F;
                _coroutineState = CoroutineState.DestroyObject;
            }
            else
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
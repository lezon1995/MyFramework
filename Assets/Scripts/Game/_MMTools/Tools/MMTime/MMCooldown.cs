using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A class to handle cooldown related properties and their resource consumption over time
    /// Remember to initialize it (once) and update it every frame from another class
    /// </summary>
    [Serializable]
    public class MMCooldown
    {
        public enum States
        {
            Idle,
            Consume,
            Stop,
            Refill
        }

        /// if this is true, the cooldown won't do anything
        public bool Unlimited;

        /// the time it takes, in seconds, to consume the object
        public float ConsumptionDuration;

        /// the pause to apply before refilling once the object's been depleted
        public float PauseOnEmptyDuration;

        /// the duration of the refill, in seconds, if uninterrupted
        public float RefillDuration;

        /// whether or not the refill can be interrupted by a new Start instruction
        public bool CanInterruptRefill = true;

        [ShowInInspector, ReadOnly]
        public States State { get; set; }

        [ShowInInspector, ReadOnly]
        public float DurationLeft { get; set; }

        [ShowInInspector, ReadOnly]
        public float StopElapsed { get; set; }

        public event Action<States> OnStateChange;

        /// <summary>
        /// An init method that ensures the object is reset
        /// </summary>
        public virtual void Initialization()
        {
            DurationLeft = ConsumptionDuration;
            StopElapsed = 0F;
            ChangeState(States.Idle);
        }

        /// <summary>
        /// Starts consuming the cooldown object if possible
        /// </summary>
        public virtual void Start()
        {
            if (Ready())
            {
                ChangeState(States.Consume);
            }
        }

        /// <summary>
        /// Returns true if the cooldown is ready to be consumed, false otherwise
        /// </summary>
        /// <returns></returns>
        public virtual bool Ready()
        {
            if (Unlimited)
                return true;

            return State switch
            {
                States.Idle => true,
                States.Refill when CanInterruptRefill => true,
                _ => false
            };
        }

        public virtual bool NotReady()
        {
            return !Ready();
        }

        /// <summary>
        /// Stops consuming the object 
        /// </summary>
        public virtual void Stop()
        {
            if (State == States.Consume)
            {
                ChangeState(States.Stop);
            }
        }

        public float Progress
        {
            get
            {
                if (Unlimited)
                    return 1F;

                return State switch
                {
                    States.Stop => 0F,
                    States.Consume => 0F,
                    States.Refill => DurationLeft / RefillDuration,
                    _ => 1F
                };
            }
        }

        /// <summary>
        /// Processes the object's state machine
        /// </summary>
        public virtual void Update(float dt = 0F)
        {
            if (Unlimited)
                return;

            if (dt == 0F)
                dt = Time.deltaTime;

            switch (State)
            {
                case States.Idle:
                    break;

                case States.Consume:
                    DurationLeft -= dt;
                    if (DurationLeft <= 0F)
                    {
                        DurationLeft = 0F;
                        StopElapsed = 0F;
                        ChangeState(States.Stop);
                    }

                    break;

                case States.Stop:
                    StopElapsed += dt;
                    if (StopElapsed >= PauseOnEmptyDuration)
                    {
                        ChangeState(States.Refill);
                    }

                    break;
                case States.Refill:
                    DurationLeft += dt;
                    if (DurationLeft >= RefillDuration)
                    {
                        DurationLeft = ConsumptionDuration;
                        ChangeState(States.Idle);
                    }

                    break;
            }
        }

        protected virtual void ChangeState(States state)
        {
            State = state;
            OnStateChange?.Invoke(state);
        }

        static RefPool<MMCooldown> _pool = new();

        public static MMCooldown Get(float duration)
        {
            var cooldown = _pool.Get();
            cooldown.ConsumptionDuration = duration;
            cooldown.Unlimited = false;
            cooldown.PauseOnEmptyDuration = 0;
            cooldown.RefillDuration = 0;
            cooldown.CanInterruptRefill = true;
            cooldown.Initialization();
            cooldown.Start();
            return cooldown;
        }

        public static void Return(MMCooldown o)
        {
            _pool.Return(o);
        }
    }
}
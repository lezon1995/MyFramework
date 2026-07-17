using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// A class used to store the charge properties of the weapons that together make up a charge weapon
    /// Each charge weapon is made of multiple of these, each representing a step in the charge sequence
    /// </summary>
    [Serializable]
    public class ChargeWeaponStep
    {
        [Tooltip("the weapon to cause an attack with at that step")]
        public Weapon TargetWeapon;

        [Tooltip("the duration (in seconds) it should take to keep the charge going to the next step")]
        public float ChargeDuration = 1f;

        [Tooltip("if the charge is interrupted at this step, whether or not to trigger this weapon's attack")]
        public bool TriggerIfChargeInterrupted = true;

        [Tooltip("if this is true, the weapon at this step will be flipped when the charge weapon flips")]
        public bool FlipWhenChargeWeaponFlips = true;

        [Tooltip("a feedback to trigger when this step starts charging")]
        public MMFeedbacks ChargeStartFeedbacks;

        [Tooltip("a feedback to trigger when this step gets interrupted (when the charge is dropped at this step)")]
        public MMFeedbacks ChargeInterruptedFeedbacks;

        [Tooltip("a feedback to trigger when this step completes and the charge potentially moves on to the next step")]
        public MMFeedbacks ChargeCompleteFeedbacks;

        /// the total time (in seconds) from the complete start of the charge weapon to this weapon's charge being complete
        public virtual float TotalDuration { get; set; }

        /// whether this step's charge has started or not
        public virtual bool IsStarted { get; set; }

        /// whether this step's charge has completed or not
        public virtual bool IsCompleted { get; set; }
    }

    /// <summary>
    /// Add this component to an object, and it'll let you define a sequence of charge steps, each triggering their own unique weapon, complete with options like input modes or conditional releases, hooks for every step, and more. Useful for Megaman or Zelda like types of charge weapons.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/ChargeWeapon")]
    public class ChargeWeapon : Weapon
    {
        /// the possible timescales for this weapon
        public enum TimescaleModes
        {
            Scaled,
            Unscaled
        }

        /// whether the charge should be released on input release, or after the last charge duration
        public enum ReleaseModes
        {
            OnInputRelease,
            AfterLastChargeDuration
        }

        /// the current delta time value
        public virtual float DeltaTime => TimescaleMode == TimescaleModes.Scaled ? Time.deltaTime : Time.unscaledDeltaTime;

        /// the current time value
        public virtual float CurrentTime => TimescaleMode == TimescaleModes.Scaled ? Time.time : Time.unscaledTime;

        [MMInspectorGroup("Charge Weapon")]
        [Header("List of Weapons in the Charge Sequence")]
        [Tooltip("the list of weapons that make up this charge weapon's sequence of steps")]
        public List<ChargeWeaponStep> Weapons;

        [Header("Settings")]
        [Tooltip("whether this weapon should trigger its attack when all steps are done charging, or when input gets released")]
        public ReleaseModes ReleaseMode = ReleaseModes.OnInputRelease;

        [Tooltip("whether this weapon's input should run on scaled or unscaled time")]
        public TimescaleModes TimescaleMode = TimescaleModes.Scaled;

        [Tooltip("whether or not the start of the charge should trigger the first step's weapon's attack or not")]
        public bool AllowInitialShot = true;

        [Title("Debug")]
        [Tooltip("the current charge index in the Weapons step list")]
        [ShowInInspector, ReadOnly]
        public int Index { get; set; }

        [Tooltip("whether this weapon is currently charging or not")]
        [ShowInInspector, ReadOnly]
        public bool IsCharging { get; set; }

        protected float _chargingStartedAt;
        protected int _lastIndex;
        protected int _initialIndex;

        /// <summary>
        /// On init, we initialize our durations, weapons and reset the charge
        /// </summary>
        public override void Initialization()
        {
            base.Initialization();
            InitializeTotalDurations();
            InitializeWeapons();
            ResetCharge();
        }

        /// <summary>
        /// goes through all weapons to set up their total duration (the time from start after which their step is complete)
        /// </summary>
        public virtual void InitializeTotalDurations()
        {
            var total = 0F;
            var delay = delayBeforeUse;
            if (delay > 0)
            {
                total += delay;
                Index = -1;
            }

            foreach (var step in Weapons)
            {
                total += step.ChargeDuration;
                step.TotalDuration = total;
            }

            _lastIndex = Index;
            _initialIndex = Index;
        }

        /// <summary>
        /// resets the charge, reinitializing all counters 
        /// </summary>
        public virtual void ResetCharge()
        {
            IsCharging = false;
            Index = _initialIndex;
            foreach (var step in Weapons)
            {
                step.IsStarted = false;
                step.IsCompleted = false;
            }
        }

        /// <summary>
        /// Initializes all weapons for all steps
        /// </summary>
        protected virtual void InitializeWeapons()
        {
            foreach (var step in Weapons)
            {
                step.TargetWeapon.SetOwner(Owner, HandleWeapon);
                step.TargetWeapon.Initialization();
                step.TargetWeapon.InitializeAnimatorParameters();
            }
        }

        /// <summary>
        /// On update, if we're charging, we process our charge to evaluate the current step
        /// </summary>
        protected override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            ProcessCharge();
        }

        /// <summary>
        /// Determines the current step, and if it's different from the last frame, starts the new step
        /// </summary>
        protected virtual void ProcessCharge()
        {
            if (!IsCharging)
                return;

            Index = FindCurrentWeaponIndex();

            if (Index != _lastIndex)
            {
                CompleteStepCharge(_lastIndex);
                StartStepCharge(Index);
            }

            if (ReleaseMode == ReleaseModes.AfterLastChargeDuration && Index == Weapons.Count - 1)
            {
                StopChargeSequence();
            }

            _lastIndex = Index;
        }

        /// <summary>
        /// Initializes the charge sequence
        /// </summary>
        protected virtual void StartChargeSequence()
        {
            IsCharging = true;
            _chargingStartedAt = CurrentTime;
            if (WeaponExists(Index))
            {
                StartStepCharge(Index);
                if (AllowInitialShot)
                {
                    ForceWeaponAttack(0);
                }
            }
        }

        /// <summary>
        /// Causes a step to start charging
        /// </summary>
        /// <param name="index"></param>
        protected virtual void StartStepCharge(int index)
        {
            if (!WeaponExists(index))
                return;

            Weapons[index].IsStarted = true;
            Weapons[index].ChargeStartFeedbacks.Play();
        }

        /// <summary>
        /// Stops a step charge
        /// </summary>
        /// <param name="index"></param>
        protected virtual void InterruptStepCharge(int index)
        {
            if (!WeaponExists(index))
                return;

            Weapons[index].ChargeStartFeedbacks.Stop();
            Weapons[index].ChargeInterruptedFeedbacks.Play();
        }

        /// <summary>
        /// Completes a step charge
        /// </summary>
        /// <param name="index"></param>
        protected virtual void CompleteStepCharge(int index)
        {
            if (!WeaponExists(index))
                return;

            Weapons[index].ChargeStartFeedbacks.Stop();
            Weapons[index].IsCompleted = true;
            Weapons[index].ChargeCompleteFeedbacks.Play();
        }

        /// <summary>
        /// Stops the entire charge sequence, triggering the appropriate feedbacks
        /// </summary>
        protected virtual void StopChargeSequence()
        {
            if (!IsCharging)
                return;

            if (Index >= 0 || !AllowInitialShot)
            {
                bool shouldAttack = true;
                if (Index < Weapons.Count - 1 && !Weapons[Index].IsCompleted)
                {
                    if (!Weapons[Index].TriggerIfChargeInterrupted)
                    {
                        shouldAttack = false;
                    }
                }

                if (shouldAttack)
                {
                    Weapons[Index].ChargeStartFeedbacks.Stop();
                    Weapons[Index].ChargeCompleteFeedbacks.Stop();
                    if (WeaponExists(Index - 1))
                    {
                        Weapons[Index - 1].ChargeStartFeedbacks.Stop();
                        Weapons[Index - 1].ChargeCompleteFeedbacks.Stop();
                    }

                    ForceWeaponAttack(Index);
                }
            }

            if (!Weapons[Index].IsCompleted)
            {
                InterruptStepCharge(Index);
            }

            ResetCharge();
        }

        /// <summary>
        /// Forces the weapon at the specified step to turn on
        /// </summary>
        /// <param name="index"></param>
        protected virtual void ForceWeaponAttack(int index)
        {
            Weapons[index].TargetWeapon.TurnWeaponOn();
        }

        /// <summary>
        /// Returns the index of the current weapon in the charge sequence
        /// </summary>
        /// <returns></returns>
        protected virtual int FindCurrentWeaponIndex()
        {
            float elapsedTime = CurrentTime - _chargingStartedAt;

            if (elapsedTime < delayBeforeUse)
                return -1;

            for (int i = 0; i < Weapons.Count; i++)
            {
                if (Weapons[i].TotalDuration > elapsedTime)
                    return i;
            }

            return Weapons.Count - 1;
        }

        /// <summary>
        /// Returns true if the weapon at the specified index exists
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual bool WeaponExists(int index)
        {
            return 0 <= index && index < Weapons.Count;
        }

        /// <summary>
        /// When the charge weapon gets activated, we start charging
        /// </summary>
        public override void TurnWeaponOn()
        {
            base.TurnWeaponOn();
            StartChargeSequence();
        }

        /// <summary>
        /// When the charge weapon's input gets released, we stop charging
        /// </summary>
        public override void WeaponInputReleased()
        {
            base.WeaponInputReleased();
            StopChargeSequence();
        }

        public override void FlipWeapon()
        {
            base.FlipWeapon();
            for (var i = 0; i < Weapons.Count; i++)
            {
                if (Weapons[i].FlipWhenChargeWeaponFlips)
                {
                    Weapons[i].TargetWeapon.Flipped = Flipped;
                }
            }
        }
    }
}
using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Used by the DamageResistanceProcessor, this class defines the resistance versus a certain type of damage. 
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Health/Damage Resistance")]
    public class DamageResistance : TopDownMonoBehaviour
    {
        public enum DamageModes
        {
            Multiplier,
            Flat
        }

        public enum KnockbackModes
        {
            Multiplier,
            Flat
        }

        [Header("General")]
        [Tooltip("The priority of this damage resistance. This will be used to determine in what order damage resistances should be evaluated. Lowest priority means evaluated first.")]
        public float Priority;

        [Tooltip("The label of this damage resistance. Used for organization, and to activate/disactivate a resistance by its label.")]
        public string Label = "";

        [Header("Damage Resistance Settings")]
        [Tooltip("Whether this resistance impacts base damage or typed damage")]
        public DamageType.Modes DamageTypeMode = DamageType.Modes.Base;

        [Tooltip("In TypedDamage mode, the type of damage this resistance will interact with")]
        [MMEnumCondition(nameof(DamageTypeMode), (int)DamageType.Modes.Typed)]
        public DamageType TypeResistance;

        [Tooltip("the way to reduce (or increase) received damage. Multiplier will multiply incoming damage by a multiplier, flat will subtract a constant value from incoming damage.")]
        public DamageModes DamageModifierMode = DamageModes.Multiplier;

        [Header("Damage Modifiers")]
        [Tooltip("In multiplier mode, the multiplier to apply to incoming damage. 0.5 will reduce it in half, while a value of 2 will create a weakness to the specified damage type, and damages will double.")]
        [MMEnumCondition(nameof(DamageModifierMode), (int)DamageModes.Multiplier)]
        public float DamageMultiplier = 0.25f;

        [Tooltip("In flat mode, the amount of damage to subtract every time that type of damage is received")]
        [MMEnumCondition(nameof(DamageModifierMode), (int)DamageModes.Flat)]
        public float FlatDamageReduction = 10f;

        [Tooltip("whether or not incoming damage of the specified type should be clamped between a min and a max")]
        public bool ClampDamage;

        [Tooltip("the values between which to clamp incoming damage")]
        [MMVector("Min", "Max")]
        public Vector2 DamageModifierClamps = new Vector2(0f, 10f);

        [Header("Condition Change")]
        [Tooltip("whether or not condition change for that type of damage is allowed or not")]
        public bool PreventCharacterConditionChange;

        [Tooltip("whether or not movement modifiers are allowed for that type of damage or not")]
        public bool PreventMovementModifier;

        [Header("Knockback")]
        [Tooltip("if this is true, knockback force will be ignored and not applied")]
        public bool ImmuneToKnockback;

        [Tooltip("the way to reduce (or increase) received knockback. Multiplier will multiply incoming knockback intensity by a multiplier, flat will subtract a constant value from incoming knockback intensity.")]
        public KnockbackModes KnockbackModifierMode = KnockbackModes.Multiplier;

        [Tooltip("In multiplier mode, the multiplier to apply to incoming knockback. 0.5 will reduce it in half, while a value of 2 will create a weakness to the specified damage type, and knockback intensity will double.")]
        [MMEnumCondition(nameof(KnockbackModifierMode), (int)DamageModes.Multiplier)]
        public float KnockbackMultiplier = 1f;

        [Tooltip("In flat mode, the amount of knockback to subtract every time that type of damage is received")]
        [MMEnumCondition(nameof(KnockbackModifierMode), (int)DamageModes.Flat)]
        public float FlatKnockbackMagnitudeReduction = 10f;

        [Tooltip("whether or not incoming knockback of the specified type should be clamped between a min and a max")]
        public bool ClampKnockback;

        [Tooltip("the values between which to clamp incoming knockback magnitude")]
        [MMCondition("ClampKnockback", true)]
        public float KnockbackMaxMagnitude = 10f;

        [Header("Feedbacks")]
        [Tooltip("This feedback will only be triggered if damage of the matching type is received")]
        public MMFeedbacks OnDamageReceived;

        [Tooltip("whether or not this feedback can be interrupted (stopped) when that type of damage is interrupted")]
        public bool InterruptibleFeedback;

        [Tooltip("if this is true, the feedback will always be preventively stopped before playing")]
        public bool AlwaysInterruptFeedbackBeforePlay;

        [Tooltip("whether this feedback should play if damage received is zero")]
        public bool TriggerFeedbackIfDamageIsZero;

        /// <summary>
        /// On awake we initialize our feedback
        /// </summary>
        protected virtual void Awake()
        {
            OnDamageReceived.Initialize(gameObject);
        }

        /// <summary>
        /// When getting damage, goes through damage reduction and outputs the resulting damage
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="type"></param>
        /// <param name="damageApplied"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual float ProcessDamage(float damage, DamageType type, bool damageApplied)
        {
            if (!gameObject.activeInHierarchy)
                return damage;

            if (type == null && DamageTypeMode != DamageType.Modes.Base)
                return damage;

            if (type != null && DamageTypeMode == DamageType.Modes.Base)
                return damage;

            if (type != null && type != TypeResistance)
                return damage;

            // applies damage modifier or reduction
            switch (DamageModifierMode)
            {
                case DamageModes.Multiplier:
                    damage *= DamageMultiplier;
                    break;
                case DamageModes.Flat:
                    damage -= FlatDamageReduction;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // clamps damage
            damage = ClampDamage ? Mathf.Clamp(damage, DamageModifierClamps.x, DamageModifierClamps.y) : damage;

            if (damageApplied)
            {
                if (!TriggerFeedbackIfDamageIsZero && damage == 0)
                {
                    // do nothing
                }
                else
                {
                    if (AlwaysInterruptFeedbackBeforePlay)
                    {
                        OnDamageReceived.Stop();
                    }

                    OnDamageReceived.Play(transform.position);
                }
            }

            return damage;
        }

        /// <summary>
        /// Processes the knockback input value and returns it potentially modified by damage resistances
        /// </summary>
        /// <param name="knockback"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual Vector3 ProcessKnockback(Vector3 knockback, DamageType type)
        {
            if (!gameObject.activeInHierarchy)
                return knockback;

            if (type == null && DamageTypeMode != DamageType.Modes.Base)
                return knockback;

            if (type != null && DamageTypeMode == DamageType.Modes.Base)
                return knockback;

            if (type != null && type != TypeResistance)
                return knockback;

            // applies damage modifier or reduction
            switch (KnockbackModifierMode)
            {
                case KnockbackModes.Multiplier:
                    knockback *= KnockbackMultiplier;
                    break;
                case KnockbackModes.Flat:
                    float magnitudeReduction = Mathf.Clamp(Mathf.Abs(knockback.magnitude) - FlatKnockbackMagnitudeReduction, 0f, float.MaxValue);
                    knockback = knockback.normalized * (magnitudeReduction * Mathf.Sign(knockback.magnitude));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // clamps damage
            knockback = ClampKnockback ? Vector3.ClampMagnitude(knockback, KnockbackMaxMagnitude) : knockback;

            return knockback;
        }
    }
}
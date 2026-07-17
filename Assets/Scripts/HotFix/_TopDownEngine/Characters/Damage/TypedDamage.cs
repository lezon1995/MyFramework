using System;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains
{
    /// <summary>
    /// A class used to store and define typed damage impact : damage caused, condition or movement speed changes, etc
    /// </summary>
    [Serializable]
    public class TypedDamage
    {
        [Tooltip("the type of damage associated to this definition")]
        public DamageType AssociatedDamageType;

        public float MinDamageCaused = 10f;
        public float MaxDamageCaused = 10f;

        [Tooltip("whether or not this damage, when applied, should force the character into a specified condition")]
        public bool ForceCharacterCondition;

        [Tooltip("when in forced character condition mode, the condition to which to swap")]
        [MMCondition(nameof(ForceCharacterCondition), true)]
        public Character.Conditions ForcedCondition;

        [Tooltip("when in forced character condition mode, whether or not to disable gravity")]
        [MMCondition(nameof(ForceCharacterCondition), true)]
        public bool DisableGravity;

        [Tooltip("when in forced character condition mode, whether or not to reset controller forces")]
        [MMCondition(nameof(ForceCharacterCondition), true)]
        public bool ResetControllerForces;

        [Tooltip("when in forced character condition mode, the duration of the effect, after which condition will be reverted")]
        [MMCondition(nameof(ForceCharacterCondition), true)]
        public float ForcedConditionDuration = 3f;

        [Tooltip("whether or not to apply a movement multiplier to the damaged character")]
        public bool ApplyMovementMultiplier;

        [Tooltip("the movement multiplier to apply when ApplyMovementMultiplier is true")]
        [MMCondition(nameof(ApplyMovementMultiplier), true)]
        public float MovementMultiplier = 0.5f;

        [Tooltip("the duration of the movement multiplier, if ApplyMovementMultiplier is true")]
        [MMCondition(nameof(ApplyMovementMultiplier), true)]
        public float MovementMultiplierDuration = 2f;

        protected int _lastRandomFrame = -1000;
        protected float _lastRandomValue;

        public virtual float DamageCaused
        {
            get
            {
                if (Time.frameCount != _lastRandomFrame)
                {
                    _lastRandomValue = Random.Range(MinDamageCaused, MaxDamageCaused);
                    _lastRandomFrame = Time.frameCount;
                }

                return _lastRandomValue;
            }
        }
    }
}
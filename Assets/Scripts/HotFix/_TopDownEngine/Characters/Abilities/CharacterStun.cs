using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Add this component to a character and it'll be able to be stunned. To stun a character, simply call its Stun or StunFor methods. You'll find test buttons at the bottom of this component's inspector. You can also use StunZones to stun your characters.
    /// Animator parameters : Stunned (bool)
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/CharacterStun")]
    public class CharacterStun : CharacterAbility
    {
        public override string HelpBoxText()
        {
            return "Add this component to a character and it'll be able to be stunned. To stun a character, simply call its Stun or StunFor methods. You'll find test buttons at the bottom of this component's inspector. You can also use StunZones to stun your characters.";
        }

        [Header("IK")]
        [Tooltip("a weapon IK to pilot when stunned")]
        public WeaponIK BoundWeaponIK;

        [Tooltip("whether or not to detach the left hand of the character from IK when stunned")]
        public bool DetachLeftHand;

        [Tooltip("whether or not to detach the right hand of the character from IK when stunned")]
        public bool DetachRightHand;

        [Header("Weapon Models")]
        [Tooltip("whether or not to disable the weapon model when stunned")]
        public bool DisableAimWeaponModelAtTargetDuringStun;

        [Tooltip("the list of weapon models to disable when stunned")]
        public List<WeaponModel> WeaponModels;

        [Header("Tests")]
        [MMInspectorButton("Stun")]
        public bool StunButton;

        [MMInspectorButton("ExitStun")]
        public bool ExitStunButton;

        protected const string _stunnedAnimationParameterName = "Stunned";
        protected int _stunnedAnimationParameter;
        protected CoroutineHandle _stunCoroutine;
        protected Character.Conditions _previousCondition;

        /// <summary>
        /// Stuns the character
        /// </summary>
        public virtual void Stun()
        {
            var state = Character.Conditions.Stunned;
            if (_previousCondition != state && _conditionState.Not(state))
            {
                _previousCondition = _conditionState.CurrentState;
            }

            _conditionState.ChangeState(state);
            _controller.SetMovement(Vector3.zero);
            AbilityStartFeedbacks.Play();
            DetachIK();
        }

        /// <summary>
        /// Stuns the character for the specified duration
        /// </summary>
        /// <param name="duration"></param>
        public virtual void StunFor(float duration)
        {
            if (_stunCoroutine != default)
            {
                Timing.KillCoroutines(_stunCoroutine);
            }

            _stunCoroutine = Timing.RunCoroutine(StunCoroutine(duration));
        }

        /// <summary>
        /// Exits stun, resetting condition to the previous one
        /// </summary>
        public virtual void ExitStun()
        {
            if (_conditionState.Not(Character.Conditions.Stunned))
                return;

            AbilityStopFeedbacks.Play();
            _conditionState.ChangeState(_previousCondition);
            AttachIK();
        }

        /// <summary>
        /// Stuns the character, waits for the specified duration, then exits stun
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected virtual IEnumerator<float> StunCoroutine(float duration)
        {
            Stun();
            yield return Timing.WaitForSeconds(duration);
            ExitStun();
        }

        /// <summary>
        /// Detaches IK
        /// </summary>
        protected virtual void DetachIK()
        {
            if (DetachLeftHand)
            {
                BoundWeaponIK.AttachLeftHand = false;
            }

            if (DetachRightHand)
            {
                BoundWeaponIK.AttachRightHand = false;
            }

            if (DisableAimWeaponModelAtTargetDuringStun)
            {
                foreach (WeaponModel model in WeaponModels)
                {
                    model.AimWeaponModelAtTarget = false;
                }
            }
        }

        /// <summary>
        /// Attaches IK
        /// </summary>
        protected virtual void AttachIK()
        {
            if (DetachLeftHand)
            {
                BoundWeaponIK.AttachLeftHand = true;
            }

            if (DetachRightHand)
            {
                BoundWeaponIK.AttachRightHand = true;
            }

            if (DisableAimWeaponModelAtTargetDuringStun)
            {
                foreach (WeaponModel model in WeaponModels)
                {
                    model.AimWeaponModelAtTarget = true;
                }
            }
        }

        /// <summary>
        /// Adds required animator parameters to the animator parameters list if they exist
        /// </summary>
        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter(_stunnedAnimationParameterName, AnimatorControllerParameterType.Bool, out _stunnedAnimationParameter);
        }

        /// <summary>
        /// At the end of each cycle, we send our Running status to the character's animator
        /// </summary>
        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _stunnedAnimationParameter, _conditionState.Is(Character.Conditions.Stunned), _character.AnimatorParameters, _character.RunAnimatorSanityChecks);
        }
    }
}
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Add this ability to a character, and it'll be able to dash in 2D, covering a certain distance in a certain duration
    ///
    /// Animation parameters :
    /// Dashing : true if the character is currently dashing
    /// DashingDirectionX : the x component of the dash direction, normalized
    /// DashingDirectionY : the y component of the dash direction, normalized
    /// </summary>
    public class PlayerDash2D : PlayerAbility
    {
        /// the possible dash modes (fixed : always the same direction)
        public enum DashModes
        {
            Fixed,
            MainMovement,
            SecondaryMovement,
            MousePosition
        }

        /// the possible spaces the dash should happen in, either in world coordinates or local ones
        public enum DashSpaces
        {
            World,
            Local
        }

        [Tooltip("the dash mode to apply the dash in")]
        public DashModes DashMode = DashModes.MainMovement;

        [Header("Dash")]
        public DashSpaces DashSpace = DashSpaces.World;

        [Tooltip("the dash direction")]
        public Vector3 DashDirection = Vector3.forward;

        [Tooltip("the distance the dash should last for")]
        public float DashDistance = 6f;

        [Tooltip("the duration of the dash, in seconds")]
        public float DashDuration = 0.2f;

        [Tooltip("the animation curve to apply to the dash acceleration")]
        public AnimationCurve DashCurve = new(new(0f, 0f), new(1f, 1f));

        [Header("Cooldown")]
        [Tooltip("this ability's cooldown")]
        public MMCooldown Cooldown;

        [Header("Damage")]
        [Tooltip("if this is true, this character won't receive any damage while a dash is in progress")]
        public bool InvincibleWhileDashing;

        [Header("Feedback")]
        [Tooltip("the feedbacks to play when dashing")]
        public MMFeedbacks DashFeedback;

        [Header("Damage Dash")]
        [Tooltip("the DamageOnTouch object to activate when dashing (usually placed under the Character's model, will require a Collider2D of some form, set to trigger")]
        public DamageOnTouch TargetDamageOnTouch;


        protected bool _dashing;
        protected float _dashTimer;
        protected Vector3 _dashOrigin;
        protected Vector3 _dashDestination;
        protected Vector3 _newPosition;
        protected Vector3 _oldPosition;
        protected Camera _mainCamera;

        /// <summary>
        /// On init, we stop our particles, and initialize our dash bar
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();
            Cooldown.Initialization();

            _mainCamera = Camera.main;
            TargetDamageOnTouch.SetActive(false);
        }

        /// <summary>
        /// Watches for dash inputs
        /// </summary>
        protected override void HandleInput()
        {
            if (AbilityUnauthorized)
                return;

            if (Cooldown.NotReady())
                return;

            if (_conditionState.Not(Character.Conditions.Normal))
                return;

            if (_inputManager.DashButton.IsDown())
            {
                DashStart();
            }
        }

        /// <summary>
        /// Initiates the dash
        /// </summary>
        public virtual void DashStart()
        {
            if (Cooldown.NotReady())
                return;

            Cooldown.Start();
            _motionState.ChangeState(Character.Motions.Dashing);
            _dashing = true;
            _dashTimer = 0f;
            _dashOrigin = transform.position;
            // _controller.FreeMovement = false;
            DashFeedback.Play(transform.position);
            PlayAbilityStartFeedbacks();

            if (InvincibleWhileDashing)
            {
                _health.DamageDisabled();
            }

            HandleDashMode();
            _character.Event.trigger(new DoDash());

            TargetDamageOnTouch.SetActive(true);
        }

        protected virtual void HandleDashMode()
        {
            var dashDistance = DashDistance;
            Vector3 dashDestination = Vector3.zero;
            switch (DashMode)
            {
                case DashModes.Fixed:
                    dashDestination = transform.position + DashDirection.normalized * dashDistance;
                    break;

                case DashModes.MainMovement:
                    dashDestination = transform.position + _controller.CurrentDirection.normalized * dashDistance;
                    break;

                case DashModes.SecondaryMovement:
                    dashDestination = transform.position + (Vector3)_inputManager.SecondaryMovement.normalized * dashDistance;
                    break;

                case DashModes.MousePosition:
                    var position = _mainCamera.ScreenToWorldPoint(_inputManager.MousePosition);
                    position.z = transform.position.z;
                    dashDestination = transform.position + (position - transform.position).normalized * dashDistance;
                    break;
            }

            _dashDestination = dashDestination;
        }

        /// <summary>
        /// Stops the dash
        /// </summary>
        public virtual void DashStop()
        {
            DashFeedback.Stop(transform.position);

            StopStartFeedbacks();
            PlayAbilityStopFeedbacks();

            if (InvincibleWhileDashing)
            {
                _health.DamageEnabled();
            }

            _motionState.ChangeState(Character.Motions.Idle);
            _dashing = false;
            // _controller.FreeMovement = true;

            TargetDamageOnTouch.SetActive(false);
        }

        /// <summary>
        /// On update, moves the character if needed
        /// </summary>
        public override void OnFixedUpdate(float dt)
        {
            Cooldown.Update(dt);
            UpdateDashBar();

            if (_dashing)
            {
                if (_dashTimer < DashDuration)
                {
                    var f = DashCurve.Evaluate(_dashTimer / DashDuration);
                    _dashTimer += dt;
                    switch (DashSpace)
                    {
                        case DashSpaces.World:
                            _newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, f);
                            _controller.MovePosition(_newPosition);
                            break;
                        case DashSpaces.Local:
                            _oldPosition = _dashTimer == 0 ? _dashOrigin : _newPosition;
                            _newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, f);
                            _controller.MovePosition(transform.position + _newPosition - _oldPosition);
                            break;
                    }
                }
                else
                {
                    DashStop();
                }
            }
        }

        /// <summary>
        /// Updates the GUI jetpack bar.
        /// </summary>
        protected virtual void UpdateDashBar()
        {
            // if (GUIManager.HasInstance && _character.CharacterType == Character.Types.Player)
            // {
            //     GUIManager.Instance.UpdateDashBars(Cooldown.DurationLeft, 0f, Cooldown.ConsumptionDuration, _character.PlayerID);
            // }
        }

        /// <summary>
        /// Adds required animator parameters to the animator parameters list if they exist
        /// </summary>
        protected override void InitializeAnimatorParameters()
        {
        }

        /// <summary>
        /// At the end of each cycle, we send our Running status to the character's animator
        /// </summary>
        public override void UpdateAnimator()
        {
        }
    }
}
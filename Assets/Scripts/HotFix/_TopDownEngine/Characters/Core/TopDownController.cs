using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Do not use this class directly, use TopDownController2D for 2D characters, or TopDownController3D for 3D characters
    /// Both of these classes inherit from this one
    /// </summary>
    public abstract class TopDownController : TopDownMonoBehaviour
    {
        public bool IsPlayer;
        public Character Character;
        public Vector3 Velocity { get; set; }
        public bool Grounded { get; set; }
        public bool JustGotGrounded { get; set; }
        public Vector3 CurrentMovement { get; set; }
        public Vector3 CurrentDirection { get; set; }

        /// the surface modifier object below our character (if any)
        // public virtual SurfaceModifier SurfaceModifierBelow { get; set; }
        public virtual Vector3 AppliedImpact => _impact;

        // public virtual bool OnMovingPlatform { get; set; }
        public virtual Vector3 MovingPlatformSpeed { get; set; }

        protected Vector3 _lastPosition { get; set; }
        protected bool _groundedLastFrame;
        protected Vector3 _impact;

        protected virtual void Awake()
        {
            Character = GetComponent<Character>();
            CurrentDirection = transform.forward;
            _lastPosition = transform.position;
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        /// <summary>
        /// On update, we check if we're grounded, and determine the direction
        /// </summary>
        protected virtual void Update()
        {
            CheckIfGrounded();
            DetermineDirection();
        }

        protected abstract void DetermineDirection();

        protected void CheckDeltaMovement()
        {
            var movement = transform.position - _lastPosition;
            if (movement == Vector3.zero)
                return;

            if (Character)
            {
                Character.Event.trigger(new DoMove(movement));
            }
        }

        /// <summary>
        /// Called at FixedUpdate
        /// </summary>
        protected virtual void FixedUpdate()
        {
            CheckDeltaMovement();
        }

        /// <summary>
        /// On LateUpdate, computes the speed of the agent
        /// </summary>
        protected virtual void LateUpdate()
        {
        }

        /// <summary>
        /// Checks if the character is grounded
        /// </summary>
        protected virtual void CheckIfGrounded()
        {
            JustGotGrounded = !_groundedLastFrame && Grounded;
            _groundedLastFrame = Grounded;
        }

        /// <summary>
        /// Use this to apply an impact to a controller, moving it in the specified direction at the specified force
        /// </summary>
        public abstract void AddImpact(Vector3 direction, float force);

        /// <summary>
        /// Sets gravity active or inactive
        /// </summary>
        public virtual void SetGravityActive(bool status)
        {
        }

        /// <summary>
        /// Adds the specified force to the controller
        /// </summary>
        public abstract void AddForce(Vector3 movement);

        /// <summary>
        /// Sets the current movement of the controller to the specified Vector3
        /// </summary>
        public abstract void SetMovement(Vector3 movement);

        /// <summary>
        /// Moves the controller to the specified position (in world space)
        /// </summary>
        public abstract void MovePosition(Vector3 newPosition);

        /// <summary>
        /// Set the controller to the specified position (in world space)
        /// </summary>
        public abstract void SetPosition(Vector3 newPosition);

        /// <summary>
        /// Resizes the controller's collider
        /// </summary>
        public abstract void ResizeColliderHeight(float newHeight, bool translateCenter = false);

        /// <summary>
        /// Resets the controller's collider size
        /// </summary>
        public abstract void ResetColliderSize();

        /// <summary>
        /// Returns true if the controller's collider can go back to original size without hitting an obstacle, false otherwise
        /// </summary>
        public virtual bool CanGoBackToOriginalSize()
        {
            return true;
        }

        /// <summary>
        /// Turns the controller's collisions on
        /// </summary>
        public abstract void CollisionsOn();

        /// <summary>
        /// Turns the controller's collisions off
        /// </summary>
        public abstract void CollisionsOff();

        /// <summary>
        /// Sets the controller's rigidbody to Kinematic (or not kinematic)
        /// </summary>
        /// <param name="state"></param>
        public abstract void SetKinematic(bool state);


        /// <summary>
        /// Resets all values for this controller
        /// </summary>
        public virtual void Reset()
        {
            _impact = Vector3.zero;
            Velocity = Vector3.zero;
            Grounded = true;
            JustGotGrounded = false;
            CurrentMovement = Vector3.zero;
            CurrentDirection = Vector3.zero;
        }
    }
}
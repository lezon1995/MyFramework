using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Do not use this class directly, use TopDownController2D for 2D characters, or TopDownController3D for 3D characters
    /// Both of these classes inherit from this one
    /// </summary>
    public abstract class TopDownController : TopDownMonoBehaviour
    {
        public Character Character;

        [Header("Gravity")]
        [Tooltip("the current gravity to apply to our character (positive goes down, negative goes up, higher value, higher acceleration)")]
        public float Gravity = 40f;

        [Tooltip("whether or not the gravity is currently being applied to this character")]
        public bool GravityActive = true;

        [Header("General Raycasts")]
        [Tooltip("by default, the length of the raycasts used to get back to normal size will be auto generated based on your character's normal/standing height, but here you can specify a different value")]
        public float CrouchedRaycastLengthMultiplier = 1f;

        [Tooltip("if this is true, extra raycasts will be cast on all 4 sides to detect obstacles and feed the CollidingWithCardinalObstacle bool, only useful when working with grid movement, or if you need that info for some reason")]
        public bool PerformCardinalObstacleRaycastDetection;

        [ShowInInspector, ReadOnly]
        [Tooltip("the current speed of the character")]
        public Vector3 Speed { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("the current velocity in units/second")]
        public Vector3 Velocity { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("the velocity of the character last frame")]
        public Vector3 VelocityLastFrame { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("the current acceleration")]
        public Vector3 Acceleration { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("whether or not the character is grounded")]
        public bool Grounded { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("whether or not the character got grounded this frame")]
        public bool JustGotGrounded { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("the current movement of the character")]
        public Vector3 CurrentMovement { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("the direction the character is going in")]
        public Vector3 CurrentDirection { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("the current friction")]
        public float Friction { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("the current added force, to be added to the character's movement")]
        public Vector3 AddedForce { get; set; }

        [ShowInInspector, ReadOnly]
        [Tooltip("whether or not the character is in free movement mode or not")]
        public bool FreeMovement { get; set; } = true;

        /// the collider's center coordinates
        public virtual Vector3 ColliderCenter => Vector3.zero;

        /// the collider's bottom coordinates
        public virtual Vector3 ColliderBottom => Vector3.zero;

        /// the collider's top coordinates
        public virtual Vector3 ColliderTop => Vector3.zero;

        /// the object (if any) below our character
        public virtual GameObject ObjectBelow { get; set; }

        /// the surface modifier object below our character (if any)
        // public virtual SurfaceModifier SurfaceModifierBelow { get; set; }

        public virtual Vector3 AppliedImpact => _impact;

        /// whether or not the character is on a moving platform
        // public virtual bool OnMovingPlatform { get; set; }

        /// the speed of the moving platform
        public virtual Vector3 MovingPlatformSpeed { get; set; }

        // the obstacle left to this controller (only updated if DetectObstacles is called)
        public virtual GameObject DetectedObstacleLeft { get; set; }

        // the obstacle right to this controller (only updated if DetectObstacles is called)
        public virtual GameObject DetectedObstacleRight { get; set; }

        // the obstacle up to this controller (only updated if DetectObstacles is called)
        public virtual GameObject DetectedObstacleUp { get; set; }

        // the obstacle down to this controller (only updated if DetectObstacles is called)
        public virtual GameObject DetectedObstacleDown { get; set; }

        // true if an obstacle was detected in any of the cardinal directions
        public virtual bool CollidingWithCardinalObstacle { get; set; }

        protected Vector3 _positionLastFrame { get; set; }
        protected Vector3 _lastPosition { get; set; }
        protected Vector3 _speedComputation;
        protected bool _groundedLastFrame;
        protected Vector3 _impact;
        protected const float _smallValue = 0.0001f;

        /// <summary>
        /// On awake, we initialize our current direction
        /// </summary>
        protected virtual void Awake()
        {
            Character = GetComponent<Character>();
            CurrentDirection = transform.forward;
            _lastPosition = transform.position;
        }

        /// <summary>
        /// On update, we check if we're grounded, and determine the direction
        /// </summary>
        protected virtual void Update()
        {
            CheckIfGrounded();
            HandleFriction();
            DetermineDirection();
        }

        /// <summary>
        /// Computes the speed
        /// </summary>
        protected virtual void ComputeSpeed()
        {
            if (Time.deltaTime != 0f)
            {
                Speed = (transform.position - _positionLastFrame) / Time.deltaTime;
            }

            // we round the speed to 2 decimals
            var x = Mathf.Round(Speed.x * 100f) / 100f;
            var y = Mathf.Round(Speed.y * 100f) / 100f;
            var z = Mathf.Round(Speed.z * 100f) / 100f;
            Speed = new Vector3(x, y, z);
            _positionLastFrame = transform.position;
        }

        /// <summary>
        /// Determines the controller's current direction
        /// </summary>
        protected abstract void DetermineDirection();

        /// <summary>
        /// Performs obstacle detection "manually"
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="offset"></param>
        public abstract void DetectObstacles(float distance, Vector3 offset);

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
        /// <param name="direction"></param>
        /// <param name="force"></param>
        public abstract void Impact(Vector3 direction, float force);

        /// <summary>
        /// Sets gravity active or inactive
        /// </summary>
        /// <param name="status"></param>
        public virtual void SetGravityActive(bool status)
        {
            GravityActive = status;
        }

        /// <summary>
        /// Adds the specified force to the controller
        /// </summary>
        /// <param name="movement"></param>
        public abstract void AddForce(Vector3 movement);

        /// <summary>
        /// Sets the current movement of the controller to the specified Vector3
        /// </summary>
        /// <param name="movement"></param>
        public abstract void SetMovement(Vector3 movement);

        /// <summary>
        /// Moves the controller to the specified position (in world space)
        /// </summary>
        /// <param name="newPosition"></param>
        public abstract void MovePosition(Vector3 newPosition);

        /// <summary>
        /// Set the controller to the specified position (in world space)
        /// </summary>
        /// <param name="newPosition"></param>
        public abstract void SetPosition(Vector3 newPosition);

        /// <summary>
        /// Resizes the controller's collider
        /// </summary>
        /// <param name="newHeight"></param>
        public abstract void ResizeColliderHeight(float newHeight, bool translateCenter = false);

        /// <summary>
        /// Resets the controller's collider size
        /// </summary>
        public abstract void ResetColliderSize();

        /// <summary>
        /// Returns true if the controller's collider can go back to original size without hitting an obstacle, false otherwise
        /// </summary>
        /// <returns></returns>
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
        /// Handles friction collisions
        /// </summary>
        protected abstract void HandleFriction();

        /// <summary>
        /// Resets all values for this controller
        /// </summary>
        public virtual void Reset()
        {
            _impact = Vector3.zero;
            GravityActive = true;
            Speed = Vector3.zero;
            Velocity = Vector3.zero;
            VelocityLastFrame = Vector3.zero;
            Acceleration = Vector3.zero;
            Grounded = true;
            JustGotGrounded = false;
            CurrentMovement = Vector3.zero;
            CurrentDirection = Vector3.zero;
            AddedForce = Vector3.zero;
        }
    }
}
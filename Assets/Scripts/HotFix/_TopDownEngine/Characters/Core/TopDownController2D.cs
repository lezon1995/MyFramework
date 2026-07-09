using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// a controller to move a rigidbody2D and collider2D around in top-down view
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Core/TopDown Controller 2D")]
    public class TopDownController2D : TopDownController
    {
        // [ShowInInspector]
        // [Tooltip("whether or not the character is above a hole right now")]
        // public bool OverHole { get; set; }

        public override Vector3 ColliderCenter => (Vector2)transform.position + ColliderOffset;
        public override Vector3 ColliderBottom => (Vector2)transform.position + ColliderOffset + Vector2.down * ColliderBounds.extents.y;
        public override Vector3 ColliderTop => (Vector2)transform.position + ColliderOffset + Vector2.up * ColliderBounds.extents.y;
        // public override bool OnMovingPlatform => _movingPlatform;

        public override Vector3 MovingPlatformSpeed
        {
            get
            {
                // if (_movingPlatform)
                    // return _movingPlatform.CurrentSpeed;

                return Vector3.zero;
            }
        }

        [Tooltip("the layer mask to consider as ground")]
        public LayerMask GroundLayerMask = LayerManager.Ground_Mask;

        // [Tooltip("the layer mask to consider as holes")]
        // public LayerMask HoleLayerMask = LayerManager.Hole_Mask;

        [Tooltip("the layer to consider as obstacles (will prevent movement)")]
        public LayerMask ObstaclesLayerMask = LayerManager.Obstacles_Mask;

        public Vector2 ColliderSize
        {
            get
            {
                if (_boxCollider) return _boxCollider.size;
                if (_capsuleCollider) return _capsuleCollider.size;
                if (_circleCollider) return Vector2.one * _circleCollider.radius;
                return Vector2.zero;
            }
            set
            {
                if (_boxCollider) _boxCollider.size = value;
                if (_capsuleCollider) _capsuleCollider.size = value;
                if (_circleCollider) _circleCollider.radius = value.x;
            }
        }

        public Vector2 ColliderOffset
        {
            get
            {
                if (_boxCollider) return _boxCollider.offset;
                if (_capsuleCollider) return _capsuleCollider.offset;
                if (_circleCollider) return _circleCollider.offset;
                return Vector2.zero;
            }
            set
            {
                if (_boxCollider) _boxCollider.offset = value;
                if (_capsuleCollider) _capsuleCollider.offset = value;
                if (_circleCollider) _circleCollider.offset = value;
            }
        }

        public Bounds ColliderBounds
        {
            get
            {
                if (_boxCollider) return _boxCollider.bounds;
                if (_capsuleCollider) return _capsuleCollider.bounds;
                if (_circleCollider) return _circleCollider.bounds;
                return new Bounds();
            }
        }

        protected Rigidbody2D _rigidBody;
        protected BoxCollider2D _boxCollider;
        protected CapsuleCollider2D _capsuleCollider;
        protected CircleCollider2D _circleCollider;
        protected Vector2 _originalColliderSize;
        protected Vector3 _originalColliderCenter;
        protected Vector3 _originalSizeRaycastOrigin;
        protected Vector3 _orientedMovement;
        protected Collider2D _groundedTest;
        // protected Collider2D _holeTestMin;
        // protected Collider2D _holeTestMax;
        // protected MovingPlatform2D _movingPlatform;
        protected Vector3 _movingPlatformPositionLastFrame;

        // collision detection
        protected RaycastHit2D _raycastUp;
        protected RaycastHit2D _raycastDown;
        protected RaycastHit2D _raycastLeft;
        protected RaycastHit2D _raycastRight;

        /// <summary>
        /// On awake we grab our components
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            _rigidBody = GetComponent<Rigidbody2D>();
            _boxCollider = GetComponent<BoxCollider2D>();
            _capsuleCollider = GetComponent<CapsuleCollider2D>();
            _circleCollider = GetComponent<CircleCollider2D>();
            _originalColliderSize = ColliderSize;
            _originalColliderCenter = ColliderOffset;
        }

        /// <summary>
        /// Determines whether or not this character is grounded
        /// </summary>
        protected override void CheckIfGrounded()
        {
            _groundedTest = Physics2D.OverlapPoint(transform.position, GroundLayerMask);
            // _holeTestMin = Physics2D.OverlapPoint(ColliderBounds.min, HoleLayerMask);
            // _holeTestMax = Physics2D.OverlapPoint(ColliderBounds.max, HoleLayerMask);
            Grounded = _groundedTest;
            // OverHole = _holeTestMin && _holeTestMax;
            JustGotGrounded = !_groundedLastFrame && Grounded;
            _groundedLastFrame = Grounded;
        }

        /// <summary>
        /// On update we determine our acceleration
        /// </summary>
        protected override void Update()
        {
            base.Update();
            Velocity = (_rigidBody.transform.position - _positionLastFrame) / Time.deltaTime;
            Acceleration = (Velocity - VelocityLastFrame) / Time.deltaTime;
        }

        /// <summary>
        /// On late update, we apply an impact
        /// </summary>
        protected override void LateUpdate()
        {
            base.LateUpdate();
            VelocityLastFrame = Velocity;
            ComputeSpeed();
        }

        /// <summary>
        /// Handles the friction, still a work in progress (todo)
        /// </summary>
        protected override void HandleFriction()
        {
            // if (SurfaceModifierBelow == null)
            {
                Friction = 0f;
                AddedForce = Vector3.zero;
            }
            // else
            // {
            //     Friction = SurfaceModifierBelow.Friction;
            //
            //     if (AddedForce.y != 0F)
            //     {
            //         AddForce(AddedForce);
            //     }
            //
            //     AddedForce = new Vector3(AddedForce.x, 0F, AddedForce.z);
            //     AddedForce = SurfaceModifierBelow.AddedForce;
            // }
        }

        /// <summary>
        /// On fixed update, we move our rigidbody 
        /// </summary>
        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            ApplyImpact();

            if (FreeMovement)
            {
                switch (Friction)
                {
                    case > 1:
                        CurrentMovement /= Friction;
                        break;
                    case > 0 and < 1:
                        // if we have a low friction (ice, marbles...) we lerp the speed accordingly
                        CurrentMovement = Vector3.Lerp(Speed, CurrentMovement, Time.fixedDeltaTime * Friction);
                        break;
                }

                Vector2 newMovement = _rigidBody.position + (Vector2)(CurrentMovement + AddedForce) * Time.fixedDeltaTime;

                // if (OnMovingPlatform)
                //     newMovement += (Vector2)_movingPlatform.CurrentSpeed * Time.fixedDeltaTime;

                _rigidBody.MovePosition(newMovement);
            }

            _lastPosition = transform.position;
        }

        /// <summary>
        /// Another way to add a force of the specified force and direction
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="force"></param>
        public override void Impact(Vector3 direction, float force)
        {
            direction = direction.normalized;
            _impact += direction.normalized * force;
        }

        /// <summary>
        /// Applies the current impact
        /// </summary>
        protected virtual void ApplyImpact()
        {
            if (_impact.magnitude > 0.2f)
            {
                _rigidBody.AddForce(_impact);
            }

            _impact = Vector3.Lerp(_impact, Vector3.zero, 5f * Time.deltaTime);
        }

        /// <summary>
        /// Adds a force of the specified vector
        /// </summary>
        /// <param name="movement"></param>
        public override void AddForce(Vector3 movement)
        {
            Impact(movement.normalized, movement.magnitude);
        }

        /// <summary>
        /// Sets the current movement
        /// </summary>
        /// <param name="movement"></param>
        public override void SetMovement(Vector3 movement)
        {
            movement.y = movement.z;
            movement.z = 0;
            _orientedMovement = movement;
            CurrentMovement = movement;
        }

        /// <summary>
        /// Tries to move to the specified position
        /// </summary>
        /// <param name="newPosition"></param>
        public override void MovePosition(Vector3 newPosition)
        {
            _rigidBody.MovePosition(newPosition);
        }

        public override void SetPosition(Vector3 newPosition)
        {
            _rigidBody.position = newPosition;
        }

        /// <summary>
        /// Resizes the collider to the new size set in parameters
        /// </summary>
        /// <param name="newHeight">New size.</param>
        /// <param name="translateCenter"></param>
        public override void ResizeColliderHeight(float newHeight, bool translateCenter = false)
        {
            float newYOffset = _originalColliderCenter.y - (_originalColliderSize.y - newHeight) / 2;
            Vector2 newSize = ColliderSize;
            newSize.y = newHeight;
            ColliderSize = newSize;
            ColliderOffset = newYOffset * Vector3.up;
        }

        /// <summary>
        /// Returns the collider to its initial size
        /// </summary>
        public override void ResetColliderSize()
        {
            ColliderSize = _originalColliderSize;
            ColliderOffset = _originalColliderCenter;
        }

        /// <summary>
        /// Determines the controller's current direction
        /// </summary>
        protected override void DetermineDirection()
        {
            if (CurrentMovement != Vector3.zero)
            {
                CurrentDirection = CurrentMovement.normalized;
            }
        }

        /// <summary>
        /// Sets a moving platform to this controller
        /// </summary>
        /// <param name="platform"></param>
        // public virtual void SetMovingPlatform(MovingPlatform2D platform)
        // {
        //     _movingPlatform = platform;
        // }

        /// <summary>
        /// Sets this rigidbody as kinematic
        /// </summary>
        /// <param name="state"></param>
        public override void SetKinematic(bool state)
        {
            _rigidBody.bodyType = state ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        }

        /// <summary>
        /// Enables the collider
        /// </summary>
        public override void CollisionsOn()
        {
            if (_boxCollider) _boxCollider.enabled = true;
            if (_capsuleCollider) _capsuleCollider.enabled = true;
            if (_circleCollider) _circleCollider.enabled = true;
        }

        /// <summary>
        /// Disables the collider
        /// </summary>
        public override void CollisionsOff()
        {
            if (_boxCollider) _boxCollider.enabled = false;
            if (_capsuleCollider) _capsuleCollider.enabled = false;
            if (_circleCollider) _circleCollider.enabled = false;
        }

        /// <summary>
        /// Performs a cardinal collision check and stores collision objects information
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="offset"></param>
        public override void DetectObstacles(float distance, Vector3 offset)
        {
            if (!PerformCardinalObstacleRaycastDetection)
                return;

            CollidingWithCardinalObstacle = false;
            _raycastRight = MMDebug.RayCast(transform.position + offset, Vector3.right, distance, ObstaclesLayerMask, Color.yellow, true);
            if (_raycastRight.collider)
            {
                DetectedObstacleRight = _raycastRight.collider.gameObject;
                CollidingWithCardinalObstacle = true;
            }
            else
            {
                DetectedObstacleRight = null;
            }

            _raycastLeft = MMDebug.RayCast(transform.position + offset, Vector3.left, distance, ObstaclesLayerMask, Color.yellow, true);
            if (_raycastLeft.collider)
            {
                DetectedObstacleLeft = _raycastLeft.collider.gameObject;
                CollidingWithCardinalObstacle = true;
            }
            else
            {
                DetectedObstacleLeft = null;
            }

            _raycastUp = MMDebug.RayCast(transform.position + offset, Vector3.up, distance, ObstaclesLayerMask, Color.yellow, true);
            if (_raycastUp.collider)
            {
                DetectedObstacleUp = _raycastUp.collider.gameObject;
                CollidingWithCardinalObstacle = true;
            }
            else
            {
                DetectedObstacleUp = null;
            }

            _raycastDown = MMDebug.RayCast(transform.position + offset, Vector3.down, distance, ObstaclesLayerMask, Color.yellow, true);
            if (_raycastDown.collider)
            {
                DetectedObstacleDown = _raycastDown.collider.gameObject;
                CollidingWithCardinalObstacle = true;
            }
            else
            {
                DetectedObstacleDown = null;
            }
        }


        /// <summary>
        /// On reset, we reset our rb's velocity
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            if (_rigidBody)
            {
                _rigidBody.linearVelocity = Vector2.zero;
            }
        }
    }
}